using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MZDNETWORK.Models
{
    /// <summary>
    /// Toplantı Odası Rezervasyon modeli
    /// </summary>
    public class MeetingRoomReservation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Oda Adı")]
        public string RoomName { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Toplantı Başlığı")]
        public string Title { get; set; }

        [Column(TypeName = "text")]
        [Display(Name = "Açıklama")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Başlangıç Tarihi")]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "Bitiş Tarihi")]
        public DateTime EndDate { get; set; }

        [Display(Name = "Katılımcı Sayısı")]
        public int AttendeeCount { get; set; }

        [StringLength(500)]
        [Display(Name = "Katılımcılar")]
        public string Attendees { get; set; }

        [StringLength(50)]
        [Display(Name = "Durum")]
        public string Status { get; set; } = "Aktif"; // Aktif, İptal, Tamamlandı

        [StringLength(200)]
        [Display(Name = "Ekipman İhtiyaçları")]
        public string EquipmentNeeds { get; set; }

        [Display(Name = "Tekrarlanan Toplantı mı?")]
        public bool IsRecurring { get; set; } = false;

        [StringLength(50)]
        [Display(Name = "Tekrar Türü")]
        public string RecurrenceType { get; set; } // Günlük, Haftalık, Aylık

        [Display(Name = "Tekrar Bitiş Tarihi")]
        public DateTime? RecurrenceEndDate { get; set; }

        [Display(Name = "Onaylandı mı?")]
        public bool IsApproved { get; set; } = true;

        public int? ApprovedBy { get; set; }

        [ForeignKey("ApprovedBy")]
        public virtual User Approver { get; set; }

        [Display(Name = "Onay Tarihi")]
        public DateTime? ApprovedAt { get; set; }

        [StringLength(500)]
        [Display(Name = "Onay Notu")]
        public string ApprovalNote { get; set; }

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Güncellenme Tarihi")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "Aktif mi?")]
        public bool IsActive { get; set; } = true;

        [StringLength(50)]
        [Display(Name = "Öncelik")]
        public string Priority { get; set; } = "Normal"; // Düşük, Normal, Yüksek

        [Display(Name = "Hatırlatma")]
        public int ReminderMinutes { get; set; } = 15;

        [Display(Name = "Hatırlatma Gönderildi mi?")]
        public bool ReminderSent { get; set; } = false;
    }
} 