using System;
using System.ComponentModel.DataAnnotations;

namespace MZDNETWORK.Models
{
    /// <summary>
    /// İşe Geç Gelme Tutanak kaydı
    /// </summary>
    public class LateArrivalReport
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string FullName { get; set; } // Adı Soyadı

        [StringLength(150)]
        public string Department { get; set; } // Birimi

        [StringLength(100)]
        public string Title { get; set; } // Ünvanı

        [Required]
        public TimeSpan ShiftStartTime { get; set; } // Mesai Başlangıç Saati

        [Required]
        public TimeSpan ArrivalTime { get; set; } // İşe Geldiği Saat

        [Required]
        public DateTime LateDate { get; set; } // İşe Geç Gelme Tarihi (yyyy-MM-dd)

        public string DefenseText { get; set; } // Personelin savunması

        public string SignaturePath { get; set; } // İmza (Base64 PNG)

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
} 