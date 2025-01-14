using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Policy;
using System.Web;

namespace MZDNETWORK.Models
{
    public class Dokumantasyon
    {
        [Key]
        public int Id { get; set; }
        public string Username { get; set; }
        public string TalepAdSoyad { get; set; }
        public string TalepBirim { get; set; }
        public string TalepUnvan { get; set; }
        public DateTime TalepTarihi { get; set; } = DateTime.Now;

        public string HamUreticiParcaNo { get; set; }
        public string HamDatasheet { get; set; }
        public string HamHammaddeTuru { get; set; }
        public string HamStokBirimi { get; set; }
        public string HamDokumantasyonTarafindanVerilenKod { get; set; }

        public string MamulFirmaAdi { get; set; }
        public string MamulMamulKodu { get; set; }
        public string MamulMamulTuru { get; set; }
        public string MamulStokTanimi { get; set; }
        public string MamulDokumantasyonTarafindanVerilenKod { get; set; }

        public string YariMamulFirmaAdi { get; set; }
        public string YariMamulYarimamulTuru { get; set; }
        public string YariMamulStokTanimi { get; set; }
        public string YarimamulBagliMamulKodu { get; set; }
        public string YarimamulDokumantasyonTarafindanVerilenKod { get; set; }

        public string UretimDokFirmaAdi { get; set; }
        public string UretimDokTur { get; set; }
        public string UretimDokStokTanimi { get; set; }
        public string UretimDokBagliMamulKodu { get; set; }
        public string UretimDokumantasyonTarafindanVerilenKod { get; set; }

        public string KablajTahtasiFirmaAdi { get; set; }
        public string KablajTahtasiTuru { get; set; }
        public string KablajTahtasiStokTanimi { get; set; }
        public string KablajTahtasiBagliKod { get; set; }
        public string KablajTahtasiDokumantasyonTarafindanVerilenKod { get; set; }

        public string EpoksiFirmaAdi { get; set; }
        public string EpoksiHammadeParcaNo { get; set; }
        public string EpoksiYukseklik { get; set; }
        public string EpoksiTur { get; set; }
        public string EpoksiStokTanimi { get; set; }
        public string EpoksiBagliKod { get; set; }
        public string EpoksiDokumantasyonTarafindanVerilenKod { get; set; }

        public string MakinaFirmaAdi { get; set; }
        public string MakinaYatirimNo { get; set; }
        public string MakinaHammadeParcaNo { get; set; }
        public string MakinaTur { get; set; }
        public string MakinaStokTanimi { get; set; }
        public string MakinaBagliKod { get; set; }
        public string MakinaDokumantasyonTarafindanVerilenKod { get; set; }

        public string TestKonnektorTur { get; set; }
        public string TestHammadeParcaNo { get; set; }
        public string TestJigNoSiraNo { get; set; }
        public string TestParcaAciklama { get; set; }
        public string TestStokTanimi { get; set; }
        public string TestDokumantasyonTarafindanVerilenKod { get; set; }

        public string EpoksiHavFirmaAdi { get; set; }
        public string EpoksiHavHammadeParcaNo { get; set; }
        public string EpoksiHavEn { get; set; }
        public string EpoksiHavBoy { get; set; }
        public string EpoksiHavDerinlik { get; set; }
        public string EpoksiHavTur { get; set; }
        public string EpoksiHavStokTanimi { get; set; }
        public string EpoksiHavBagliKod { get; set; }
        public string EpoksiHavDokumantasyonTarafindanVerilenKod { get; set; }

        public string DokumantasyonSorumlu { get; set; }
        public string DokumantasyonTarih { get; set; }

        public string KaliteYonetimTemsilcisi { get; set; }
        public string KaliteYonetimTemsilcisiTarih { get; set; }
    }
}