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
                            var role = userData[1];
                            var roles = new[] { role };

                            HttpContext.Current.User = new System.Security.Principal.GenericPrincipal(formsIdentity, roles);
                            Logger.Info($"User authenticated: {username}, Role: {role}, IP: {HttpContext.Current.Request.UserHostAddress}, URL: {HttpContext.Current.Request.Url}");
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
    }

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}
