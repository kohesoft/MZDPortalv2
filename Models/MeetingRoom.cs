using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MZDNETWORK.Models
{
    /// <summary>
    /// Toplantı Salonu modeli
    /// </summary>
    [Table("MeetingRooms")]
    public class MeetingRoom
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Salon Adı")]
        public string Name { get; set; }

        [StringLength(200)]
        [Display(Name = "Konum")]
        public string Location { get; set; }

        [Display(Name = "Kapasite")]
        public int Capacity { get; set; }

        [StringLength(500)]
        [Display(Name = "Açıklama")]
        public string Description { get; set; }

        [StringLength(500)]
        [Display(Name = "Özellikler")]
        public string Features { get; set; }

        [StringLength(50)]
        [Display(Name = "Renk Kodu")]
        public string ColorCode { get; set; }

        [Display(Name = "Aktif mi?")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Sıra")]
        public int OrderIndex { get; set; }

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Güncellenme Tarihi")]
        public DateTime? UpdatedAt { get; set; }
    }
}
