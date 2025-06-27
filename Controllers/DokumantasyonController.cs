using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MZDNETWORK.Models;
using MZDNETWORK.Data;
using MZDNETWORK.Attributes;

namespace MZDNETWORK.Controllers
{
    public class DokumantasyonController : Controller
    {
        private MZDNETWORKContext db = new MZDNETWORKContext();

        // GET: Dokumantasyon
        public ActionResult Index()
        {
            return View(db.Dokumantasyons.ToList());
        }

        public ActionResult Index_sorumlu()
        {
            return View(db.Dokumantasyons.ToList());
        }

        // GET: Dokumantasyon/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Dokumantasyon dokumantasyon = db.Dokumantasyons.Find(id);
            if (dokumantasyon == null)
            {
                return HttpNotFound();
            }
            return View(dokumantasyon);
        }

        // GET: Dokumantasyon/Create
        [HttpGet]
        public ActionResult Create()
        {
            var model = new Dokumantasyon
            {
                Username = User.Identity.Name
            };
            return View(model);
        }

        // POST: Dokumantasyon/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Username,Id,TalepAdSoyad,TalepBirim,TalepUnvan,TalepTarihi,HamUreticiParcaNo,HamDatasheet,HamHammaddeTuru,HamStokBirimi,HamDokumantasyonTarafindanVerilenKod,MamulMamulDurum,MamulFirmaAdi,MamulMamulKodu,MamulMamulTuru,MamulStokTanimi,MamulDokumantasyonTarafindanVerilenKod,YariMamulFirmaAdi,YariMamulYarimamulTuru,YariMamulStokTanimi,YarimamulBagliMamulKodu,YarimamulDokumantasyonTarafindanVerilenKod,UretimDokFirmaAdi,UretimDokTur,UretimDokStokTanimi,UretimDokBagliMamulKodu,UretimDokumantasyonTarafindanVerilenKod,KablajTahtasiFirmaAdi,KablajTahtasiTuru,KablajTahtasiStokTanimi,KablajTahtasiBagliKod,KablajTahtasiDokumantasyonTarafindanVerilenKod,EpoksiFirmaAdi,EpoksiHammadeParcaNo,EpoksiYukseklik,EpoksiTur,EpoksiStokTanimi,EpoksiBagliKod,EpoksiDokumantasyonTarafindanVerilenKod,MakinaFirmaAdi,MakinaYatirimNo,MakinaHammadeParcaNo,MakinaTur,MakinaStokTanimi,MakinaBagliKod,MakinaDokumantasyonTarafindanVerilenKod,TestKonnektorTur,TestHammadeParcaNo,TestJigNoSiraNo,TestParcaAciklama,TestStokTanimi,TestDokumantasyonTarafindanVerilenKod,EpoksiHavFirmaAdi,EpoksiHavHammadeParcaNo,EpoksiHavEn,EpoksiHavBoy,EpoksiHavDerinlik,EpoksiHavTur,EpoksiHavStokTanimi,EpoksiHavBagliKod,EpoksiHavDokumantasyonTarafindanVerilenKod,DokumantasyonSorumlu,DokumantasyonTarih,KaliteYonetimTemsilcisi,KaliteYonetimTemsilcisiTarih")] Dokumantasyon dokumantasyon)
        {
            if (ModelState.IsValid)
            {
                dokumantasyon.Username = User.Identity.Name;

                db.Dokumantasyons.Add(dokumantasyon);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(dokumantasyon);
        }

        // GET: Dokumantasyon/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Dokumantasyon dokumantasyon = db.Dokumantasyons.Find(id);
            if (dokumantasyon == null)
            {
                return HttpNotFound();
            }
            return View(dokumantasyon);
        }

        // POST: Dokumantasyon/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,TalepAdSoyad,TalepBirim,TalepUnvan,TalepTarihi,HamUreticiParcaNo,HamDatasheet,HamHammaddeTuru,HamStokBirimi,HamDokumantasyonTarafindanVerilenKod,MamulMamulDurum,MamulFirmaAdi,MamulMamulKodu,MamulMamulTuru,MamulStokTanimi,MamulDokumantasyonTarafindanVerilenKod,YariMamulFirmaAdi,YariMamulYarimamulTuru,YariMamulStokTanimi,YarimamulBagliMamulKodu,YarimamulDokumantasyonTarafindanVerilenKod,UretimDokFirmaAdi,UretimDokTur,UretimDokStokTanimi,UretimDokBagliMamulKodu,UretimDokumantasyonTarafindanVerilenKod,KablajTahtasiFirmaAdi,KablajTahtasiTuru,KablajTahtasiStokTanimi,KablajTahtasiBagliKod,KablajTahtasiDokumantasyonTarafindanVerilenKod,EpoksiFirmaAdi,EpoksiHammadeParcaNo,EpoksiYukseklik,EpoksiTur,EpoksiStokTanimi,EpoksiBagliKod,EpoksiDokumantasyonTarafindanVerilenKod,MakinaFirmaAdi,MakinaYatirimNo,MakinaHammadeParcaNo,MakinaTur,MakinaStokTanimi,MakinaBagliKod,MakinaDokumantasyonTarafindanVerilenKod,TestKonnektorTur,TestHammadeParcaNo,TestJigNoSiraNo,TestParcaAciklama,TestStokTanimi,TestDokumantasyonTarafindanVerilenKod,EpoksiHavFirmaAdi,EpoksiHavHammadeParcaNo,EpoksiHavEn,EpoksiHavBoy,EpoksiHavDerinlik,EpoksiHavTur,EpoksiHavStokTanimi,EpoksiHavBagliKod,EpoksiHavDokumantasyonTarafindanVerilenKod,DokumantasyonSorumlu,DokumantasyonTarih,KaliteYonetimTemsilcisi,KaliteYonetimTemsilcisiTarih")] Dokumantasyon dokumantasyon)
        {
            if (ModelState.IsValid)
            {
                var existingDokumantasyon = db.Dokumantasyons.AsNoTracking().FirstOrDefault(d => d.Id == dokumantasyon.Id);
                if (existingDokumantasyon != null)
                {
                    dokumantasyon.Username = existingDokumantasyon.Username;
                }

                db.Entry(dokumantasyon).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index_sorumlu");
            }
            return View(dokumantasyon);
        }

        // GET: Dokumantasyon/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Dokumantasyon dokumantasyon = db.Dokumantasyons.Find(id);
            if (dokumantasyon == null)
            {
                return HttpNotFound();
            }
            return View(dokumantasyon);
        }

        // POST: Dokumantasyon/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Dokumantasyon dokumantasyon = db.Dokumantasyons.Find(id);
            db.Dokumantasyons.Remove(dokumantasyon);
            db.SaveChanges();
            return RedirectToAction("Index_sorumlu");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
