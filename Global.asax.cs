using System.Web;
using System;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using NLog;
using Microsoft.Owin;
using Owin;
using WebSocketSharp.Server;
using System.Web.Http;
using MZDNETWORK.Helpers;
using MZDNETWORK.Data;
using OfficeOpenXml;
using Hangfire;
using Hangfire.SqlServer;
using Hangfire.Dashboard;
using System.Configuration;


[assembly: OwinStartup(typeof(MZDNETWORK.Startup))]

namespace MZDNETWORK
{
    // Hangfire Dashboard iÃ§in yetkilendirme filtresi
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            // HttpContext'e System.Web Ã¼zerinden eriÅŸ
            var httpContext = System.Web.HttpContext.Current;
            
            if (httpContext == null || httpContext.User == null || !httpContext.User.Identity.IsAuthenticated)
            {
                return false;
            }

            // Sadece admin kullanÄ±cÄ±lar eriÅŸebilir
            try
            {
                return Attributes.DynamicAuthorizeAttribute.CurrentUserHasPermission("Operasyon.ToplantiOdasi", "Manage");
            }
            catch
            {
                return false;
            }
        }
    }

    public class MvcApplication : System.Web.HttpApplication
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected void Application_Start()
        {
            try
            {
                Logger.Info("Application starting...");
                AreaRegistration.RegisterAllAreas();
                FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
                RouteConfig.RegisterRoutes(RouteTable.Routes);
                BundleConfig.RegisterBundles(BundleTable.Bundles);

                // EPPlus 5+ gereÄŸi lisans baÄŸlamÄ±nÄ± belirle
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                // Seed
                try
                {
                    Logger.Info("Starting database seeding...");
                    MZDNETWORKContext.SeedDatabase();
                    Logger.Info("Database seeding completed successfully");
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Database seeding failed");
                }

                Logger.Info("Application started successfully.");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, "A fatal error occurred during Application_Start, causing the application to shut down.");
                throw;
            }
        }
      
        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            Logger.Info("Application_AuthenticateRequest called");
            if (HttpContext.Current.User != null && HttpContext.Current.User.Identity.IsAuthenticated)
            {
                var formsIdentity = HttpContext.Current.User.Identity as FormsIdentity;
                if (formsIdentity != null)
                {
                    try
                    {
                        var ticket = formsIdentity.Ticket;
                        var userData = ticket.UserData.Split('|');
                        if (userData.Length >= 2)
                        {
                            var username = userData[0];
                            var userId = userData[1];
                            
                            // Multiple roles desteÄŸi iÃ§in RoleHelper kullan
                            string[] roles;
                            if (int.TryParse(userId, out int userIdInt))
                            {
                                roles = MZDNETWORK.Helpers.RoleHelper.GetUserRoles(userIdInt);
                                if (roles.Length == 0)
                                {
                                    if (userData.Length >= 3)
                                    {
                                        roles = new[] { userData[2] };
                                    }
                                    else
                                    {
                                        roles = new[] { "Default" };
                                    }
                                }
                            }
                            else
                            {
                                var role = userData[1];
                                roles = new[] { role };
                            }

                            HttpContext.Current.User = new System.Security.Principal.GenericPrincipal(formsIdentity, roles);
                            Logger.Info($"User authenticated: {username}, Roles: {string.Join(", ", roles)}, IP: {HttpContext.Current.Request.UserHostAddress}, URL: {HttpContext.Current.Request.Url}");
                        }
                        else
                        {
                            Logger.Warn($"Invalid userData format: {ticket.UserData}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Error processing authentication ticket");
                    }
                }
                else
                {
                    Logger.Warn("FormsIdentity is null");
                }
            }
            else
            {
                Logger.Warn("User is not authenticated or HttpContext.Current.User is null");
            }
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception exception = Server.GetLastError();
            if (exception != null)
            {
                if (IsIgnorableException(exception))
                {
                    Logger.Debug(exception, $"Ignorable exception, URL: {HttpContext.Current?.Request?.Url}, User: {HttpContext.Current?.User?.Identity?.Name ?? "Anonymous"}");
                    Server.ClearError();
                    return;
                }

                Logger.Error(exception, $"Unhandled exception, URL: {HttpContext.Current?.Request?.Url}, User: {HttpContext.Current?.User?.Identity?.Name ?? "Anonymous"}");
            }
            else
            {
                Logger.Warn("Exception is null in Application_Error");
            }
        }

        private bool IsIgnorableException(Exception exception)
        {
            if (exception is HttpException httpEx)
            {
                // Ignore client disconnect errors (0x800704CD is ERROR_CONNECTION_ABORTED)
                if (httpEx.ErrorCode == -2147023667) // 0x800704CD
                {
                    return true;
                }
                
                // Ignore other common client-side errors
                if (httpEx.GetHttpCode() == 404 || httpEx.GetHttpCode() == 400)
                {
                    return true;
                }
            }

            // Check if it's a SignalR ping timeout or similar
            if (exception.Message.Contains("ping") || 
                exception.Message.Contains("Connection aborted") ||
                exception.Message.Contains("Uzak ana bilgisayar baÄŸlantÄ±yÄ± kapattÄ±"))
            {
                return true;
            }

            return false;
        }

        protected void Session_Start(object sender, EventArgs e) { }
        protected void Session_End(object sender, EventArgs e) { }
        protected void Application_BeginRequest(object sender, EventArgs e) { }

        protected void Application_End()
        {
            try
            {
                Logger.Info("Application ending...");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error during Application_End");
            }
        }
    }

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // SignalR
            app.MapSignalR("/signalr", new Microsoft.AspNet.SignalR.HubConfiguration()
            {
                EnableDetailedErrors = true,
                EnableJavaScriptProxies = true
            });

            // Hangfire (conditional)
            bool hangfireEnabled = string.Equals(ConfigurationManager.AppSettings["Hangfire_Enabled"], "true", StringComparison.OrdinalIgnoreCase);
            if (hangfireEnabled)
            {
                var logger = LogManager.GetCurrentClassLogger();
                try
                {
                    var connectionString = ConfigurationManager.ConnectionStrings["MZDNETWORKContext"].ConnectionString;
                    Hangfire.GlobalConfiguration.Configuration.UseSqlServerStorage(connectionString, new SqlServerStorageOptions
                    {
                        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                        QueuePollInterval = TimeSpan.Zero,
                        UseRecommendedIsolationLevel = true,
                        DisableGlobalLocks = true
                    });

                    if (string.Equals(ConfigurationManager.AppSettings["Hangfire_Dashboard_Enabled"], "true", StringComparison.OrdinalIgnoreCase))
                    {
                        var dashboardOptions = new DashboardOptions
                        {
                            Authorization = new[] { new HangfireAuthorizationFilter() },
                            AppPath = "/", // Ana sayfaya dÃ¶n butonu iÃ§in
                            DisplayStorageConnectionString = false
                        };
                        app.UseHangfireDashboard("/hangfire", dashboardOptions);
                    }

                    app.UseHangfireServer();
                    logger.Info("Hangfire server started");
                    
                    // Recurring job'larÄ± server baÅŸladÄ±ktan sonra tanÄ±mla
                    RecurringJob.AddOrUpdate("daily-overtime-cleanup", () => OvertimeServiceScheduler.ResetOvertimeData(null), Cron.Daily);
                    RecurringJob.AddOrUpdate("hourly-connection-cleanup", () => ConnectionPoolManager.ClearConnectionPools(), Cron.Hourly);
                    RecurringJob.AddOrUpdate("meeting-reminders", () => new MeetingReminderService().CheckAndSendReminders(), "*/5 * * * *");
                    logger.Info("Hangfire recurring jobs configured");
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Failed to start Hangfire");
                }
            }
            else
            {
                var logger = LogManager.GetCurrentClassLogger();
                logger.Info("Hangfire is disabled by configuration (Hangfire_Enabled=false)");
            }
        }
    }
}

