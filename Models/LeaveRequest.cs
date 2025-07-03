using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MZDNETWORK.Models
{
    public class LeaveRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("RequestingUser")]
        public int UserId { get; set; }
        public virtual User RequestingUser { get; set; }

        [Required]
        [Display(Name = "Başlangıç Tarihi")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "Bitiş Tarihi")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required]
        [Display(Name = "İzin Türü")]
        public LeaveType LeaveType { get; set; }

        [Required]
        [Display(Name = "İzin Nedeni")]
        [StringLength(500)]
        public string Reason { get; set; }

        [Display(Name = "Açıklama")]
        [StringLength(1000)]
        public string Description { get; set; }

        [Display(Name = "Durum")]
        public LeaveStatus Status { get; set; }

        [Display(Name = "Onay/Red Nedeni")]
        [StringLength(500)]
        public string ApprovalReason { get; set; }

        [ForeignKey("ApprovedBy")]
        public int? ApprovedById { get; set; }
        public virtual User ApprovedBy { get; set; }

        [Display(Name = "Talep Tarihi")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [Display(Name = "Güncellenme Tarihi")]
        public DateTime? UpdatedAt { get; set; }

        [NotMapped]
        public int TotalDays => (EndDate - StartDate).Days + 1;

        [NotMapped]
        public double TotalHours => (EndDate - StartDate).TotalHours;

        [NotMapped]
        public string DurationDisplay => TotalHours >= 24 ? $"{TotalDays} gün" : $"{Math.Round(TotalHours, 1)} saat";

        [Display(Name = "İletişim Bilgisi")]
        [StringLength(200)]
        public string ContactInfo { get; set; }

        [Display(Name = "Departman")]
        [StringLength(100)]
        public string Department { get; set; }

        [Display(Name = "Yerine Bakacak Kişi")]
        [StringLength(200)]
        public string SubstituteName { get; set; }

        [Display(Name = "Devredilecek Görevler")]
        [StringLength(1000)]
        public string Tasks { get; set; }
    }

    public enum LeaveType
    {
        [Display(Name = "Yıllık İzin")]
        Annual = 1,
        [Display(Name = "Hastalık İzni")]
        Sick = 2,
        [Display(Name = "Mazeret İzni")]
        Excuse = 3,
        [Display(Name = "Ücretsiz İzin")]
        Unpaid = 4,
        [Display(Name = "Diğer")]
        Other = 5
    }

    public enum LeaveStatus
    {
        // Çalışanın talebi ilk oluşturulduğunda şefin onayına gider
        [Display(Name = "Şef Onayı Bekliyor")]
        PendingSupervisor = 0,

        [Display(Name = "Onaylandı")]
        Approved = 1,

        [Display(Name = "Şartlı Onaylandı")]
        ConditionallyApproved = 2,

        [Display(Name = "Reddedildi")]
        Rejected = 3,

        // Şef onayladıktan sonra müdürün onayı beklenir (yeni durum)
        [Display(Name = "Müdür Onayı Bekliyor")]
        PendingManager = 4
    }
} 