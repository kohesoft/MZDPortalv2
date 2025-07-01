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
using OfficeOpenXml;

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

            var query = db.LeaveRequests
                .Include(l => l.RequestingUser)
                .Include(l => l.ApprovedBy)
                .Where(l => l.UserId == userId);

            // --- Filters from query string ---
            var searchTerm = Request.QueryString["searchTerm"];
            var leaveTypeStr = Request.QueryString["leaveType"];
            var statusStr = Request.QueryString["status"];
            var dateRange = Request.QueryString["dateRange"];

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(l =>
                    l.Reason.Contains(searchTerm) ||
                    l.Department.Contains(searchTerm) ||
                    l.SubstituteName.Contains(searchTerm));
            }

            if (!string.IsNullOrWhiteSpace(leaveTypeStr))
            {
                if (Enum.TryParse(leaveTypeStr, out LeaveType lt))
                {
                    query = query.Where(l => l.LeaveType == lt);
                }
            }

            if (!string.IsNullOrWhiteSpace(statusStr))
            {
                if (Enum.TryParse(statusStr, out LeaveStatus st))
                {
                    query = query.Where(l => l.Status == st);
                }
                else if(statusStr == "Pending")
                {
                    query = query.Where(l => l.Status == LeaveStatus.PendingSupervisor || l.Status == LeaveStatus.PendingManager);
                }
            }

            if (!string.IsNullOrWhiteSpace(dateRange))
            {
                DateTime from, to;
                var today = DateTime.Today;
                switch (dateRange)
                {
                    case "thisMonth":
                        from = new DateTime(today.Year, today.Month, 1);
                        to = from.AddMonths(1);
                        break;
                    case "lastMonth":
                        var lastMonth = today.AddMonths(-1);
                        from = new DateTime(lastMonth.Year, lastMonth.Month, 1);
                        to = from.AddMonths(1);
                        break;
                    case "thisYear":
                        from = new DateTime(today.Year, 1, 1);
                        to = from.AddYears(1);
                        break;
                    case "lastYear":
                        from = new DateTime(today.Year - 1, 1, 1);
                        to = from.AddYears(1);
                        break;
                    default:
                        from = DateTime.MinValue;
                        to = DateTime.MaxValue;
                        break;
                }
                query = query.Where(l => l.StartDate >= from && l.StartDate < to);
            }

            var requests = await query.OrderByDescending(l => l.CreatedAt).ToListAsync();

            return View(requests);
        }

        // GET: LeaveRequest/Create
        [DynamicAuthorize(Permission = "Operational.LeaveRequest", Action = "Create")]
        public ActionResult Create()
        {
            PrepareSelectLists();
            return View(new LeaveRequest { Status = LeaveStatus.PendingSupervisor });
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
                // Yeni iş akışı: Talep oluşturulduğunda ilk olarak Şef onayı bekler
                leaveRequest.Status = LeaveStatus.PendingSupervisor;
                leaveRequest.CreatedAt = DateTime.Now;

                db.LeaveRequests.Add(leaveRequest);
                await db.SaveChangesAsync();

                // Onaylayıcı (Approve veya Manage) yetkisine sahip kullanıcıları bul
                var approvePermissionPath = "Operational.LeaveRequest";

                // Öncelikle Approve yetkili roller
                var approverRoleIds = db.RolePermissions
                    .Where(rp => rp.PermissionNode.Path == approvePermissionPath && rp.CanApprove)
                    .Select(rp => rp.RoleId)
                    .Distinct()
                    .ToList();

                // Bu rollere sahip kullanıcılar
                var notifyUserIds = db.UserRoles
                    .Where(ur => approverRoleIds.Contains(ur.RoleId))
                    .Select(ur => ur.UserId)
                    .Distinct()
                    .ToList();

                var notifyUsers = await db.Users
                    .Where(u => notifyUserIds.Contains(u.Id))
                    .ToListAsync();

                foreach (var targetUser in notifyUsers)
                {
                    var notification = new Notification
                    {
                        UserId = targetUser.Id.ToString(),
                        Message = $"{User.Identity.Name} tarafından yeni bir izin talebi oluşturuldu.",
                        CreatedDate = DateTime.Now,
                        IsRead = false
                    };

                    db.Notifications.Add(notification);
                    await notificationHubContext.Clients.User(targetUser.Id.ToString()).newNotification(notification);
                }

                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            PrepareSelectLists();
            return View(leaveRequest);
        }

        // GET: LeaveRequest/Review/5
        [DynamicAuthorize(Permission = "Operational.LeaveRequest", Action = "Approve")]
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

            bool canApprove = DynamicAuthorizeAttribute.CurrentUserHasPermission("Operational.LeaveRequest", "Approve");
            bool canManage = DynamicAuthorizeAttribute.CurrentUserHasPermission("Operational.LeaveRequest", "Manage");

            if (leaveRequest.UserId != currentUserId && !canApprove && !canManage)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Unauthorized);
            }

            return View(leaveRequest);
        }

        // POST: LeaveRequest/Review/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DynamicAuthorize(Permission = "Operational.LeaveRequest", Action = "Approve")]
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

            const string approvePermissionPath = "Operational.LeaveRequest";
            bool canApprove = DynamicAuthorizeAttribute.CurrentUserHasPermission(approvePermissionPath, "Approve");
            bool canManage = DynamicAuthorizeAttribute.CurrentUserHasPermission(approvePermissionPath, "Manage");

            if (leaveRequest.UserId != currentUserId && !canApprove && !canManage)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Unauthorized);
            }

            // Mevcut durum ve kullanıcının izinlerine göre iş akışını yönet
            var previousStatus = leaveRequest.Status;

            // Kullanıcı izinleri
            bool isSupervisor = canApprove && !canManage; // Sadece approve yetkisi
            bool isManager = canManage; // Manage yetkisi (daha yüksek)

            bool updateAllowed = false;

            // Şef aşaması
            if (previousStatus == LeaveStatus.PendingSupervisor)
            {
                if (!isSupervisor)
                    return new HttpStatusCodeResult(System.Net.HttpStatusCode.Unauthorized);

                if (status == LeaveStatus.Rejected)
                {
                    leaveRequest.Status = LeaveStatus.Rejected;
                }
                else
                {
                    // Tüm olumlu kararlar müdüre gider
                    leaveRequest.Status = LeaveStatus.PendingManager;

                    // Manage yetkisine sahip kullanıcıları bilgilendir (bildirim bloğu yukarıda mevcut)
                }

                updateAllowed = true;
            }
            // Müdür aşaması
            else if (previousStatus == LeaveStatus.PendingManager)
            {
                if (!isManager)
                    return new HttpStatusCodeResult(System.Net.HttpStatusCode.Unauthorized);

                // Müdür son kararı verir
                if (status == LeaveStatus.Approved)
                    leaveRequest.Status = LeaveStatus.Approved;
                else if (status == LeaveStatus.ConditionallyApproved)
                    leaveRequest.Status = LeaveStatus.ConditionallyApproved;
                else if (status == LeaveStatus.Rejected)
                    leaveRequest.Status = LeaveStatus.Rejected;

                updateAllowed = true;
            }

            if (!updateAllowed)
            {
                // Diğer durumları değiştirmeye izin verme
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

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

            // Talep sahibi bilgilendirme
            await notificationHubContext.Clients.User(leaveRequest.UserId.ToString()).addNotification(
                new
                {
                    title = "İzin Talebi Güncellendi",
                    message = $"İzin talebiniz {leaveRequest.Status.ToString()} durumuna güncellenmiştir.",
                    url = Url.Action("Details", "LeaveRequest", new { id = leaveRequest.Id }, Request.Url.Scheme)
                });

            if (DynamicAuthorizeAttribute.CurrentUserHasPermission("Operational.LeaveRequest", "View"))
            {
                // Görüntüleme yetkisi olanlar yönetim paneline gider
                return RedirectToAction("AdminDashboard");
            }

            // Diğer tüm kullanıcılar (sadece Approve/Manage olanlar dâhil) kendi liste sayfasına yönlendirilir
            return RedirectToAction("Index");
        }

        // GET: LeaveRequest/AdminDashboard
        [DynamicAuthorize(Permission = "Operational.LeaveRequest", Action = "View")]
        public async Task<ActionResult> AdminDashboard()
        {
            var query = db.LeaveRequests
                .Include(l => l.RequestingUser)
                .Include(l => l.ApprovedBy);

            var searchTerm = Request.QueryString["searchTerm"]; 
            var leaveTypeStr = Request.QueryString["leaveType"]; 
            var statusStr = Request.QueryString["status"]; 
            var dateRange = Request.QueryString["dateRange"];

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(l =>
                    l.RequestingUser.Username.Contains(searchTerm) ||
                    l.Department.Contains(searchTerm) ||
                    l.SubstituteName.Contains(searchTerm));
            }

            if (!string.IsNullOrWhiteSpace(leaveTypeStr) && Enum.TryParse(leaveTypeStr, out LeaveType lt))
            {
                query = query.Where(l => l.LeaveType == lt);
            }

            if (!string.IsNullOrWhiteSpace(statusStr))
            {
                if (statusStr == "Pending")
                {
                    query = query.Where(l => l.Status == LeaveStatus.PendingSupervisor || l.Status == LeaveStatus.PendingManager);
                }
                else if (Enum.TryParse(statusStr, out LeaveStatus st))
                {
                    query = query.Where(l => l.Status == st);
                }
            }

            if (!string.IsNullOrWhiteSpace(dateRange))
            {
                DateTime from, to;
                var today = DateTime.Today;
                switch (dateRange)
                {
                    case "thisMonth":
                        from = new DateTime(today.Year, today.Month, 1);
                        to = from.AddMonths(1);
                        break;
                    case "lastMonth":
                        var lastMonth = today.AddMonths(-1);
                        from = new DateTime(lastMonth.Year, lastMonth.Month, 1);
                        to = from.AddMonths(1);
                        break;
                    case "thisYear":
                        from = new DateTime(today.Year, 1, 1);
                        to = from.AddYears(1);
                        break;
                    case "lastYear":
                        from = new DateTime(today.Year - 1, 1, 1);
                        to = from.AddYears(1);
                        break;
                    default:
                        from = DateTime.MinValue;
                        to = DateTime.MaxValue;
                        break;
                }
                query = query.Where(l => l.StartDate >= from && l.StartDate < to);
            }

            var requests = await query.OrderByDescending(l => l.CreatedAt).ToListAsync();

            return View(requests);
        }

        // GET: LeaveRequest/ExportExcel
        [DynamicAuthorize(Permission = "Operational.LeaveRequest", Action = "Export")]
        public async Task<ActionResult> ExportExcel()
        {
            var data = await db.LeaveRequests
                .Include(l => l.RequestingUser)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("LeaveRequests");

                // Header
                ws.Cells[1, 1].Value = "Personel";
                ws.Cells[1, 2].Value = "İzin Türü";
                ws.Cells[1, 3].Value = "Başlangıç";
                ws.Cells[1, 4].Value = "Bitiş";
                ws.Cells[1, 5].Value = "Gün";
                ws.Cells[1, 6].Value = "Departman";
                ws.Cells[1, 7].Value = "Vekil";
                ws.Cells[1, 8].Value = "Durum";
                ws.Cells[1, 9].Value = "Talep Tarihi";

                int row = 2;
                foreach (var r in data)
                {
                    ws.Cells[row, 1].Value = r.RequestingUser?.Username;
                    ws.Cells[row, 2].Value = MZDNETWORK.Helpers.EnumDisplayHelper.ToDisplayName(r.LeaveType);
                    ws.Cells[row, 3].Value = r.StartDate.ToShortDateString();
                    ws.Cells[row, 4].Value = r.EndDate.ToShortDateString();
                    ws.Cells[row, 5].Value = r.TotalDays;
                    ws.Cells[row, 6].Value = r.Department;
                    ws.Cells[row, 7].Value = r.SubstituteName;
                    ws.Cells[row, 8].Value = MZDNETWORK.Helpers.EnumDisplayHelper.ToDisplayName(r.Status);
                    ws.Cells[row, 9].Value = r.CreatedAt.ToString("dd.MM.yyyy HH:mm");
                    row++;
                }

                ws.Cells[ws.Dimension.Address].AutoFitColumns();

                var bytes = package.GetAsByteArray();
                var fileName = $"IzinTalepleri_{DateTime.Now:yyyyMMddHHmm}.xlsx";
                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
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

            bool canApprove = DynamicAuthorizeAttribute.CurrentUserHasPermission("Operational.LeaveRequest", "Approve");
            bool canManage = DynamicAuthorizeAttribute.CurrentUserHasPermission("Operational.LeaveRequest", "Manage");

            if (leaveRequest.UserId != currentUserId && !canApprove && !canManage)
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