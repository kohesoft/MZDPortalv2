using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.SignalR;
using MZDNETWORK.Models;
using AuthorizeAttribute = System.Web.Mvc.AuthorizeAttribute;

[Authorize(Roles = "IK, Yonetici, Sys, IdariIsler, BilgiIslem")]
public class NotificationController : Controller
{
    private readonly MZDNETWORKContext db;

    public ActionResult SendNotification()
    {
        return View();
    }

    private readonly IHubContext _hubContext;

    public NotificationController()
    {
        _hubContext = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
        db = new MZDNETWORKContext();

    }


    [HttpPost]
    public ActionResult SendNotification(string message)
    {
        _hubContext.Clients.All.showNotification(message);
        return new HttpStatusCodeResult(200);
    }

    [HttpPost]
    public ActionResult MarkAsRead(int id)
    {
        var notification = db.Notifications.SingleOrDefault(n => n.Id == id);
        if (notification != null)
        {
            notification.IsRead = true;
            db.SaveChanges();
        }
        return Json(new { success = true });
    }
}
