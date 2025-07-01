using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using MZDNETWORK.Models;
using MZDNETWORK.Data;
using MZDNETWORK.Attributes;

namespace MZDNETWORK.Controllers
{
    [DynamicAuthorize(Permission = "Operational.MeetingRoom")]
    public class MeetingRoomController : Controller
    {
        private readonly MZDNETWORKContext db = new MZDNETWORKContext();

        [DynamicAuthorize(Permission = "Operational.MeetingRoom", Action = "View")]
        public ActionResult Index()
        {
            ViewBag.Reservations = db.Reservations
                .OrderBy(r => r.Date)
                .ThenBy(r => r.StartTime)
                .ToList();
            ViewBag.IsAdmin = DynamicAuthorizeAttribute.CurrentUserHasPermission("Operational.MeetingRoom", "Manage");
            ViewBag.CurrentUserName = User.Identity.GetUserName();
            ViewBag.CurrentUserId = User.Identity.GetUserId();
            return View();
        }
        [DynamicAuthorize(Permission = "Operational.MeetingRoom", Action = "View")]
        [HttpGet]
        public JsonResult GetReservations()
        {
            var reservations = db.Reservations
                .OrderBy(r => r.Date)
                .ThenBy(r => r.StartTime)
                .AsEnumerable()
                .Select(r => new
                {
                    id = r.Id,
                    userId = r.UserId,
                    userName = r.UserName,
                    room = r.Room,
                    date = r.Date.ToString("yyyy-MM-dd"),
                    startTime = r.StartTime.ToString(@"hh\:mm"),
                    endTime = r.EndTime.ToString(@"hh\:mm"),
                    title = r.Title,
                    description = r.Description,
                    attendees = r.Attendees,
                    status = r.Status.ToString().ToLower(),
                    createdAt = r.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                    approvedAt = r.ApprovedAt.HasValue ? r.ApprovedAt.Value.ToString("yyyy-MM-dd HH:mm") : "",
                    rejectedAt = r.RejectedAt.HasValue ? r.RejectedAt.Value.ToString("yyyy-MM-dd HH:mm") : "",
                    rejectionReason = r.RejectionReason
                })
                .ToList();

            return Json(reservations, JsonRequestBehavior.AllowGet);
        }
        [DynamicAuthorize(Permission = "Operational.MeetingRoom", Action = "Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult CreateReservation(
    int? userId,
    string userName,
    string room,
    DateTime date,
    string startTime,
    string endTime,
    string title,
    string description,
    string attendees)
        {
            // Saat formatı kontrolü ve karşılaştırma
            if (!TimeSpan.TryParse(startTime, out var startTs) || !TimeSpan.TryParse(endTime, out var endTs))
            {
                return Json(new { success = false, message = "Saat formatı hatalı." });
            }
            if (startTs >= endTs)
            {
                return Json(new { success = false, message = "Başlangıç saati, bitiş saatinden önce olmalıdır." });
            }

            // Önce ilgili gün ve oda için çakışabilecek rezervasyonları çek
            var sameDayReservations = db.Reservations
                .Where(r =>
                    r.Room == room &&
                    DbFunctions.TruncateTime(r.Date) == date.Date &&
                    r.Status != ReservationStatus.Rejected &&
                    r.Status != ReservationStatus.Cancelled)
                .ToList();

            // Bellekte saat çakışma kontrolü
            bool conflict = sameDayReservations.Any(r =>
                r.StartTime < endTs && r.EndTime > startTs
            );

            if (conflict)
                return Json(new { success = false, message = "Çakışan rezervasyon var!" });

            var res = new Reservation
            {
                UserId = userId ?? 0,
                UserName = userName,
                Room = room,
                Date = date,
                StartTime = startTs,
                EndTime = endTs,
                Title = title,
                Description = description,
                Attendees = attendees,
                Status = ReservationStatus.Pending,
                CreatedAt = DateTime.Now
            };
            db.Reservations.Add(res);
            db.SaveChanges();
            return Json(new
            {
                success = true,
                reservation = new
                {
                    Room = res.Room,
                    Date = res.Date.ToString("yyyy-MM-dd"),
                    StartTime = res.StartTime.ToString(@"hh\:mm"),
                    EndTime = res.EndTime.ToString(@"hh\:mm"),
                    Title = res.Title,
                    Status = res.Status.ToString().ToLower()
                }
            });
        }

        // --------------------------------------------------
        // Onay / Reddet işlemleri
        // --------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ApproveReservation(int id)
        {
            const string permissionPath = "Operational.MeetingRoom";

            bool canApprove = DynamicAuthorizeAttribute.CurrentUserHasPermission(permissionPath, "Approve");
            bool canManage  = DynamicAuthorizeAttribute.CurrentUserHasPermission(permissionPath, "Manage");

            if (!canApprove && !canManage)
                return Json(new { success = false, message = "Yetkiniz yok" });

            var r = db.Reservations.Find(id);
            if (r == null)
                return Json(new { success = false, message = "Rezervasyon bulunamadı" });

            if (r.Status != ReservationStatus.Pending)
                return Json(new { success = false, message = "Bu rezervasyon zaten işlem görmüş." });

            r.Status      = ReservationStatus.Approved;
            r.ApprovedAt  = DateTime.Now;
            db.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult RejectReservation(int id, string reason)
        {
            const string permissionPath = "Operational.MeetingRoom";

            bool canApprove = DynamicAuthorizeAttribute.CurrentUserHasPermission(permissionPath, "Approve");
            bool canManage  = DynamicAuthorizeAttribute.CurrentUserHasPermission(permissionPath, "Manage");

            if (!canApprove && !canManage)
                return Json(new { success = false, message = "Yetkiniz yok" });

            var r = db.Reservations.Find(id);
            if (r == null)
                return Json(new { success = false, message = "Rezervasyon bulunamadı" });

            if (r.Status != ReservationStatus.Pending)
                return Json(new { success = false, message = "Bu rezervasyon zaten işlem görmüş." });

            r.Status          = ReservationStatus.Rejected;
            r.RejectedAt      = DateTime.Now;
            r.RejectionReason = reason;
            db.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult CancelReservation(int id)
        {
            var r = db.Reservations.Find(id);
            if (r == null)
                return Json(new { success = false, message = "Rezervasyon bulunamadı" });

            var permissionPath = "Operational.MeetingRoom";
            bool canManage = DynamicAuthorizeAttribute.CurrentUserHasPermission(permissionPath, "Manage");

            var currentUserId = User.Identity.GetUserId();

            // Kendi oluşturduğu rezervasyonu Pending durumunda iptal edebilir veya Manage yetkilisi her zaman
            if (!canManage && (r.UserId.ToString() != currentUserId || r.Status != ReservationStatus.Pending))
            {
                return Json(new { success = false, message = "Yetkiniz yok" });
            }

            if (r.Status == ReservationStatus.Cancelled)
                return Json(new { success = false, message = "Rezervasyon zaten iptal edilmiş" });

            r.Status = ReservationStatus.Cancelled;
            r.RejectedAt = DateTime.Now; // reuse field for timestamp
            r.RejectionReason = "İptal edildi";
            db.SaveChanges();

            return Json(new { success = true });
        }
    }
}
