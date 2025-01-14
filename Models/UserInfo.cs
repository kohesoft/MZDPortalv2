using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MZDNETWORK.Models
{
    public class UserInfo
    {

        public int Id { get; set; }
        public string Email { get; set; }
        public string RealPhoneNumber { get; set; }
        public string Adres { get; set; }
        public string Adres2 { get; set; }
        public string Sehir { get; set; }
        public string Ulke { get; set; }
        public string Postakodu { get; set; }
        public string KanGrubu { get; set; }
        public string DogumTarihi { get; set; }
        public string Cinsiyet { get; set; }
        public string MedeniDurum { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

    }
}