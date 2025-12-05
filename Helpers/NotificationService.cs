using System;
using System.Linq;
using System.Data.Entity;
using MZDNETWORK.Data;
using MZDNETWORK.Models;

namespace MZDNETWORK.Helpers
{
    /// <summary>
    /// Bildirim yönetimi için servis sınıfı
    /// </summary>
    public class NotificationService
    {
        private readonly MZDNETWORKContext _context;
        private readonly EmailService _emailService;

        public NotificationService()
        {
            _context = new MZDNETWORKContext();
            _emailService = new EmailService();
        }

        public NotificationService(MZDNETWORKContext context)
        {
            _context = context;
            _emailService = new EmailService();
        }

        /// <summary>
        /// Kullanıcıya bildirim oluşturur
        /// </summary>
        public bool CreateNotification(string userId, string message)
        {
            try
            {
                var notification = new Notification
                {
                    UserId = userId,
                    Message = message,
                    IsRead = false,
                    CreatedDate = DateTime.Now
                };

                _context.Notifications.Add(notification);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Bildirim oluşturma hatası: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Toplantı rezervasyonu oluşturulduğunda bildirim gönderir
        /// </summary>
        public void SendMeetingCreatedNotification(Reservation reservation)
        {
            try
            {
                // Organizatöre bildirim
                CreateNotification(
                    reservation.UserId.ToString(),
                    $"'{reservation.Title}' toplantı talebiniz oluşturuldu ve onay bekliyor."
                );

                // Email gönder
                var user = _context.Users.Find(reservation.UserId);
                if (user != null && !string.IsNullOrEmpty(user.InternalEmail ?? user.ExternalEmail))
                {
                    _emailService.SendMeetingCreatedEmailAsync(
                        user.InternalEmail ?? user.ExternalEmail,
                        $"{user.Name} {user.Surname}",
                        reservation.Title,
                        reservation.Room,
                        reservation.Date,
                        reservation.StartTime
                    ).Wait();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Toplantı oluşturma bildirimi hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Toplantı onaylandığında bildirim gönderir
        /// </summary>
        public void SendMeetingApprovedNotification(Reservation reservation)
        {
            try
            {
                // Organizatöre bildirim
                CreateNotification(
                    reservation.UserId.ToString(),
                    $"✅ '{reservation.Title}' toplantınız onaylandı! Tarih: {reservation.Date:dd.MM.yyyy} {reservation.StartTime:hh\\:mm}, Salon: {reservation.Room}"
                );

                // Organizatöre email
                var organizer = _context.Users.Find(reservation.UserId);
                if (organizer != null && !string.IsNullOrEmpty(organizer.InternalEmail ?? organizer.ExternalEmail))
                {
                    _emailService.SendMeetingApprovedEmailAsync(
                        organizer.InternalEmail ?? organizer.ExternalEmail,
                        $"{organizer.Name} {organizer.Surname}",
                        reservation.Title,
                        reservation.Room,
                        reservation.Date,
                        reservation.StartTime,
                        reservation.EndTime
                    ).Wait();
                }

                // Katılımcılara bildirim ve email
                if (reservation.ReservationAttendees != null && reservation.ReservationAttendees.Any())
                {
                    foreach (var attendee in reservation.ReservationAttendees.Where(a => a.User != null))
                    {
                        CreateNotification(
                            attendee.UserId.ToString(),
                            $"📅 '{reservation.Title}' toplantısına davet edildiniz. Tarih: {reservation.Date:dd.MM.yyyy} {reservation.StartTime:hh\\:mm}, Salon: {reservation.Room}"
                        );

                        if (!string.IsNullOrEmpty(attendee.User.InternalEmail ?? attendee.User.ExternalEmail))
                        {
                            _emailService.SendMeetingApprovedEmailAsync(
                                attendee.User.InternalEmail ?? attendee.User.ExternalEmail,
                                $"{attendee.User.Name} {attendee.User.Surname}",
                                reservation.Title,
                                reservation.Room,
                                reservation.Date,
                                reservation.StartTime,
                                reservation.EndTime
                            ).Wait();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Toplantı onay bildirimi hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Toplantı reddedildiğinde bildirim gönderir
        /// </summary>
        public void SendMeetingRejectedNotification(Reservation reservation)
        {
            try
            {
                // Organizatöre bildirim
                CreateNotification(
                    reservation.UserId.ToString(),
                    $"❌ '{reservation.Title}' toplantınız reddedildi. Sebep: {reservation.RejectionReason}"
                );

                // Email gönder
                var user = _context.Users.Find(reservation.UserId);
                if (user != null && !string.IsNullOrEmpty(user.InternalEmail ?? user.ExternalEmail))
                {
                    _emailService.SendMeetingRejectedEmailAsync(
                        user.InternalEmail ?? user.ExternalEmail,
                        $"{user.Name} {user.Surname}",
                        reservation.Title,
                        reservation.Room,
                        reservation.Date,
                        reservation.StartTime,
                        reservation.RejectionReason
                    ).Wait();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Toplantı red bildirimi hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Toplantı hatırlatıcısı gönderir (15 dakika öncesi)
        /// </summary>
        public void SendMeetingReminder(Reservation reservation)
        {
            try
            {
                // Organizatöre hatırlatma
                CreateNotification(
                    reservation.UserId.ToString(),
                    $"⏰ Hatırlatma: '{reservation.Title}' toplantınız 15 dakika sonra başlayacak!"
                );

                // Katılımcılara hatırlatma
                if (reservation.ReservationAttendees != null && reservation.ReservationAttendees.Any())
                {
                    foreach (var attendee in reservation.ReservationAttendees.Where(a => a.HasAccepted))
                    {
                        CreateNotification(
                            attendee.UserId.ToString(),
                            $"⏰ Hatırlatma: '{reservation.Title}' toplantısı 15 dakika sonra başlayacak!"
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Toplantı hatırlatma bildirimi hatası: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
