using System.Web.Mvc;
using Microsoft.AspNet.SignalR;
using AuthorizeAttribute = System.Web.Mvc.AuthorizeAttribute;

public class NotificationController : Controller
{
    [Authorize(Roles = "IK, Yonetici, Sys, BilgiIslem, IdariIsler")]
    public ActionResult SendNotification()
    {
        return View();
    }

    private readonly IHubContext _hubContext;

    public NotificationController()
    {
        _hubContext = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
    }

    [HttpPost]
    [Authorize(Roles = "IK, Yonetici, Sys, BilgiIslem, IdariIsler")]
    public ActionResult SendNotification(string message)
    {
        _hubContext.Clients.All.showNotification(message);
        return new HttpStatusCodeResult(200);
    }
}
