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


[assembly: OwinStartup(typeof(MZDNETWORK.Startup))]

namespace MZDNETWORK
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected void Application_Start()
        {
            Logger.Info("Application started");
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // **YENİ: Dynamic Permission System Seeding**
            try
            {
                Logger.Info("Starting database seeding...");
                MZDNETWORKContext.SeedDatabase();
                Logger.Info("Database seeding completed successfully");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Database seeding failed");
                // Production'da bu hata nedeniyle uygulama çökmemeli
                // Sadece log'a kaydedip devam ediyoruz
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
                            
                            // Multiple roles desteği için RoleHelper kullan
                            string[] roles;
                            if (int.TryParse(userId, out int userIdInt))
                            {
                                roles = MZDNETWORK.Helpers.RoleHelper.GetUserRoles(userIdInt);
                                if (roles.Length == 0)
                                {
                                    // Backward compatibility - eski userData formatı kontrol et
                                    if (userData.Length >= 3)
                                    {
                                        roles = new[] { userData[2] }; // Eski role format
                                    }
                                    else
                                    {
                                        roles = new[] { "Default" }; // Fallback
                                    }
                                }
                            }
                            else
                            {
                                // Eski format: username|role
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
            Logger.Info("Application_Error called");
            Exception exception = Server.GetLastError();
            if (exception != null)
            {
                Logger.Error(exception, $"Unhandled exception, URL: {HttpContext.Current.Request.Url}, User: {HttpContext.Current.User?.Identity?.Name ?? "Anonymous"}");
            }
            else
            {
                Logger.Warn("Exception is null");
            }
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            SessionTracker.Increment();
        }

        protected void Session_End(object sender, EventArgs e)
        {
            SessionTracker.Decrement();
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            MZDNETWORK.Helpers.HourlyRequestCounter.Increment();
        }
    }

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}
