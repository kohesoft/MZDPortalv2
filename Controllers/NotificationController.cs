using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.SignalR;
using MZDNETWORK.Models;
using AuthorizeAttribute = System.Web.Mvc.AuthorizeAttribute;
using Ganss.Xss;

public class NotificationController : Controller
{
    private readonly MZDNETWORKContext db;
    [Authorize(Roles = "IK, Yonetici, Sys, IdariIsler, BilgiIslem")]

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

    [Authorize(Roles = "IK, Yonetici, Sys, IdariIsler, BilgiIslem")]

    [HttpPost]
    [ValidateInput(false)]
    public ActionResult SendNotification(string message, int duration = 4000, string color = "info", bool playSound = false, string sound = null)
    {
        var sanitizer = new HtmlSanitizer();
        var safeMessage = sanitizer.Sanitize(message);
        _hubContext.Clients.All.showNotification(safeMessage, duration, color, playSound, sound);
        return new HttpStatusCodeResult(200);
    }
    [Authorize(Roles = "IK, Yonetici, Sys, IdariIsler, BilgiIslem,Lider,Merkez,Yerleske, Dokumantasyon")]

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
    [Authorize(Roles = "IK, Yonetici, Sys, IdariIsler, BilgiIslem,Lider,Merkez,Yerleske, Dokumantasyon")]

    [HttpGet]
    public ActionResult GetUnreadNotifications()
    {
        var username = User.Identity.Name;
        var userId = db.Users.SingleOrDefault(u => u.Username == username)?.Id.ToString();
        var notifications = db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .Select(n => new { n.Id, n.Message })
            .ToList();
        return Json(notifications, JsonRequestBehavior.AllowGet);
    }
}
