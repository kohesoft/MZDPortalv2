using System.Web.Mvc;
using Microsoft.AspNet.SignalR;
using MZDNETWORK.Hubs;
using MZDNETWORK.Attributes;

namespace MZDNETWORK.Controllers
{
    [DynamicAuthorize(Permission = "SystemManagement.OnlineUsers")]
    public class OnlineUsersController : Controller
    {
        private readonly IHubContext _hubContext;

        public OnlineUsersController()
        {
            _hubContext = GlobalHost.ConnectionManager.GetHubContext<OnlineUsersHub>();
        }

        [DynamicAuthorize(Permission = "SystemManagement.OnlineUsers")]
        public ActionResult Index()
        {
            return View();
        }

        [DynamicAuthorize(Permission = "SystemManagement.OnlineUsers")]
        public ActionResult Logout()
        {
            return RedirectToAction("Index");
        }

        [DynamicAuthorize(Permission = "SystemManagement.OnlineUsers")]
        [HttpGet]
        public ActionResult GetOnlineUsers()
        {
            // OnlineUsersHub sınıfındaki GetOnlineUsersCount metodunu çağır
            int onlineUsersCount = OnlineUsersHub.GetOnlineUsersCount();
            return Json(new { onlineUsers = onlineUsersCount }, JsonRequestBehavior.AllowGet);
        }
    }
}