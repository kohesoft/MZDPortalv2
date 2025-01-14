using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MZDNETWORK.Models;

namespace MZDNETWORK.Controllers
{
   [Authorize(Roles = "IK, Yonetici, Sys, IdariIsler")]
    public class GonderiController : Controller
    {
        private MZDNETWORKContext db = new MZDNETWORKContext(); // Veritabanı context'i


        public ActionResult GonderiOlustur()
        {
            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult GonderiOlustur(Gonderi model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    model.CreatedAt = DateTime.Now; // Kayıt zamanını ayarlayın
                    db.Gonderiler.Add(model); // Gonderiler, veritabanı context'inde Gonderi tablosuna karşılık gelir
                    db.SaveChanges();
                    return RedirectToAction("GonderiListele"); // Kayıt başarılı olursa yönlendirilecek sayfa
                }
            }
            catch (Exception ex)
            {
                // Hata mesajını loglayabilir veya kullanıcıya gösterebilirsiniz
                ModelState.AddModelError("", "Bir hata oluştu: " + ex.Message);
            }

            return View(model);
        }

        // Gönderi Listeleme
        public ActionResult GonderiListele()
        {
            var gonderiler = db.Gonderiler.ToList();
            return View(gonderiler);
        }

        // Gönderi Silme
        [HttpGet]
        public ActionResult GonderiSil(int id)
        {
            var gonderi = db.Gonderiler.Find(id);
            if (gonderi == null)
            {
                return HttpNotFound();
            }
            return View(gonderi);
        }

        [HttpPost, ActionName("GonderiSil")]
        [ValidateAntiForgeryToken]
        public ActionResult GonderiSilConfirmed(int id)
        {
            var gonderi = db.Gonderiler.Find(id);
            if (gonderi != null)
            {
                db.Gonderiler.Remove(gonderi);
                db.SaveChanges();
            }
            return RedirectToAction("GonderiListele");
        }
    }
}
