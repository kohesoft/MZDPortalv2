using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MZDNETWORK.Models
{
    /// <summary>
    /// Gonderi (Announcement) attachments model
    /// </summary>
    public class GonderiEk
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int GonderiId { get; set; }

        [ForeignKey("GonderiId")]
        public virtual Gonderi Gonderi { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Dosya Adý")]
        public string FileName { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Orijinal Dosya Adý")]
        public string OriginalFileName { get; set; }

        [Required]
        [StringLength(500)]
        [Display(Name = "Dosya Yolu")]
        public string FilePath { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Dosya Türü")]
        public string FileType { get; set; }

        [Display(Name = "Dosya Boyutu (Bytes)")]
        public long FileSize { get; set; }

        [Display(Name = "Yüklenme Tarihi")]
        public DateTime UploadedAt { get; set; } = DateTime.Now;

        [Display(Name = "Ýndirilme Sayýsý")]
        public int DownloadCount { get; set; } = 0;

        [Display(Name = "Aktif mi?")]
        public bool IsActive { get; set; } = true;

        [StringLength(500)]
        [Display(Name = "Açýklama")]
        public string Description { get; set; }
    }
}