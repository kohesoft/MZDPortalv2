using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MZDNETWORK.Models;

namespace MZDNETWORK.Controllers
{
    [AllowAnonymous]
    public class ContactController : Controller
    {
        private MZDNETWORKContext db = new MZDNETWORKContext();

        public ActionResult Index()
        {
            var users = db.Users.ToList();
            return View(users);
        }
    }
}

