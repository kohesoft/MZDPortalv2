using System.Web.Mvc;
using Microsoft.AspNet.SignalR;
using MZDNETWORK.Hubs;

namespace MZDNETWORK.Controllers
{
    public class ChatController : Controller
    {
        private readonly IHubContext _hubContext;

        public ChatController()
        {
            _hubContext = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
        }

        [HttpPost]
        public ActionResult SendMessage(string user, string message)
        {
            _hubContext.Clients.All.broadcastMessage(user, message);
            return new HttpStatusCodeResult(200);
        }

    }
}