using System.Web.Mvc;
using Microsoft.AspNet.SignalR;
using MZDNETWORK.Hubs;

namespace MZDNETWORK.Controllers
{
    [AllowAnonymous]
    public class OnlineUsersController : Controller
    {
        private readonly IHubContext _hubContext;

        public OnlineUsersController()
        {
            _hubContext = GlobalHost.ConnectionManager.GetHubContext<OnlineUsersHub>();
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Logout()
        {
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult GetOnlineUsers()
        {
            // OnlineUsersHub sınıfındaki GetOnlineUsersCount metodunu çağır
            int onlineUsersCount = OnlineUsersHub.GetOnlineUsersCount();
            return Json(new { onlineUsers = onlineUsersCount }, JsonRequestBehavior.AllowGet);
        }
    }
}