using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MZDNETWORK.Models
{
    public class BeyazTahtaEntry
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }

        public string OneriVeren { get; set; }
        public string Problem { get; set; }
        public string Oneri { get; set; }
        
        // User relationship
        public int? UserId { get; set; }
        
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}