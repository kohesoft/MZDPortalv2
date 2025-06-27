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
                .AsEnumerable() // Materialize the query
                .Select(r => new
                {
                    id = r.Id,
                    userId = r.UserId,
                    userName = r.UserName,
                    room = r.Room,
                    date = r.Date.ToString("yyyy-MM-dd"),
                    startTime = r.StartTime,
                    endTime = r.EndTime,
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
                    r.Status != ReservationStatus.Rejected)
                .ToList();

            // Bellekte saat çakışma kontrolü
            bool conflict = sameDayReservations.Any(r =>
                TimeSpan.TryParse(r.StartTime, out var rStart) &&
                TimeSpan.TryParse(r.EndTime, out var rEnd) &&
                rStart < endTs && rEnd > startTs
            );

            if (conflict)
                return Json(new { success = false, message = "Çakışan rezervasyon var!" });

            var res = new Reservation
            {
                UserId = userId ?? 0,
                UserName = userName,
                Room = room,
                Date = date,
                StartTime = startTime,
                EndTime = endTime,
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
                    StartTime = res.StartTime,
                    EndTime = res.EndTime,
                    Title = res.Title,
                    Status = res.Status.ToString().ToLower()
                }
            });
        }




        [DynamicAuthorize(Permission = "Operational.MeetingRoom", Action = "Approve")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ApproveReservation(int id)
        {
            var r = db.Reservations.Find(id);
            if (r == null)
                return Json(new { success = false, message = "Bulunamadı" });
            r.Status = ReservationStatus.Approved;
            r.ApprovedAt = DateTime.Now;
            db.SaveChanges();
            return Json(new { success = true });
        }

        [DynamicAuthorize(Permission = "Operational.MeetingRoom", Action = "Reject")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult RejectReservation(int id, string reason)
        {
            var r = db.Reservations.Find(id);
            if (r == null)
                return Json(new { success = false, message = "Bulunamadı" });
            r.Status = ReservationStatus.Rejected;
            r.RejectedAt = DateTime.Now;
            r.RejectionReason = reason;
            db.SaveChanges();
            return Json(new { success = true });
        }
    }
}
