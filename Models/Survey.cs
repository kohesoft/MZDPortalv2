using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MZDNETWORK.Models
{
    public class Survey
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Duration { get; set; } // Süre (dakika cinsinden)
        public DateTime EndDate { get; set; } // Bitiş Tarihi

        public virtual List<Question> Questions { get; set; } // ICollection yerine List kullanıldı
    }


}