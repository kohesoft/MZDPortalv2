using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MZDNETWORK.Models
{
    /// <summary>
    /// Toplantý rezervasyon katýlýmcýlarý junction table
    /// Many-to-Many iliþkisi için: Reservation <-> User
    /// </summary>
    public class ReservationAttendee
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Ýlgili rezervasyon ID
        /// </summary>
        public int ReservationId { get; set; }

        [ForeignKey("ReservationId")]
        public virtual Reservation Reservation { get; set; }

        /// <summary>
        /// Katýlýmcý kullanýcý ID
        /// </summary>
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        /// <summary>
        /// Katýlýmcý opsiyonel mi (zorunlu mu)?
        /// </summary>
        public bool IsOptional { get; set; } = false;

        /// <summary>
        /// Katýlýmcý daveti kabul etti mi?
        /// </summary>
        public bool HasAccepted { get; set; } = false;

        /// <summary>
        /// Katýlýmcý daveti reddetti mi?
        /// </summary>
        public bool HasDeclined { get; set; } = false;

        /// <summary>
        /// Yanýt tarihi
        /// </summary>
        public DateTime? ResponseDate { get; set; }

        /// <summary>
        /// Kayýt oluþturulma tarihi
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
