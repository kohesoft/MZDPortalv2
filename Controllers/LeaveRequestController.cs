using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using MZDNETWORK.Models;
using MZDNETWORK.Data;
using MZDNETWORK.Hubs;
using Microsoft.AspNet.SignalR;
using AuthorizeAttribute = System.Web.Mvc.AuthorizeAttribute;
using MZDNETWORK.Attributes;

namespace MZDNETWORK.Controllers
{
    [DynamicAuthorize(Permission = "Operational.LeaveRequest")]
    public class LeaveRequestController : Controller
    {
        private MZDNETWORKContext db = new MZDNETWORKContext();
        private IHubContext notificationHubContext = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();

        // GET: LeaveRequest
        public async Task<ActionResult> Index()
        {
            var username = User.Identity.Name;
            var userId = db.Users
                .Where(u => u.Username == username)
                .Select(u => u.Id)
                .FirstOrDefault();
            var requests = await db.LeaveRequests
                .Include(l => l.RequestingUser)
                .Include(l => l.ApprovedBy)
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            return View(requests);
        }

        // GET: LeaveRequest/Create
        [DynamicAuthorize(Permission = "Operational.LeaveRequest", Action = "Create")]
        public ActionResult Create()
        {
            PrepareSelectLists();
            return View(new LeaveRequest { Status = LeaveStatus.Pending });
        }

        private void PrepareSelectLists()
        {
            var departments = db.Users
                .Where(u => !string.IsNullOrEmpty(u.Department))
                .Select(u => u.Department)
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            var substitutes = db.Users
                .Select(u => new {
                    FullName = (u.Name + " " + u.Surname).Trim(),
                    u.Id
                })
                .OrderBy(u => u.FullName)
                .ToList();

            ViewBag.Departments = new SelectList(departments);
            ViewBag.Substitutes = new SelectList(substitutes, "FullName", "FullName"); // value and text both name
        }

        // POST: LeaveRequest/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DynamicAuthorize(Permission = "Operational.LeaveRequest", Action = "Create")]
        public async Task<ActionResult> Create(LeaveRequest leaveRequest)
        {
            if (ModelState.IsValid)
            {
                var username = User.Identity.Name;
                var currentUserId = db.Users
                    .Where(u => u.Username == username)
                    .Select(u => u.Id)
                    .FirstOrDefault();

                leaveRequest.UserId = currentUserId;
                leaveRequest.Status = LeaveStatus.Pending;
                leaveRequest.CreatedAt = DateTime.Now;

                db.LeaveRequests.Add(leaveRequest);
                await db.SaveChangesAsync();

                // Notify HR/Admin users - Yeni UserRoles sistemi kullan
                var hrRoleIds = db.Roles
                    .Where(r => r.Name == "IK" || r.Name == "Yonetici" || r.Name == "Sys" || r.Name == "IdariIsler")
                    .Select(r => r.Id)
                    .ToList();

                var hrUserIds = db.UserRoles
                    .Where(ur => hrRoleIds.Contains(ur.RoleId))
                    .Select(ur => ur.UserId)
                    .Distinct()
                    .ToList();

                var hrUsers = await db.Users
                    .Where(u => hrUserIds.Contains(u.Id))
                    .ToListAsync();

                foreach (var hrUser in hrUsers)
                {
                    var notification = new Notification
                    {
                        UserId = hrUser.Id.ToString(),
                        Message = $"{User.Identity.Name} tarafından yeni bir izin talebi oluşturuldu.",
                        CreatedDate = DateTime.Now,
                        IsRead = false
                    };

                    db.Notifications.Add(notification);
                    await notificationHubContext.Clients.User(hrUser.Id.ToString()).newNotification(notification);
                }

                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            PrepareSelectLists();
            return View(leaveRequest);
        }

        // GET: LeaveRequest/Review/5
        [DynamicAuthorize(Permission = "Operational.LeaveRequest", Action = "Edit")]
        public async Task<ActionResult> Review(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            var leaveRequest = await db.LeaveRequests
                .Include(l => l.RequestingUser)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (leaveRequest == null)
            {
                return HttpNotFound();
            }

            var currentUserId = db.Users
                .Where(u => u.Username == User.Identity.Name)
                .Select(u => u.Id)
                .FirstOrDefault();

            if (leaveRequest.UserId != currentUserId && !User.IsInRole("IK") && !User.IsInRole("Yonetici") && !User.IsInRole("IdariIsler") && !User.IsInRole("Sys"))
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Unauthorized);
            }

            return View(leaveRequest);
        }

        // POST: LeaveRequest/Review/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DynamicAuthorize(Permission = "Operational.LeaveRequest", Action = "Edit")]
        public async Task<ActionResult> Review(int id, LeaveStatus status, string approvalReason)
        {
            var leaveRequest = await db.LeaveRequests.FindAsync(id);
            if (leaveRequest == null)
            {
                return HttpNotFound();
            }

            var currentUserId = db.Users
                .Where(u => u.Username == User.Identity.Name)
                .Select(u => u.Id)
                .FirstOrDefault();

            if (leaveRequest.UserId != currentUserId && !User.IsInRole("IK") && !User.IsInRole("Yonetici") && !User.IsInRole("IdariIsler") && !User.IsInRole("Sys"))
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Unauthorized);
            }

            leaveRequest.Status = status;
            leaveRequest.ApprovalReason = approvalReason;
            var approverName = User.Identity.Name;
            var approverId = db.Users
                .Where(u => u.Username == approverName)
                .Select(u => u.Id)
                .FirstOrDefault();
            leaveRequest.ApprovedById = approverId;
            leaveRequest.UpdatedAt = DateTime.Now;

            db.Entry(leaveRequest).State = EntityState.Modified;
            await db.SaveChangesAsync();

            // Notify the requesting user
            await notificationHubContext.Clients.User(leaveRequest.UserId.ToString()).addNotification(
                new
                {
                    title = "İzin Talebi Güncellendi",
                    message = $"İzin talebiniz {status.ToString()} durumuna güncellenmiştir.",
                    url = Url.Action("Details", "LeaveRequest", new { id = leaveRequest.Id }, Request.Url.Scheme)
                });

            return RedirectToAction("AdminDashboard");
        }

        // GET: LeaveRequest/AdminDashboard
        [DynamicAuthorize(Permission = "Operational.LeaveRequest", Action = "View")]
        public async Task<ActionResult> AdminDashboard()
        {
            var requests = await db.LeaveRequests
                .Include(l => l.RequestingUser)
                .Include(l => l.ApprovedBy)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            return View(requests);
        }

        // GET: LeaveRequest/Details/5
        [DynamicAuthorize(Permission = "Operational.LeaveRequest", Action = "View")]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            var leaveRequest = await db.LeaveRequests
                .Include(l => l.RequestingUser)
                .Include(l => l.ApprovedBy)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (leaveRequest == null)
            {
                return HttpNotFound();
            }

            var currentUserId = db.Users
                .Where(u => u.Username == User.Identity.Name)
                .Select(u => u.Id)
                .FirstOrDefault();

            if (leaveRequest.UserId != currentUserId && !User.IsInRole("IK") && !User.IsInRole("Yonetici") && !User.IsInRole("IdariIsler") && !User.IsInRole("Sys"))
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Unauthorized);
            }

            return View(leaveRequest);
        }

        // GET: LeaveRequest/GetSubstitutesByDepartment
        [DynamicAuthorize(Permission = "Operational.LeaveRequest", Action = "View")]
        public JsonResult GetSubstitutesByDepartment(string department)
        {
            var substitutes = db.Users
                .Where(u => u.Department == department)
                .Select(u => new {
                    FullName = (u.Name + " " + u.Surname).Trim(),
                    u.Id
                })
                .OrderBy(u => u.FullName)
                .ToList();

            return Json(substitutes, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
} 