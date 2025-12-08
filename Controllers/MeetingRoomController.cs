using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using MZDNETWORK.Models;
using MZDNETWORK.Data;
using MZDNETWORK.Attributes;
using MZDNETWORK.Helpers;

namespace MZDNETWORK.Controllers
{
    [DynamicAuthorize(Permission = "Operational.MeetingRoom")]
    public class MeetingRoomController : Controller
    {
        private readonly MZDNETWORKContext db = new MZDNETWORKContext();
        private readonly NotificationService _notificationService;

        public MeetingRoomController()
        {
            _notificationService = new NotificationService(db);
        }

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
        public ActionResult MyMeetings()
        {
            return View();
        }

        [DynamicAuthorize(Permission = "Operational.MeetingRoom", Action = "View")]
        [HttpGet]
        public JsonResult GetReservations()
        {
            var reservations = db.Reservations
                .Include(r => r.ReservationAttendees.Select(ra => ra.User))
                .OrderBy(r => r.Date)
                .ThenBy(r => r.StartTime)
                .ToList() // ToList ile sorguyu tamamla
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
                    attendeeList = r.ReservationAttendees.Select(ra => new
                    {
                        id = ra.UserId,
                        name = ra.User != null ? (ra.User.Name + " " + ra.User.Surname).Trim() : "Bilinmeyen",
                        department = ra.User != null ? ra.User.Department : "",
                        hasAccepted = ra.HasAccepted,
                        hasDeclined = ra.HasDeclined,
                        responseDate = ra.ResponseDate.HasValue ? ra.ResponseDate.Value.ToString("yyyy-MM-dd HH:mm") : null
                    }).ToList(),
                    attendeeCount = r.ReservationAttendees.Count(),
                    status = r.Status.ToString().ToLower(),
                    createdAt = r.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                    approvedAt = r.ApprovedAt.HasValue ? r.ApprovedAt.Value.ToString("yyyy-MM-dd HH:mm") : "",
                    rejectedAt = r.RejectedAt.HasValue ? r.RejectedAt.Value.ToString("yyyy-MM-dd HH:mm") : "",
                    rejectionReason = r.RejectionReason
                })
                .ToList();

            return Json(reservations, JsonRequestBehavior.AllowGet);
        }

        [DynamicAuthorize(Permission = "Operational.MeetingRoom", Action = "View")]
        [HttpGet]
        public JsonResult GetActiveUsers()
        {
            try
            {
                var users = db.Users
                    .Where(u => u.Username != null) // Aktif kullanıcılar (IsActive yoksa username kontrolü)
                    .OrderBy(u => u.Name)
                    .ThenBy(u => u.Surname)
                    .Select(u => new
                    {
                        id = u.Id,
                        name = (u.Name + " " + u.Surname).Trim(),
                        username = u.Username,
                        department = u.Department,
                        position = u.Position,
                        email = u.InternalEmail ?? u.ExternalEmail
                    })
                    .ToList();

                return Json(users, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [DynamicAuthorize(Permission = "Operational.MeetingRoom", Action = "View")]
        [HttpGet]
        public JsonResult GetMeetingRooms()
        {
            try
            {
                var rooms = db.MeetingRooms
                    .Where(r => r.IsActive)
                    .OrderBy(r => r.OrderIndex)
                    .Select(r => new
                    {
                        id = r.Id,
                        name = r.Name,
                        location = r.Location,
                        capacity = r.Capacity,
                        description = r.Description,
                        features = r.Features,
                        colorCode = r.ColorCode
                    })
                    .ToList();

                return Json(rooms, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
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
    string attendees,
    string attendeeIds = null) // Yeni parametre: katılımcı ID'leri
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
                Attendees = attendees, // Şimdilik tutuyoruz (backward compatibility)
                Status = ReservationStatus.Pending,
                CreatedAt = DateTime.Now
            };

            db.Reservations.Add(res);
            db.SaveChanges(); // Önce reservation'ı kaydet ki ID'si oluşsun

            // Bildirim gönder - Rezervasyon oluşturuldu
            try
            {
                _notificationService.SendMeetingCreatedNotification(res);
            }
            catch (Exception ex)
            {
                // Bildirim hatası rezervasyon oluşturmayı engellemez
                System.Diagnostics.Debug.WriteLine($"Bildirim hatası: {ex.Message}");
            }

            // Katılımcıları kaydet (attendeeIds varsa)
            if (!string.IsNullOrEmpty(attendeeIds))
            {
                var attendeeIdList = attendeeIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(id => int.TryParse(id.Trim(), out int parsedId) ? parsedId : 0)
                    .Where(id => id > 0)
                    .Distinct()
                    .ToList();

                foreach (var attendeeId in attendeeIdList)
                {
                    var attendee = new ReservationAttendee
                    {
                        ReservationId = res.Id,
                        UserId = attendeeId,
                        CreatedAt = DateTime.Now
                    };
                    db.ReservationAttendees.Add(attendee);
                }

                db.SaveChanges(); // Katılımcıları kaydet
            }

            // SignalR ile tüm istemcilere bildir (sayfa yenilemeden)
            try
            {
                var hubContext = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MZDNETWORK.Hubs.NotificationHub>();
                hubContext.Clients.All.refreshMeetings();
            }
            catch { /* SignalR hatası görmezden gelinir */ }

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

            // Bildirim gönder - Rezervasyon onaylandı
            try
            {
                // Include ile katılımcıları yükle
                var reservation = db.Reservations
                    .Include(res => res.ReservationAttendees.Select(ra => ra.User))
                    .FirstOrDefault(res => res.Id == id);
                
                if (reservation != null)
                {
                    _notificationService.SendMeetingApprovedNotification(reservation);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Bildirim hatası: {ex.Message}");
            }

            // SignalR ile tüm istemcilere bildir (sayfa yenilemeden)
            try
            {
                var hubContext = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MZDNETWORK.Hubs.NotificationHub>();
                hubContext.Clients.All.refreshMeetings();
            }
            catch { /* SignalR hatası görmezden gelinir */ }

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

            // Bildirim gönder - Rezervasyon reddedildi
            try
            {
                _notificationService.SendMeetingRejectedNotification(r);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Bildirim hatası: {ex.Message}");
            }

            // SignalR ile tüm istemcilere bildir (sayfa yenilemeden)
            try
            {
                var hubContext = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MZDNETWORK.Hubs.NotificationHub>();
                hubContext.Clients.All.refreshMeetings();
            }
            catch { /* SignalR hatası görmezden gelinir */ }

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

            // SignalR ile tüm istemcilere bildir (sayfa yenilemeden)
            try
            {
                var hubContext = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MZDNETWORK.Hubs.NotificationHub>();
                hubContext.Clients.All.refreshMeetings();
            }
            catch { /* SignalR hatası görmezden gelinir */ }

            return Json(new { success = true });
        }

        // --------------------------------------------------
        // Rezervasyon Düzenleme
        // --------------------------------------------------

        [DynamicAuthorize(Permission = "Operational.MeetingRoom", Action = "View")]
        [HttpGet]
        public ActionResult Edit(int id)
        {
            var reservation = db.Reservations
                .Include(r => r.ReservationAttendees.Select(ra => ra.User))
                .FirstOrDefault(r => r.Id == id);

            if (reservation == null)
            {
                TempData["ErrorMessage"] = "Rezervasyon bulunamadı";
                return RedirectToAction("Index");
            }

            var currentUserId = User.Identity.GetUserId();
            bool canManage = DynamicAuthorizeAttribute.CurrentUserHasPermission("Operational.MeetingRoom", "Manage");

            // Sadece kendi rezervasyonunu düzenleyebilir (Pending durumda) veya yönetici
            if (!canManage && (reservation.UserId.ToString() != currentUserId || reservation.Status != ReservationStatus.Pending))
            {
                TempData["ErrorMessage"] = "Bu rezervasyonu düzenleme yetkiniz yok";
                return RedirectToAction("Index");
            }

            ViewBag.CurrentUserId = currentUserId;
            return View(reservation);
        }

        [DynamicAuthorize(Permission = "Operational.MeetingRoom", Action = "View")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UpdateReservation(
            int id,
            string room,
            DateTime date,
            string startTime,
            string endTime,
            string title,
            string description,
            string attendeeIds)
        {
            try
            {
                var reservation = db.Reservations.Find(id);
                if (reservation == null)
                    return Json(new { success = false, message = "Rezervasyon bulunamadı" });

                var currentUserId = User.Identity.GetUserId();
                bool canManage = DynamicAuthorizeAttribute.CurrentUserHasPermission("Operational.MeetingRoom", "Manage");

                // Yetki kontrolü
                if (!canManage && (reservation.UserId.ToString() != currentUserId || reservation.Status != ReservationStatus.Pending))
                {
                    return Json(new { success = false, message = "Yetkiniz yok" });
                }

                // Saat formatı kontrolü
                if (!TimeSpan.TryParse(startTime, out var startTs) || !TimeSpan.TryParse(endTime, out var endTs))
                {
                    return Json(new { success = false, message = "Saat formatı hatalı" });
                }
                if (startTs >= endTs)
                {
                    return Json(new { success = false, message = "Başlangıç saati, bitiş saatinden önce olmalıdır" });
                }

                // Çakışma kontrolü (kendi rezervasyonu hariç)
                var sameDayReservations = db.Reservations
                    .Where(r =>
                        r.Id != id &&
                        r.Room == room &&
                        DbFunctions.TruncateTime(r.Date) == date.Date &&
                        r.Status != ReservationStatus.Rejected &&
                        r.Status != ReservationStatus.Cancelled)
                    .ToList();

                bool conflict = sameDayReservations.Any(r =>
                    r.StartTime < endTs && r.EndTime > startTs
                );

                if (conflict)
                    return Json(new { success = false, message = "Çakışan rezervasyon var!" });

                // Rezervasyonu güncelle
                reservation.Room = room;
                reservation.Date = date;
                reservation.StartTime = startTs;
                reservation.EndTime = endTs;
                reservation.Title = title;
                reservation.Description = description;

                // Katılımcıları güncelle
                if (!string.IsNullOrEmpty(attendeeIds))
                {
                    // Mevcut katılımcıları sil
                    var existingAttendees = db.ReservationAttendees.Where(ra => ra.ReservationId == id).ToList();
                    db.ReservationAttendees.RemoveRange(existingAttendees);

                    // Yeni katılımcıları ekle
                    var attendeeIdList = attendeeIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(aid => int.TryParse(aid.Trim(), out int parsedId) ? parsedId : 0)
                        .Where(aid => aid > 0)
                        .Distinct()
                        .ToList();

                    foreach (var attendeeId in attendeeIdList)
                    {
                        var attendee = new ReservationAttendee
                        {
                            ReservationId = id,
                            UserId = attendeeId,
                            CreatedAt = DateTime.Now
                        };
                        db.ReservationAttendees.Add(attendee);
                    }
                }

                db.SaveChanges();

                // SignalR ile tüm istemcilere bildir (sayfa yenilemeden)
                try
                {
                    var hubContext = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MZDNETWORK.Hubs.NotificationHub>();
                    hubContext.Clients.All.refreshMeetings();
                }
                catch { /* SignalR hatası görmezden gelinir */ }

                return Json(new { success = true, message = "Rezervasyon güncellendi" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Rezervasyon güncelleme hatası: {ex.Message}");
                return Json(new { success = false, message = "Bir hata oluştu" });
            }
        }

        // --------------------------------------------------
        // Katılımcı Onay İşlemleri
        // --------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AcceptMeeting(int reservationId)
        {
            try
            {
                var username = User.Identity.Name;
                if (string.IsNullOrEmpty(username))
                    return Json(new { success = false, message = "Kullanıcı bulunamadı" });

                var user = db.Users.FirstOrDefault(u => u.Username == username);
                if (user == null)
                    return Json(new { success = false, message = "Kullanıcı veritabanında bulunamadı" });

                var attendee = db.ReservationAttendees
                    .FirstOrDefault(a => a.ReservationId == reservationId && a.UserId == user.Id);

                if (attendee == null)
                    return Json(new { success = false, message = "Katılımcı kaydı bulunamadı" });

                attendee.HasAccepted = true;
                attendee.HasDeclined = false;
                attendee.ResponseDate = DateTime.Now;
                db.SaveChanges();

                // SignalR ile tüm istemcilere bildir
                try
                {
                    var hubContext = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MZDNETWORK.Hubs.NotificationHub>();
                    hubContext.Clients.All.refreshMeetings();
                }
                catch { /* SignalR hatası görmezden gelinir */ }

                return Json(new { success = true, message = "Toplantıya katılım onaylandı" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Toplantı kabul hatası: {ex.Message}");
                return Json(new { success = false, message = "Bir hata oluştu" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeclineMeeting(int reservationId)
        {
            try
            {
                var username = User.Identity.Name;
                if (string.IsNullOrEmpty(username))
                    return Json(new { success = false, message = "Kullanıcı bulunamadı" });

                var user = db.Users.FirstOrDefault(u => u.Username == username);
                if (user == null)
                    return Json(new { success = false, message = "Kullanıcı veritabanında bulunamadı" });

                var attendee = db.ReservationAttendees
                    .FirstOrDefault(a => a.ReservationId == reservationId && a.UserId == user.Id);

                if (attendee == null)
                    return Json(new { success = false, message = "Katılımcı kaydı bulunamadı" });

                attendee.HasAccepted = false;
                attendee.HasDeclined = true;
                attendee.ResponseDate = DateTime.Now;
                db.SaveChanges();

                // SignalR ile tüm istemcilere bildir
                try
                {
                    var hubContext = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MZDNETWORK.Hubs.NotificationHub>();
                    hubContext.Clients.All.refreshMeetings();
                }
                catch { /* SignalR hatası görmezden gelinir */ }

                return Json(new { success = true, message = "Toplantı katılımı reddedildi" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Toplantı red hatası: {ex.Message}");
                return Json(new { success = false, message = "Bir hata oluştu" });
            }
        }

        // Kullanıcının katılımcısı olduğu toplantıları listele
        [HttpGet]
        public JsonResult GetMyMeetings()
        {
            try
            {
                var username = User.Identity.Name;
                if (string.IsNullOrEmpty(username))
                    return Json(new { error = "Kullanıcı bulunamadı" }, JsonRequestBehavior.AllowGet);

                // Username'den User ID'yi bul
                var user = db.Users.FirstOrDefault(u => u.Username == username);
                if (user == null)
                    return Json(new { error = "Kullanıcı veritabanında bulunamadı" }, JsonRequestBehavior.AllowGet);

                var myMeetings = db.ReservationAttendees
                    .Include(a => a.Reservation)
                    .Where(a => a.UserId == user.Id)
                    .ToList()
                    .Select(a => new
                    {
                        reservationId = a.ReservationId,
                        title = a.Reservation.Title,
                        room = a.Reservation.Room,
                        date = a.Reservation.Date.ToString("yyyy-MM-dd"),
                        startTime = a.Reservation.StartTime.ToString(@"hh\:mm"),
                        endTime = a.Reservation.EndTime.ToString(@"hh\:mm"),
                        organizer = a.Reservation.UserName,
                        status = a.Reservation.Status.ToString().ToLower(),
                        hasAccepted = a.HasAccepted,
                        hasDeclined = a.HasDeclined,
                        responseDate = a.ResponseDate.HasValue ? a.ResponseDate.Value.ToString("yyyy-MM-dd HH:mm") : null,
                        isOrganizer = a.Reservation.UserId == user.Id
                    })
                    .ToList();

                return Json(myMeetings, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Toplantıları getirme hatası: {ex.Message}");
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // --------------------------------------------------
        // Toplantı Detay ve Karar Yönetimi
        // --------------------------------------------------

        [DynamicAuthorize(Permission = "Operational.MeetingRoom", Action = "View")]
        public ActionResult Details(int id)
        {
            var reservation = db.Reservations
                .Include(r => r.ReservationAttendees.Select(ra => ra.User))
                .Include(r => r.MeetingDecisions)
                .FirstOrDefault(r => r.Id == id);

            if (reservation == null)
            {
                TempData["ErrorMessage"] = "Toplantı bulunamadı";
                return RedirectToAction("Index");
            }

            ViewBag.IsAdmin = DynamicAuthorizeAttribute.CurrentUserHasPermission("Operational.MeetingRoom", "Manage");
            ViewBag.CurrentUserId = User.Identity.GetUserId();
            return View(reservation);
        }

        [HttpGet]
        public JsonResult GetMeetingDecisions(int reservationId)
        {
            try
            {
                var decisions = db.MeetingDecisions
                    .Where(d => d.ReservationId == reservationId)
                    .OrderBy(d => d.OrderIndex)
                    .Select(d => new
                    {
                        id = d.Id,
                        title = d.Title,
                        description = d.Description,
                        responsiblePerson = d.ResponsiblePerson,
                        dueDate = d.DueDate,
                        status = d.Status.ToString().ToLower(),
                        orderIndex = d.OrderIndex,
                        createdBy = d.CreatedByUserName,
                        createdAt = d.CreatedAt,
                        completedAt = d.CompletedAt,
                        notes = d.Notes
                    })
                    .ToList();

                return Json(decisions, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AddDecision(
            int reservationId,
            string title,
            string description,
            string responsiblePerson,
            DateTime? dueDate,
            string notes)
        {
            try
            {
                var username = User.Identity.Name;
                if (string.IsNullOrEmpty(username))
                    return Json(new { success = false, message = "Kullanıcı bulunamadı" });

                var currentUser = db.Users.FirstOrDefault(u => u.Username == username);
                if (currentUser == null)
                    return Json(new { success = false, message = "Kullanıcı veritabanında bulunamadı" });

                var reservation = db.Reservations.Find(reservationId);
                if (reservation == null)
                    return Json(new { success = false, message = "Toplantı bulunamadı" });

                // Son sıra numarasını bul
                var maxOrder = db.MeetingDecisions
                    .Where(d => d.ReservationId == reservationId)
                    .Max(d => (int?)d.OrderIndex) ?? 0;

                var decision = new MeetingDecision
                {
                    ReservationId = reservationId,
                    Title = title,
                    Description = description,
                    ResponsiblePerson = responsiblePerson,
                    DueDate = dueDate,
                    Notes = notes,
                    Status = DecisionStatus.Pending,
                    OrderIndex = maxOrder + 1,
                    CreatedByUserId = currentUser.Id,
                    CreatedByUserName = $"{currentUser.Name} {currentUser.Surname}",
                    CreatedAt = DateTime.Now
                };

                db.MeetingDecisions.Add(decision);
                db.SaveChanges();

                // SignalR ile tüm istemcilere bildir (sayfa yenilemeden)
                try
                {
                    var hubContext = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MZDNETWORK.Hubs.NotificationHub>();
                    hubContext.Clients.All.refreshMeetings();
                }
                catch { /* SignalR hatası görmezden gelinir */ }

                return Json(new { success = true, message = "Karar başarıyla eklendi" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Karar ekleme hatası: {ex.Message}");
                return Json(new { success = false, message = "Bir hata oluştu" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UpdateDecisionStatus(int id, int status)
        {
            try
            {
                var decision = db.MeetingDecisions.Find(id);
                if (decision == null)
                    return Json(new { success = false, message = "Karar bulunamadı" });

                decision.Status = (DecisionStatus)status;
                decision.UpdatedAt = DateTime.Now;

                if (status == (int)DecisionStatus.Completed)
                {
                    decision.CompletedAt = DateTime.Now;
                }

                db.SaveChanges();

                // SignalR ile tüm istemcilere bildir (sayfa yenilemeden)
                try
                {
                    var hubContext = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MZDNETWORK.Hubs.NotificationHub>();
                    hubContext.Clients.All.refreshMeetings();
                }
                catch { /* SignalR hatası görmezden gelinir */ }

                return Json(new { success = true, message = "Durum güncellendi" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Durum güncelleme hatası: {ex.Message}");
                return Json(new { success = false, message = "Bir hata oluştu" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteDecision(int id)
        {
            try
            {
                var decision = db.MeetingDecisions.Find(id);
                if (decision == null)
                    return Json(new { success = false, message = "Karar bulunamadı" });

                db.MeetingDecisions.Remove(decision);
                db.SaveChanges();

                // SignalR ile tüm istemcilere bildir (sayfa yenilemeden)
                try
                {
                    var hubContext = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MZDNETWORK.Hubs.NotificationHub>();
                    hubContext.Clients.All.refreshMeetings();
                }
                catch { /* SignalR hatası görmezden gelinir */ }

                return Json(new { success = true, message = "Karar silindi" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Karar silme hatası: {ex.Message}");
                return Json(new { success = false, message = "Bir hata oluştu" });
            }
        }

        #region Meeting Actions

        [HttpGet]
        public JsonResult GetMeetingActions(int reservationId)
        {
            try
            {
                var actions = db.MeetingActions
                    .Where(a => a.ReservationId == reservationId)
                    .OrderBy(a => a.OrderIndex)
                    .Select(a => new
                    {
                        id = a.Id,
                        title = a.Title,
                        description = a.Description,
                        responsiblePerson = a.ResponsiblePerson,
                        dueDate = a.DueDate,
                        status = a.Status.ToString().ToLower(),
                        orderIndex = a.OrderIndex,
                        createdBy = a.CreatedByUserName,
                        createdAt = a.CreatedAt,
                        completedAt = a.CompletedAt,
                        notes = a.Notes
                    })
                    .ToList();

                return Json(actions, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AddAction(
            int reservationId,
            string title,
            string description,
            string responsiblePerson,
            DateTime? dueDate,
            string notes)
        {
            try
            {
                var username = User.Identity.Name;
                if (string.IsNullOrEmpty(username))
                    return Json(new { success = false, message = "Kullanıcı bulunamadı" });

                var currentUser = db.Users.FirstOrDefault(u => u.Username == username);
                if (currentUser == null)
                    return Json(new { success = false, message = "Kullanıcı veritabanında bulunamadı" });

                var reservation = db.Reservations.Find(reservationId);
                if (reservation == null)
                    return Json(new { success = false, message = "Toplantı bulunamadı" });

                // Son sıra numarasını bul
                var maxOrder = db.MeetingActions
                    .Where(a => a.ReservationId == reservationId)
                    .Max(a => (int?)a.OrderIndex) ?? 0;

                var action = new MeetingAction
                {
                    ReservationId = reservationId,
                    Title = title,
                    Description = description,
                    ResponsiblePerson = responsiblePerson,
                    DueDate = dueDate,
                    Notes = notes,
                    Status = ActionStatus.Pending,
                    OrderIndex = maxOrder + 1,
                    CreatedByUserId = currentUser.Id,
                    CreatedByUserName = $"{currentUser.Name} {currentUser.Surname}",
                    CreatedAt = DateTime.Now
                };

                db.MeetingActions.Add(action);
                db.SaveChanges();

                // SignalR ile tüm istemcilere bildir
                try
                {
                    var hubContext = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MZDNETWORK.Hubs.NotificationHub>();
                    hubContext.Clients.All.refreshMeetings();
                }
                catch { /* SignalR hatası görmezden gelinir */ }

                return Json(new { success = true, message = "Aksiyon başarıyla eklendi" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Aksiyon ekleme hatası: {ex.Message}");
                return Json(new { success = false, message = "Bir hata oluştu" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UpdateActionStatus(int id, int status)
        {
            try
            {
                var action = db.MeetingActions.Find(id);
                if (action == null)
                    return Json(new { success = false, message = "Aksiyon bulunamadı" });

                action.Status = (ActionStatus)status;
                action.UpdatedAt = DateTime.Now;

                if (status == (int)ActionStatus.Completed)
                {
                    action.CompletedAt = DateTime.Now;
                }

                db.SaveChanges();

                // SignalR ile tüm istemcilere bildir
                try
                {
                    var hubContext = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MZDNETWORK.Hubs.NotificationHub>();
                    hubContext.Clients.All.refreshMeetings();
                }
                catch { /* SignalR hatası görmezden gelinir */ }

                return Json(new { success = true, message = "Durum güncellendi" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Durum güncelleme hatası: {ex.Message}");
                return Json(new { success = false, message = "Bir hata oluştu" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteAction(int id)
        {
            try
            {
                var action = db.MeetingActions.Find(id);
                if (action == null)
                    return Json(new { success = false, message = "Aksiyon bulunamadı" });

                db.MeetingActions.Remove(action);
                db.SaveChanges();

                // SignalR ile tüm istemcilere bildir
                try
                {
                    var hubContext = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MZDNETWORK.Hubs.NotificationHub>();
                    hubContext.Clients.All.refreshMeetings();
                }
                catch { /* SignalR hatası görmezden gelinir */ }

                return Json(new { success = true, message = "Aksiyon silindi" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Aksiyon silme hatası: {ex.Message}");
                return Json(new { success = false, message = "Bir hata oluştu" });
            }
        }

        #endregion

        #region Yönetim İşlemleri

        /// <summary>
        /// Toplantı salonu yönetim sayfası
        /// </summary>
        [DynamicAuthorize(Permission = "Operational.MeetingRoom", Action = "Manage")]
        public ActionResult ManageRooms()
        {
            var rooms = db.MeetingRooms.OrderBy(r => r.Name).ToList();
            return View(rooms);
        }

        /// <summary>
        /// Yeni toplantı salonu ekleme
        /// </summary>
        [DynamicAuthorize(Permission = "Operational.MeetingRoom", Action = "Manage")]
        [HttpPost]
        public JsonResult AddRoom(string name, int capacity, string description, string features = null, string colorCode = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    return Json(new { success = false, message = "Salon adı gereklidir" });

                if (capacity <= 0)
                    return Json(new { success = false, message = "Kapasite 0'dan büyük olmalıdır" });

                // Aynı isimde salon var mı kontrol et
                if (db.MeetingRooms.Any(r => r.Name == name))
                    return Json(new { success = false, message = "Bu isimde bir salon zaten mevcut" });

                var room = new MeetingRoom
                {
                    Name = name,
                    Capacity = capacity,
                    Description = description ?? "",
                    Features = features ?? "",
                    ColorCode = colorCode ?? "#3B82F6",
                    IsActive = true
                };

                db.MeetingRooms.Add(room);
                db.SaveChanges();

                return Json(new { success = true, message = "Salon başarıyla eklendi", room = new { room.Id, room.Name, room.Capacity, room.Description } });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Salon ekleme hatası: {ex.Message}");
                return Json(new { success = false, message = "Bir hata oluştu" });
            }
        }

        /// <summary>
        /// Toplantı salonu güncelleme
        /// </summary>
        [DynamicAuthorize(Permission = "Operational.MeetingRoom", Action = "Manage")]
        [HttpPost]
        public JsonResult UpdateRoom(int id, string name, int capacity, string description, string features = null, string colorCode = null)
        {
            try
            {
                var room = db.MeetingRooms.Find(id);
                if (room == null)
                    return Json(new { success = false, message = "Salon bulunamadı" });

                if (string.IsNullOrWhiteSpace(name))
                    return Json(new { success = false, message = "Salon adı gereklidir" });

                if (capacity <= 0)
                    return Json(new { success = false, message = "Kapasite 0'dan büyük olmalıdır" });

                // Başka salon aynı ismi kullanıyor mu kontrol et
                if (db.MeetingRooms.Any(r => r.Name == name && r.Id != id))
                    return Json(new { success = false, message = "Bu isimde bir salon zaten mevcut" });

                room.Name = name;
                room.Capacity = capacity;
                room.Description = description ?? "";
                room.Features = features ?? "";
                room.ColorCode = colorCode ?? "#3B82F6";
                room.UpdatedAt = DateTime.Now;
                db.SaveChanges();

                return Json(new { success = true, message = "Salon güncellendi" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Salon güncelleme hatası: {ex.Message}");
                return Json(new { success = false, message = "Bir hata oluştu" });
            }
        }

        /// <summary>
        /// Toplantı salonu silme (soft delete)
        /// </summary>
        [DynamicAuthorize(Permission = "Operational.MeetingRoom", Action = "Manage")]
        [HttpPost]
        public JsonResult DeleteRoom(int id)
        {
            try
            {
                var room = db.MeetingRooms.Find(id);
                if (room == null)
                    return Json(new { success = false, message = "Salon bulunamadı" });

                // Gelecekteki rezervasyonlar var mı kontrol et
                var futureReservations = db.Reservations
                    .Where(r => r.RoomId == id && r.Date >= DateTime.Today)
                    .Count();

                if (futureReservations > 0)
                    return Json(new { success = false, message = $"Bu salonda {futureReservations} adet gelecek rezervasyon var. Önce bunları iptal edin." });

                // Soft delete
                room.IsActive = false;
                room.UpdatedAt = DateTime.Now;
                db.SaveChanges();

                return Json(new { success = true, message = "Salon devre dışı bırakıldı" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Salon silme hatası: {ex.Message}");
                return Json(new { success = false, message = "Bir hata oluştu" });
            }
        }

        /// <summary>
        /// Devre dışı salonu tekrar aktif etme
        /// </summary>
        [DynamicAuthorize(Permission = "Operational.MeetingRoom", Action = "Manage")]
        [HttpPost]
        public JsonResult ActivateRoom(int id)
        {
            try
            {
                var room = db.MeetingRooms.Find(id);
                if (room == null)
                    return Json(new { success = false, message = "Salon bulunamadı" });

                room.IsActive = true;
                room.UpdatedAt = DateTime.Now;
                db.SaveChanges();

                return Json(new { success = true, message = "Salon aktif edildi" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Salon aktif etme hatası: {ex.Message}");
                return Json(new { success = false, message = "Bir hata oluştu" });
            }
        }

        /// <summary>
        /// Onaylanan rezervasyonu silme (admin)
        /// </summary>
        [DynamicAuthorize(Permission = "Operational.MeetingRoom", Action = "Manage")]
        [HttpPost]
        public JsonResult DeleteReservation(int id)
        {
            try
            {
                var reservation = db.Reservations
                    .Include(r => r.ReservationAttendees.Select(ra => ra.User))
                    .FirstOrDefault(r => r.Id == id);

                if (reservation == null)
                    return Json(new { success = false, message = "Rezervasyon bulunamadı" });

                var attendees = reservation.ReservationAttendees.ToList();
                var organizerName = User.Identity.GetUserName();

                // Katılımcılara bildirim gönder
                foreach (var attendee in attendees)
                {
                    if (attendee.User != null)
                    {
                        _notificationService.CreateNotification(
                            userId: attendee.UserId.ToString(),
                            title: "Toplantı İptal Edildi",
                            message: $"{reservation.Title} toplantısı yönetici tarafından iptal edildi.",
                            type: "MeetingCancelled",
                            relatedId: reservation.Id.ToString()
                        );
                    }
                }

                // Rezervasyonu sil
                db.Reservations.Remove(reservation);
                db.SaveChanges();

                // SignalR ile tüm istemcilere bildir
                try
                {
                    var hubContext = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MZDNETWORK.Hubs.NotificationHub>();
                    hubContext.Clients.All.refreshMeetings();
                }
                catch { /* SignalR hatası görmezden gelinir */ }

                return Json(new { success = true, message = "Rezervasyon silindi ve katılımcılara bildirim gönderildi" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Rezervasyon silme hatası: {ex.Message}");
                return Json(new { success = false, message = "Bir hata oluştu" });
            }
        }

        #endregion
    }
}
