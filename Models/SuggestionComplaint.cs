using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MZDNETWORK.Models
{
    /// <summary>
    /// Öneri ve Şikayet modeli
    /// </summary>
    public class SuggestionComplaint
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [Required]
        [StringLength(500)]
        [Display(Name = "Başlık")]
        public string Title { get; set; }

        [Required]
        [Column(TypeName = "text")]
        [Display(Name = "İçerik")]
        public string Content { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Tür")]
        public string Type { get; set; } // "Öneri" veya "Şikayet"

        [StringLength(100)]
        [Display(Name = "Kategori")]
        public string Category { get; set; }

        [Display(Name = "Anonim mi?")]
        public bool IsAnonymous { get; set; } = false;

        [StringLength(50)]
        [Display(Name = "Durum")]
        public string Status { get; set; } = "Beklemede"; // Beklemede, İnceleniyor, Tamamlandı, Reddedildi

        [StringLength(100)]
        [Display(Name = "Öncelik")]
        public string Priority { get; set; } = "Normal"; // Düşük, Normal, Yüksek, Kritik

        [Column(TypeName = "text")]
        [Display(Name = "Yönetici Yanıtı")]
        public string AdminResponse { get; set; }

        public int? ReviewedBy { get; set; }

        [ForeignKey("ReviewedBy")]
        public virtual User Reviewer { get; set; }

        [Display(Name = "İnceleme Tarihi")]
        public DateTime? ReviewedAt { get; set; }

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Güncellenme Tarihi")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "Aktif mi?")]
        public bool IsActive { get; set; } = true;

        [StringLength(200)]
        [Display(Name = "Etiketler")]
        public string Tags { get; set; }

        [Display(Name = "Oy Sayısı")]
        public int VoteCount { get; set; } = 0;

        [Display(Name = "Görüntülenme Sayısı")]
        public int ViewCount { get; set; } = 0;
    }
} 