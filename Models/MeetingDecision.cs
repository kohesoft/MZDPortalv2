using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MZDNETWORK.Models
{
    /// <summary>
    /// Toplantý kararlarý tablosu
    /// </summary>
    public class MeetingDecision
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
        /// Karar baþlýðý
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        /// <summary>
        /// Karar açýklamasý / detayý
        /// </summary>
        [Required]
        public string Description { get; set; }

        /// <summary>
        /// Sorumlu kiþi(ler)
        /// </summary>
        [StringLength(500)]
        public string ResponsiblePerson { get; set; }

        /// <summary>
        /// Tamamlanma tarihi (hedef)
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Karar durumu
        /// </summary>
        public DecisionStatus Status { get; set; } = DecisionStatus.Pending;

        /// <summary>
        /// Karar sýrasý (toplantýda kaçýncý karar)
        /// </summary>
        public int OrderIndex { get; set; }

        /// <summary>
        /// Kararý oluþturan kullanýcý ID
        /// </summary>
        public int CreatedByUserId { get; set; }

        /// <summary>
        /// Kararý oluþturan kullanýcý adý
        /// </summary>
        [StringLength(200)]
        public string CreatedByUserName { get; set; }

        /// <summary>
        /// Oluþturulma tarihi
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Güncellenme tarihi
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Tamamlanma tarihi (gerçek)
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Notlar
        /// </summary>
        public string Notes { get; set; }
    }

    /// <summary>
    /// Karar durumu
    /// </summary>
    public enum DecisionStatus
    {
        Pending = 0,      // Beklemede
        InProgress = 1,   // Devam ediyor
        Completed = 2,    // Tamamlandý
        Cancelled = 3     // Ýptal edildi
    }
}
