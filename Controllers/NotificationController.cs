using System.Web.Mvc;
using Microsoft.AspNet.SignalR;
using AuthorizeAttribute = System.Web.Mvc.AuthorizeAttribute;

[Authorize(Roles = "IK, Yonetici, Sys, IdariIsler, BilgiIslem")]
public class NotificationController : Controller
{

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
    public ActionResult SendNotification(string message)
    {
        _hubContext.Clients.All.showNotification(message);
        return new HttpStatusCodeResult(200);
    }
}
