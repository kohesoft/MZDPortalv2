using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MZDNETWORK.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedDate { get; set; }
        
        // User navigation property (ignored in ApplicationDbContext due to string UserId)
        [NotMapped]
        public virtual User User { get; set; }
    }
}