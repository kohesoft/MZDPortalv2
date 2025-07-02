using System;
using System.Linq;
using System.Web.Mvc;
using MZDNETWORK.Data;
using MZDNETWORK.Models;
using MZDNETWORK.Attributes;

namespace MZDNETWORK.Controllers
{
    // Only administrators should access this controller – adjust permission/role as needed.
    [DynamicAuthorize(Permission = "UserManagement.PasswordResetRequests")]
    public class PasswordResetRequestController : Controller
    {
        private readonly MZDNETWORKContext db;

        public PasswordResetRequestController()
        {
            db = new MZDNETWORKContext();
        }

        // GET: PasswordResetRequest
        public ActionResult Index()
        {
            var requests = db.PasswordResetRequests
                .OrderByDescending(r => r.RequestedAt)
                .ToList();
            return View(requests);
        }

        // POST: PasswordResetRequest/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Approve(int id)
        {
            var request = db.PasswordResetRequests.FirstOrDefault(r => r.Id == id);
            if (request == null)
            {
                return HttpNotFound();
            }

            if (!request.Approved)
            {
                request.Approved = true;
                request.ApprovedAt = DateTime.Now;
                if (User.Identity.IsAuthenticated)
                {
                    var adminUser = db.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
                    request.ApprovedByUserId = adminUser?.Id;
                }
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
} 