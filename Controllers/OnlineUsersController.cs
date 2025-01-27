using System.Web.Mvc;
using Microsoft.AspNet.SignalR;
using MZDNETWORK.Hubs;

namespace MZDNETWORK.Controllers
{
    public class OnlineUsersController : Controller
    {
        private static int onlineUsers = 0;
        private readonly IHubContext _hubContext;

        public OnlineUsersController()
        {
            _hubContext = GlobalHost.ConnectionManager.GetHubContext<OnlineUsersHub>();
        }

        public ActionResult Index()
        {
            // Kullanıcı giriş yaptığında onlineUsers sayısını artırın
            onlineUsers++;
            ViewBag.OnlineUsers = onlineUsers;
            return View();
        }

        public ActionResult Logout()
        {
            // Kullanıcı çıkış yaptığında onlineUsers sayısını azaltın
            onlineUsers--;
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult GetOnlineUsers()
        {
            // Online kullanıcı sayısını almak için bir yöntem ekleyin
            return Json(new { onlineUsers = onlineUsers }, JsonRequestBehavior.AllowGet);
        }
    }
}