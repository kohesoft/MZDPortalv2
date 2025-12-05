using System;
using System.Linq;
using System.Data.Entity;
using MZDNETWORK.Data;
using MZDNETWORK.Models;
using NLog;

namespace MZDNETWORK.Helpers
{
    /// <summary>
    /// Toplantı hatırlatıcı servisi - Hangfire ile çalışır
    /// </summary>
    public class MeetingReminderService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly MZDNETWORKContext _context;
        private readonly NotificationService _notificationService;
        private readonly EmailService _emailService;

        public MeetingReminderService()
        {
            _context = new MZDNETWORKContext();
            _notificationService = new NotificationService(_context);
            _emailService = new EmailService();
        }

        public MeetingReminderService(MZDNETWORKContext context)
        {
            _context = context;
            _notificationService = new NotificationService(context);
            _emailService = new EmailService();
        }

        /// <summary>
        /// Yaklaşan toplantıları kontrol eder ve hatırlatıcı gönderir
        /// Hangfire tarafından her 5 dakikada bir çağrılır
        /// </summary>
        public void CheckAndSendReminders()
        {
            try
            {
                var now = DateTime.Now;
                var todayDate = now.Date;

                // Bugün yapılacak, onaylanmış toplantıları getir
                var upcomingMeetings = _context.Reservations
                    .Include(r => r.ReservationAttendees.Select(ra => ra.User))
                    .Where(r =>
                        DbFunctions.TruncateTime(r.Date) == todayDate &&
                        r.Status == ReservationStatus.Approved)
                    .ToList();

                foreach (var meeting in upcomingMeetings)
                {
                    var meetingDateTime = meeting.Date.Date.Add(meeting.StartTime);
                    var timeUntilMeeting = meetingDateTime - now;

                    // 15 dakika kala hatırlatma
                    if (timeUntilMeeting.TotalMinutes > 13 && timeUntilMeeting.TotalMinutes <= 18)
                    {
                        if (!HasReminderBeenSent(meeting.Id, ReminderType.Minutes15))
                        {
                            Send15MinuteReminder(meeting);
                            LogReminderSent(meeting.Id, ReminderType.Minutes15);
                        }
                    }
                    // 1 saat kala hatırlatma
                    else if (timeUntilMeeting.TotalMinutes > 58 && timeUntilMeeting.TotalMinutes <= 62)
                    {
                        if (!HasReminderBeenSent(meeting.Id, ReminderType.Hour1))
                        {
                            Send1HourReminder(meeting);
                            LogReminderSent(meeting.Id, ReminderType.Hour1);
                        }
                    }
                    // 24 saat kala hatırlatma
                    else if (timeUntilMeeting.TotalHours > 23.5 && timeUntilMeeting.TotalHours <= 24.5)
                    {
                        if (!HasReminderBeenSent(meeting.Id, ReminderType.Hours24))
                        {
                            Send24HourReminder(meeting);
                            LogReminderSent(meeting.Id, ReminderType.Hours24);
                        }
                    }
                }

                Logger.Info($"Meeting reminder check completed. Checked {upcomingMeetings.Count} upcoming meetings.");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error in CheckAndSendReminders");
            }
        }

        /// <summary>
        /// 15 dakika öncesi hatırlatma
        /// </summary>
        private void Send15MinuteReminder(Reservation meeting)
        {
            try
            {
                // Organizatöre hatırlatma
                _notificationService.CreateNotification(
                    meeting.UserId.ToString(),
                    $"⏰ Hatırlatma: '{meeting.Title}' toplantınız 15 dakika sonra başlayacak! Salon: {meeting.Room}"
                );

                // Organizatöre email
                var organizer = _context.Users.Find(meeting.UserId);
                if (organizer != null && !string.IsNullOrEmpty(organizer.InternalEmail ?? organizer.ExternalEmail))
                {
                    _emailService.SendMeetingReminderEmailAsync(
                        organizer.InternalEmail ?? organizer.ExternalEmail,
                        $"{organizer.Name} {organizer.Surname}",
                        meeting.Title,
                        meeting.Room,
                        meeting.Date,
                        meeting.StartTime,
                        15
                    ).Wait();
                }

                // Katılımcılara hatırlatma
                if (meeting.ReservationAttendees != null && meeting.ReservationAttendees.Any())
                {
                    foreach (var attendee in meeting.ReservationAttendees.Where(a => a.HasAccepted))
                    {
                        _notificationService.CreateNotification(
                            attendee.UserId.ToString(),
                            $"⏰ Hatırlatma: '{meeting.Title}' toplantısı 15 dakika sonra başlayacak! Salon: {meeting.Room}"
                        );

                        if (attendee.User != null && !string.IsNullOrEmpty(attendee.User.InternalEmail ?? attendee.User.ExternalEmail))
                        {
                            _emailService.SendMeetingReminderEmailAsync(
                                attendee.User.InternalEmail ?? attendee.User.ExternalEmail,
                                $"{attendee.User.Name} {attendee.User.Surname}",
                                meeting.Title,
                                meeting.Room,
                                meeting.Date,
                                meeting.StartTime,
                                15
                            ).Wait();
                        }
                    }
                }

                Logger.Info($"15-minute reminder sent for meeting: {meeting.Id} - {meeting.Title}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error sending 15-minute reminder for meeting {meeting.Id}");
            }
        }

        /// <summary>
        /// 1 saat öncesi hatırlatma
        /// </summary>
        private void Send1HourReminder(Reservation meeting)
        {
            try
            {
                // Organizatöre hatırlatma
                _notificationService.CreateNotification(
                    meeting.UserId.ToString(),
                    $"🔔 Hatırlatma: '{meeting.Title}' toplantınız 1 saat sonra başlayacak. Tarih: {meeting.Date:dd.MM.yyyy} {meeting.StartTime:hh\\:mm}, Salon: {meeting.Room}"
                );

                // Katılımcılara hatırlatma
                if (meeting.ReservationAttendees != null && meeting.ReservationAttendees.Any())
                {
                    foreach (var attendee in meeting.ReservationAttendees.Where(a => a.HasAccepted))
                    {
                        _notificationService.CreateNotification(
                            attendee.UserId.ToString(),
                            $"🔔 Hatırlatma: '{meeting.Title}' toplantısı 1 saat sonra başlayacak. Tarih: {meeting.Date:dd.MM.yyyy} {meeting.StartTime:hh\\:mm}, Salon: {meeting.Room}"
                        );
                    }
                }

                Logger.Info($"1-hour reminder sent for meeting: {meeting.Id} - {meeting.Title}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error sending 1-hour reminder for meeting {meeting.Id}");
            }
        }

        /// <summary>
        /// 24 saat öncesi hatırlatma
        /// </summary>
        private void Send24HourReminder(Reservation meeting)
        {
            try
            {
                // Organizatöre hatırlatma
                _notificationService.CreateNotification(
                    meeting.UserId.ToString(),
                    $"📅 Hatırlatma: '{meeting.Title}' toplantınız yarın saat {meeting.StartTime:hh\\:mm}'de. Salon: {meeting.Room}"
                );

                // Katılımcılara hatırlatma
                if (meeting.ReservationAttendees != null && meeting.ReservationAttendees.Any())
                {
                    foreach (var attendee in meeting.ReservationAttendees.Where(a => a.HasAccepted))
                    {
                        _notificationService.CreateNotification(
                            attendee.UserId.ToString(),
                            $"📅 Hatırlatma: '{meeting.Title}' toplantısı yarın saat {meeting.StartTime:hh\\:mm}'de. Salon: {meeting.Room}"
                        );
                    }
                }

                Logger.Info($"24-hour reminder sent for meeting: {meeting.Id} - {meeting.Title}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error sending 24-hour reminder for meeting {meeting.Id}");
            }
        }

        /// <summary>
        /// Hatırlatıcı daha önce gönderilmiş mi kontrol eder
        /// </summary>
        private bool HasReminderBeenSent(int reservationId, ReminderType type)
        {
            var reminderLog = _context.MeetingReminderLogs
                .FirstOrDefault(l => l.ReservationId == reservationId && l.ReminderType == type);

            return reminderLog != null;
        }

        /// <summary>
        /// Hatırlatıcı gönderildiğini loglar
        /// </summary>
        private void LogReminderSent(int reservationId, ReminderType type)
        {
            try
            {
                var log = new MeetingReminderLog
                {
                    ReservationId = reservationId,
                    ReminderType = type,
                    SentAt = DateTime.Now
                };

                _context.MeetingReminderLogs.Add(log);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error logging reminder for reservation {reservationId}");
            }
        }
    }

    /// <summary>
    /// Hatırlatıcı tipi
    /// </summary>
    public enum ReminderType
    {
        Hours24 = 1,
        Hour1 = 2,
        Minutes15 = 3
    }
}
