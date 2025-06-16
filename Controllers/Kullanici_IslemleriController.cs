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
            // UserInfo nesnesinin UserId alanını User nesnesinin Id alanı ile eşleştir
            userInfo.UserId = user.Id; // int türünden int türüne atama

            db.Entry(user).State = EntityState.Modified;
            db.Entry(userInfo).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
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
            try
            {
                User user = db.Users.Include(u => u.UserInfo).FirstOrDefault(u => u.Id == id);
                if (user != null)
                {
                    // Eğer silinen kullanıcı aktif oturumu olan kullanıcıysa, oturumu sonlandır
                    if (User.Identity.Name == user.Username)
                    {
                        System.Web.Security.FormsAuthentication.SignOut();
                    }

                    // İlişkili UserInfo kayıtlarını sil
                    if (user.UserInfo != null)
                    {
                        foreach (var userInfo in user.UserInfo.ToList())
                        {
                            db.UserInfos.Remove(userInfo);
                        }
                    }

                    // Kullanıcıyı sil
                    db.Users.Remove(user);
                    db.SaveChanges();

                    if (Request.IsAjaxRequest())
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.OK);
                    }

                    TempData["SuccessMessage"] = "Kullanıcı başarıyla silindi.";
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                if (Request.IsAjaxRequest())
                {
                    return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
                }

                TempData["ErrorMessage"] = "Kullanıcı silinirken bir hata oluştu: " + ex.Message;
                return RedirectToAction("Index");
            }
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