using System;
using System.ComponentModel.DataAnnotations;

namespace MZDNETWORK.Models
{
    public class DilekOneri
    {
        [Key]
        public int Id { get; set; }
        public string Username { get; set; }
        [StringLength(500)]
        public string Mesaj { get; set; }
        [StringLength(500)]
        public string Bilidirim { get; set; }
        [DataType(DataType.Date)]
        public DateTime GonderimTarihi { get; set; } = DateTime.Now;
        public bool IsAnonymous { get; set; } // Yeni özellik eklendi

    }


}
