using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MZDNETWORK.Models
{
    /// <summary>
    /// Toplantı aksiyon tablosu
    /// </summary>
    public class MeetingAction
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
        /// Aksiyon başlığı
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        /// <summary>
        /// Aksiyon açıklaması / detayı
        /// </summary>
        [Required]
        public string Description { get; set; }

        /// <summary>
        /// Sorumlu kişi(ler)
        /// </summary>
        [StringLength(500)]
        public string ResponsiblePerson { get; set; }

        /// <summary>
        /// Tamamlanma tarihi (hedef)
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Aksiyon durumu
        /// </summary>
        public ActionStatus Status { get; set; } = ActionStatus.Pending;

        /// <summary>
        /// Aksiyon sırası (toplantıda kaçıncı aksiyon)
        /// </summary>
        public int OrderIndex { get; set; }

        /// <summary>
        /// Aksiyonu oluşturan kullanıcı ID
        /// </summary>
        public int CreatedByUserId { get; set; }

        /// <summary>
        /// Aksiyonu oluşturan kullanıcı adı
        /// </summary>
        [StringLength(200)]
        public string CreatedByUserName { get; set; }

        /// <summary>
        /// Oluşturulma tarihi
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
    /// Aksiyon durumu
    /// </summary>
    public enum ActionStatus
    {
        Pending = 0,      // Beklemede
        InProgress = 1,   // Devam ediyor
        Completed = 2,    // Tamamlandı
        Cancelled = 3     // İptal edildi
    }
}
