using System.Web.Mvc;
using Microsoft.AspNet.SignalR;
using MZDNETWORK.Hubs;
using AuthorizeAttribute = System.Web.Mvc.AuthorizeAttribute;

namespace MZDNETWORK.Controllers
{
    [Authorize(Roles = "IK, Yonetici, Sys, BilgiIslem, IdariIsler")]
    public class ChatController : Controller
    {
        private readonly IHubContext _hubContext;

        public ChatController()
        {
            _hubContext = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
        }

        [HttpPost]
        [Authorize(Roles = "IK, Yonetici, Sys, BilgiIslem, IdariIsler")]
        public ActionResult SendMessage(string user, string message)
        {
            _hubContext.Clients.All.broadcastMessage(user, message);
            return new HttpStatusCodeResult(200);
        }

    }
}