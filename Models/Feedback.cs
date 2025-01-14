using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MZDNETWORK.Models
{
    public class Feedback
    {
        public int Id { get; set; }
        public bool Liked { get; set; }
        public string Suggestion { get; set; }
        public DateTime CreatedAt { get; set; }
        public string IpAddress { get; set; } // IP adresi eklendi
    }

}