using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace MZDNETWORK.Helpers
{
    /// <summary>
    /// Email gönderme iþlemleri için yardýmcý sýnýf
    /// </summary>
    public static class EmailHelper
    {
        private static readonly string SmtpServer = ConfigurationManager.AppSettings["SmtpServer"] ?? "smtp.gmail.com";
        private static readonly int SmtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"] ?? "587");
        private static readonly string SmtpUsername = ConfigurationManager.AppSettings["SmtpUsername"] ?? "";
        private static readonly string SmtpPassword = ConfigurationManager.AppSettings["SmtpPassword"] ?? "";
        private static readonly string FromEmail = ConfigurationManager.AppSettings["FromEmail"] ?? SmtpUsername;
        private static readonly string FromName = ConfigurationManager.AppSettings["FromName"] ?? "MZD Portal";

        /// <summary>
        /// Email gönderir (Asenkron)
        /// </summary>
        public static async Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
        {
            try
            {
                // Email adres kontrolü
                if (string.IsNullOrWhiteSpace(toEmail))
                {
                    System.Diagnostics.Debug.WriteLine($"[EMAIL ERROR] Alýcý email adresi boþ!");
                    return false;
                }

                // SMTP ayarlarý kontrolü
                if (string.IsNullOrEmpty(SmtpUsername) || string.IsNullOrEmpty(SmtpPassword))
                {
                    System.Diagnostics.Debug.WriteLine($"[EMAIL DEBUG] SMTP ayarlarý eksik - Email gönderilemiyor");
                    System.Diagnostics.Debug.WriteLine($"[EMAIL DEBUG] To: {toEmail}");
                    System.Diagnostics.Debug.WriteLine($"[EMAIL DEBUG] Subject: {subject}");
                    return false; // Üretim ortamýnda email gönderemezsek false dönelim
                }

                System.Diagnostics.Debug.WriteLine($"[EMAIL] Gönderiliyor... To: {toEmail}, Subject: {subject}");

                using (var client = new SmtpClient(SmtpServer, SmtpPort))
                {
                    // SSL/TLS ayarlarý
                    client.EnableSsl = true;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(SmtpUsername, SmtpPassword);
                    
                    // Timeout ayarlarý
                    client.Timeout = 30000; // 30 saniye
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(FromEmail, FromName),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = isHtml,
                        Priority = MailPriority.Normal
                    };

                    mailMessage.To.Add(toEmail);

                    await client.SendMailAsync(mailMessage);
                    
                    System.Diagnostics.Debug.WriteLine($"[EMAIL SUCCESS] Email baþarýyla gönderildi: {toEmail}");
                    return true;
                }
            }
            catch (SmtpException smtpEx)
            {
                System.Diagnostics.Debug.WriteLine($"[EMAIL SMTP ERROR] {smtpEx.Message}");
                System.Diagnostics.Debug.WriteLine($"[EMAIL SMTP ERROR] StatusCode: {smtpEx.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[EMAIL ERROR] {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[EMAIL ERROR] Stack: {ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Email gönderir (Senkron)
        /// </summary>
        public static bool SendEmail(string toEmail, string subject, string body, bool isHtml = true)
        {
            try
            {
                return SendEmailAsync(toEmail, subject, body, isHtml).Result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[EMAIL SYNC ERROR] {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Test email gönderir - SMTP ayarlarýný test etmek için
        /// </summary>
        public static bool SendTestEmail(string toEmail)
        {
            var subject = "MZD Portal - Test Email";
            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #4f46e5; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background: #f9fafb; padding: 20px; border: 1px solid #e5e7eb; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>? Test Email</h1>
        </div>
        <div class='content'>
            <p>Merhaba,</p>
            <p>Bu bir test emailidir. Email sistemimiz düzgün çalýþýyor!</p>
            <p><strong>Gönderim Zamaný:</strong> {DateTime.Now:dd.MM.yyyy HH:mm:ss}</p>
            <p><strong>SMTP Server:</strong> {SmtpServer}</p>
            <p><strong>SMTP Port:</strong> {SmtpPort}</p>
            <p><strong>Gönderen:</strong> {FromEmail}</p>
        </div>
    </div>
</body>
</html>";

            System.Diagnostics.Debug.WriteLine($"[EMAIL TEST] Test emaili gönderiliyor: {toEmail}");
            return SendEmail(toEmail, subject, body, true);
        }

        /// <summary>
        /// Toplantý daveti email þablonu
        /// </summary>
        public static string GetMeetingInvitationTemplate(string userName, string meetingTitle, string room, string date, string startTime, string endTime, string description, string organizer)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #4f46e5; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background: #f9fafb; padding: 20px; border: 1px solid #e5e7eb; }}
        .footer {{ background: #f3f4f6; padding: 15px; text-align: center; border-radius: 0 0 5px 5px; font-size: 12px; color: #6b7280; }}
        .info-box {{ background: white; padding: 15px; margin: 10px 0; border-left: 4px solid #4f46e5; }}
        .button {{ display: inline-block; padding: 12px 24px; background: #4f46e5; color: white; text-decoration: none; border-radius: 5px; margin: 10px 5px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>?? Toplantý Daveti</h1>
        </div>
        <div class='content'>
            <p>Merhaba <strong>{userName}</strong>,</p>
            <p>Bir toplantýya davet edildiniz.</p>
            
            <div class='info-box'>
                <h2 style='margin-top: 0; color: #4f46e5;'>{meetingTitle}</h2>
                <p><strong>?? Salon:</strong> {room}</p>
                <p><strong>?? Tarih:</strong> {date}</p>
                <p><strong>?? Saat:</strong> {startTime} - {endTime}</p>
                <p><strong>?? Organizatör:</strong> {organizer}</p>
                {(string.IsNullOrEmpty(description) ? "" : $"<p><strong>?? Açýklama:</strong> {description}</p>")}
            </div>
            
            <p style='text-align: center;'>
                <a href='#' class='button' style='background: #10b981;'>? Katýlacaðým</a>
                <a href='#' class='button' style='background: #ef4444;'>? Katýlamam</a>
            </p>
        </div>
        <div class='footer'>
            <p>Bu otomatik bir mesajdýr. Lütfen yanýtlamayýnýz.</p>
            <p>© {DateTime.Now.Year} MZD Portal. Tüm haklarý saklýdýr.</p>
        </div>
    </div>
</body>
</html>";
        }

        /// <summary>
        /// Toplantý onay bildirimi email þablonu
        /// </summary>
        public static string GetMeetingApprovalTemplate(string userName, string meetingTitle, string room, string date, string startTime, string endTime)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #10b981; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background: #f9fafb; padding: 20px; border: 1px solid #e5e7eb; }}
        .footer {{ background: #f3f4f6; padding: 15px; text-align: center; border-radius: 0 0 5px 5px; font-size: 12px; color: #6b7280; }}
        .success-box {{ background: #d1fae5; padding: 15px; margin: 10px 0; border-left: 4px solid #10b981; border-radius: 5px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>? Toplantý Onaylandý</h1>
        </div>
        <div class='content'>
            <p>Merhaba <strong>{userName}</strong>,</p>
            <p>Toplantý talebiniz onaylandý.</p>
            
            <div class='success-box'>
                <h2 style='margin-top: 0; color: #10b981;'>{meetingTitle}</h2>
                <p><strong>?? Salon:</strong> {room}</p>
                <p><strong>?? Tarih:</strong> {date}</p>
                <p><strong>?? Saat:</strong> {startTime} - {endTime}</p>
            </div>
            
            <p>Toplantý salonunuz rezerve edilmiþtir. Ýyi toplantýlar!</p>
        </div>
        <div class='footer'>
            <p>© {DateTime.Now.Year} MZD Portal. Tüm haklarý saklýdýr.</p>
        </div>
    </div>
</body>
</html>";
        }

        /// <summary>
        /// Toplantý red bildirimi email þablonu
        /// </summary>
        public static string GetMeetingRejectionTemplate(string userName, string meetingTitle, string reason)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #ef4444; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background: #f9fafb; padding: 20px; border: 1px solid #e5e7eb; }}
        .footer {{ background: #f3f4f6; padding: 15px; text-align: center; border-radius: 0 0 5px 5px; font-size: 12px; color: #6b7280; }}
        .warning-box {{ background: #fee2e2; padding: 15px; margin: 10px 0; border-left: 4px solid #ef4444; border-radius: 5px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>? Toplantý Reddedildi</h1>
        </div>
        <div class='content'>
            <p>Merhaba <strong>{userName}</strong>,</p>
            <p>Toplantý talebiniz reddedildi.</p>
            
            <div class='warning-box'>
                <h2 style='margin-top: 0; color: #ef4444;'>{meetingTitle}</h2>
                <p><strong>Red Nedeni:</strong> {reason}</p>
            </div>
            
            <p>Farklý bir tarih veya salon için yeni bir rezervasyon oluþturabilirsiniz.</p>
        </div>
        <div class='footer'>
            <p>© {DateTime.Now.Year} MZD Portal. Tüm haklarý saklýdýr.</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}
