using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MZDNETWORK.Models;

namespace MZDNETWORK.Views.InsanKaynaklari
{
    [Authorize(Roles = "BilgiIslem, Yonetici, Sys")]
    public class Kullanici_IslemleriController : Controller
    {
        private MZDNETWORKContext db = new MZDNETWORKContext();

        // GET: IK_Kullanici
        public ActionResult Index()
        {
            return View(db.Users.Include(u => u.UserInfo).ToList());
        }

        // GET: IK_Kullanici/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Include(u => u.UserInfo).FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: IK_Kullanici/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: IK_Kullanici/Create
        // Aşırı gönderim saldırılarından korunmak için, bağlamak istediğiniz belirli özellikleri etkinleştirin, 
        // daha fazla bilgi için bkz. https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Username,Password,Role,Name,Surname,Department,Position,Intercom,PhoneNumber,InternalEmail,ExternalEmail,Sicil")] User user, [Bind(Include = "Email,RealPhoneNumber,Adres,Adres2,Sehir,Ulke,Postakodu,KanGrubu,DogumTarihi,Cinsiyet,MedeniDurum")] UserInfo userInfo)
        {
            if (ModelState.IsValid)
            {
                user.UserInfo = new List<UserInfo> { userInfo };
                db.Users.Add(user);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(user);
        }

        // GET: IK_Kullanici/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Include(u => u.UserInfo).FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: IK_Kullanici/Edit/5
        // Aşırı gönderim saldırılarından korunmak için, bağlamak istediğiniz belirli özellikleri etkinleştirin, 
        // daha fazla bilgi için bkz. https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Username,Password,Role,Name,Surname,Department,Position,Intercom,PhoneNumber,InternalEmail,ExternalEmail,Sicil")] User user, [Bind(Include = "Id,Email,RealPhoneNumber,Adres,Adres2,Sehir,Ulke,Postakodu,KanGrubu,DogumTarihi,Cinsiyet,MedeniDurum")] UserInfo userInfo)
        {
            if (ModelState.IsValid)
            {
                // UserInfo nesnesinin UserId alanını User nesnesinin Id alanı ile eşleştir
                userInfo.UserId = user.Id; // int türünden int türüne atama

                db.Entry(user).State = EntityState.Modified;
                db.Entry(userInfo).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(user);
        }

        // GET: IK_Kullanici/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Include(u => u.UserInfo).FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: IK_Kullanici/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            User user = db.Users.Include(u => u.UserInfo).FirstOrDefault(u => u.Id == id);
            db.Users.Remove(user);
            db.SaveChanges();
            return RedirectToAction("Index");
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