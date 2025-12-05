using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MZDNETWORK.Helpers;

namespace MZDNETWORK.Models
{
    /// <summary>
    /// Toplantı hatırlatıcı logları - Aynı hatırlatıcının tekrar gönderilmemesi için
    /// </summary>
    public class MeetingReminderLog
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// İlgili rezervasyon ID
        /// </summary>
        public int ReservationId { get; set; }

        [ForeignKey("ReservationId")]
        public virtual Reservation Reservation { get; set; }

        /// <summary>
        /// Hatırlatıcı tipi (24 saat, 1 saat, 15 dakika)
        /// </summary>
        public ReminderType ReminderType { get; set; }

        /// <summary>
        /// Hatırlatıcının gönderilme zamanı
        /// </summary>
        public DateTime SentAt { get; set; }
    }
}
