using System.Web.Mvc;
using NLog;

public class LogActionFilter : ActionFilterAttribute
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        var userName = filterContext.HttpContext.User.Identity.Name;
        var controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
        var actionName = filterContext.ActionDescriptor.ActionName;
        var message = $"User {userName} is executing {controllerName}/{actionName}";

        Logger.Info(message);

        base.OnActionExecuting(filterContext);
    }

    public override void OnActionExecuted(ActionExecutedContext filterContext)
    {
        var userName = filterContext.HttpContext.User.Identity.Name;
        var controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
        var actionName = filterContext.ActionDescriptor.ActionName;
        var message = $"User {userName} executed {controllerName}/{actionName}";

        Logger.Info(message);

        base.OnActionExecuted(filterContext);
    }
}
