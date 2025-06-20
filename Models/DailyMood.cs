using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MZDNETWORK.Models
{
    public class DailyMood
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [Range(1,5)]
        public int Mood { get; set; }

        [MaxLength(1000)]
        public string Comment { get; set; }
    }
} 