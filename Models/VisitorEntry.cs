using System;
using System.ComponentModel.DataAnnotations;

namespace MZDNETWORK.Models
{
    /// <summary>
    /// Ziyaretçi / Giriş çıkış form satırı
    /// </summary>
    public class VisitorEntry
    {
        [Key]
        public int Id { get; set; } // Sıra No

        [Required]
        public DateTime Date { get; set; } // Tarih

        [Required, StringLength(150)]
        public string FullName { get; set; } // Adı Soyadı

        [StringLength(150)]
        public string Organization { get; set; } // Organizasyon

        [StringLength(100)]
        public string Duty { get; set; } // Görevi

        [StringLength(20)]
        public string IdentityNo { get; set; } // Kimlik No (Yaka Kartı vb.)

        [StringLength(11)]
        public string TCKimlik { get; set; } // TC Kimlik

        [Required]
        public TimeSpan EntryTime { get; set; } // Giriş Saati

        public TimeSpan? ExitTime { get; set; } // Çıkış Saati

        // İmza verisi (Base64 PNG)
        public string SignaturePath { get; set; }

        public bool Approved { get; set; } = false; // Onay

        [StringLength(100)]
        public string ArrivalReason { get; set; } // Geliş Sebebi
    }
} 