using System;
using System.Net;
using System.Net.Mail;
using System.Configuration;
using System.Threading.Tasks;
using NLog;

namespace MZDNETWORK.Helpers
{
    /// <summary>
    /// Email gönderim servisi
    /// </summary>
    public class EmailService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly bool _enableSsl;

        public EmailService()
        {
            // Web.config'den SMTP ayarlarını al
            _smtpServer = ConfigurationManager.AppSettings["SmtpServer"] ?? "smtp.gmail.com";
            _smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"] ?? "587");
            _smtpUsername = ConfigurationManager.AppSettings["SmtpUsername"] ?? "";
            _smtpPassword = ConfigurationManager.AppSettings["SmtpPassword"] ?? "";
            _fromEmail = ConfigurationManager.AppSettings["FromEmail"] ?? _smtpUsername;
            _fromName = ConfigurationManager.AppSettings["FromName"] ?? "MZD Portal";
            _enableSsl = bool.Parse(ConfigurationManager.AppSettings["SMTP_EnableSSL"] ?? "true");
        }

        /// <summary>
        /// Email gönderir
        /// </summary>
        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
        {
            try
            {
                if (string.IsNullOrEmpty(_smtpUsername) || string.IsNullOrEmpty(_smtpPassword))
                {
                    Logger.Warn("SMTP credentials not configured. Email not sent.");
                    return false;
                }

                using (var message = new MailMessage())
                {
                    message.From = new MailAddress(_fromEmail, _fromName);
                    message.To.Add(new MailAddress(toEmail));
                    message.Subject = subject;
                    message.Body = body;
                    message.IsBodyHtml = isHtml;

                    using (var smtpClient = new SmtpClient(_smtpServer, _smtpPort))
                    {
                        smtpClient.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                        smtpClient.EnableSsl = _enableSsl;

                        await smtpClient.SendMailAsync(message);
                        Logger.Info($"Email sent successfully to {toEmail}");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to send email to {toEmail}");
                return false;
            }
        }

        /// <summary>
        /// Toplantı oluşturulduğunda organizatöre email gönderir
        /// </summary>
        public async Task SendMeetingCreatedEmailAsync(string toEmail, string userName, string meetingTitle, string room, DateTime date, TimeSpan startTime)
        {
            var subject = "Toplantı Talebiniz Alındı - MZD Portal";
            var body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f9f9f9; }}
                        .header {{ background-color: #4f46e5; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
                        .content {{ background-color: white; padding: 30px; border-radius: 0 0 8px 8px; }}
                        .info-box {{ background-color: #f3f4f6; padding: 15px; border-left: 4px solid #4f46e5; margin: 20px 0; }}
                        .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Toplantı Talebi Alındı</h1>
                        </div>
                        <div class='content'>
                            <p>Merhaba {userName},</p>
                            <p>Toplantı rezervasyon talebiniz başarıyla alınmıştır ve onay beklemektedir.</p>
                            <div class='info-box'>
                                <strong>Toplantı Detayları:</strong><br>
                                📅 <strong>Başlık:</strong> {meetingTitle}<br>
                                🏢 <strong>Salon:</strong> {room}<br>
                                📆 <strong>Tarih:</strong> {date:dd.MM.yyyy}<br>
                                🕐 <strong>Saat:</strong> {startTime:hh\\:mm}<br>
                            </div>
                            <p>Talebiniz yetkili kişi tarafından değerlendirildikten sonra size bilgi verilecektir.</p>
                            <p>Teşekkürler,<br>MZD Portal</p>
                        </div>
                        <div class='footer'>
                            Bu otomatik bir mesajdır, lütfen yanıtlamayın.
                        </div>
                    </div>
                </body>
                </html>
            ";

            await SendEmailAsync(toEmail, subject, body, true);
        }

        /// <summary>
        /// Toplantı onaylandığında email gönderir
        /// </summary>
        public async Task SendMeetingApprovedEmailAsync(string toEmail, string userName, string meetingTitle, string room, DateTime date, TimeSpan startTime, TimeSpan endTime)
        {
            var subject = "✅ Toplantınız Onaylandı - MZD Portal";
            var body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f9f9f9; }}
                        .header {{ background-color: #10b981; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
                        .content {{ background-color: white; padding: 30px; border-radius: 0 0 8px 8px; }}
                        .info-box {{ background-color: #d1fae5; padding: 15px; border-left: 4px solid #10b981; margin: 20px 0; }}
                        .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>✅ Toplantınız Onaylandı!</h1>
                        </div>
                        <div class='content'>
                            <p>Merhaba {userName},</p>
                            <p>Toplantı rezervasyonunuz onaylanmıştır.</p>
                            <div class='info-box'>
                                <strong>Toplantı Detayları:</strong><br>
                                📅 <strong>Başlık:</strong> {meetingTitle}<br>
                                🏢 <strong>Salon:</strong> {room}<br>
                                📆 <strong>Tarih:</strong> {date:dd.MM.yyyy}<br>
                                🕐 <strong>Saat:</strong> {startTime:hh\\:mm} - {endTime:hh\\:mm}<br>
                            </div>
                            <p>Toplantınızdan 15 dakika önce hatırlatma alacaksınız.</p>
                            <p>Teşekkürler,<br>MZD Portal</p>
                        </div>
                        <div class='footer'>
                            Bu otomatik bir mesajdır, lütfen yanıtlamayın.
                        </div>
                    </div>
                </body>
                </html>
            ";

            await SendEmailAsync(toEmail, subject, body, true);
        }

        /// <summary>
        /// Toplantı reddedildiğinde email gönderir
        /// </summary>
        public async Task SendMeetingRejectedEmailAsync(string toEmail, string userName, string meetingTitle, string room, DateTime date, TimeSpan startTime, string reason)
        {
            var subject = "❌ Toplantı Talebiniz Reddedildi - MZD Portal";
            var body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f9f9f9; }}
                        .header {{ background-color: #ef4444; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
                        .content {{ background-color: white; padding: 30px; border-radius: 0 0 8px 8px; }}
                        .info-box {{ background-color: #fee2e2; padding: 15px; border-left: 4px solid #ef4444; margin: 20px 0; }}
                        .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Toplantı Talebi Reddedildi</h1>
                        </div>
                        <div class='content'>
                            <p>Merhaba {userName},</p>
                            <p>Üzgünüz, toplantı rezervasyon talebiniz reddedilmiştir.</p>
                            <div class='info-box'>
                                <strong>Toplantı Detayları:</strong><br>
                                📅 <strong>Başlık:</strong> {meetingTitle}<br>
                                🏢 <strong>Salon:</strong> {room}<br>
                                📆 <strong>Tarih:</strong> {date:dd.MM.yyyy}<br>
                                🕐 <strong>Saat:</strong> {startTime:hh\\:mm}<br>
                                <br>
                                <strong>Red Nedeni:</strong> {reason ?? "Belirtilmedi"}
                            </div>
                            <p>Başka bir tarih ve saat seçerek tekrar rezervasyon oluşturabilirsiniz.</p>
                            <p>Teşekkürler,<br>MZD Portal</p>
                        </div>
                        <div class='footer'>
                            Bu otomatik bir mesajdır, lütfen yanıtlamayın.
                        </div>
                    </div>
                </body>
                </html>
            ";

            await SendEmailAsync(toEmail, subject, body, true);
        }

        /// <summary>
        /// Toplantı hatırlatıcı emaili gönderir
        /// </summary>
        public async Task SendMeetingReminderEmailAsync(string toEmail, string userName, string meetingTitle, string room, DateTime date, TimeSpan startTime, int minutesBefore)
        {
            var subject = $"⏰ Toplantı Hatırlatıcı ({minutesBefore} dakika kaldı) - MZD Portal";
            var body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f9f9f9; }}
                        .header {{ background-color: #f59e0b; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
                        .content {{ background-color: white; padding: 30px; border-radius: 0 0 8px 8px; }}
                        .info-box {{ background-color: #fef3c7; padding: 15px; border-left: 4px solid #f59e0b; margin: 20px 0; }}
                        .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>⏰ Toplantı Hatırlatıcı</h1>
                        </div>
                        <div class='content'>
                            <p>Merhaba {userName},</p>
                            <p><strong>Toplantınız {minutesBefore} dakika sonra başlayacak!</strong></p>
                            <div class='info-box'>
                                <strong>Toplantı Detayları:</strong><br>
                                📅 <strong>Başlık:</strong> {meetingTitle}<br>
                                🏢 <strong>Salon:</strong> {room}<br>
                                📆 <strong>Tarih:</strong> {date:dd.MM.yyyy}<br>
                                🕐 <strong>Saat:</strong> {startTime:hh\\:mm}<br>
                            </div>
                            <p>Lütfen toplantıya zamanında katılın.</p>
                            <p>Teşekkürler,<br>MZD Portal</p>
                        </div>
                        <div class='footer'>
                            Bu otomatik bir mesajdır, lütfen yanıtlamayın.
                        </div>
                    </div>
                </body>
                </html>
            ";

            await SendEmailAsync(toEmail, subject, body, true);
        }
    }
}
