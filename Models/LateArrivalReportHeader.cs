using System;
using System.ComponentModel.DataAnnotations;

namespace MZDNETWORK.Models
{
    /// <summary>
    /// İşe Geç Gelme Tutanak formu üst bilgi
    /// </summary>
    public class LateArrivalReportHeader
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string DocumentNo { get; set; }

        [Required]
        public DateTime FirstPublishDate { get; set; }

        [Required]
        public DateTime RevisionDate { get; set; }

        [Required, StringLength(20)]
        public string RevisionNo { get; set; }

        [StringLength(300)]
        public string PrintableNote { get; set; }
    }
} 