using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using MZDNETWORK.Models;
using System.Linq;
using System.Net;

namespace MZDNETWORK.Controllers
{
    public class UserController : Controller
    {
        private readonly MZDNETWORKContext db;

        public UserController()
        {
            db = new MZDNETWORKContext();
        }

        // GET: User/Edit
        [HttpGet]
        public ActionResult Edit()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var username = User.Identity.Name;
            var user = db.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
            {
                return HttpNotFound();
            }

            // Kullanýcý þifresini deðiþtirmemiþse ChangePassword sayfasýna yönlendir
            if (!user.IsPasswordChanged)
            {
                return RedirectToAction("ChangePassword");
            }

            var userInfo = db.UserInfos.Include("User").FirstOrDefault(u => u.User.Username == username);
            if (userInfo == null)
            {
                return HttpNotFound();
            }
            return View(userInfo);
        }

        // POST: User/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UserInfo userInfo)
        {
            if (ModelState.IsValid)
            {
                var existingUserInfo = db.UserInfos.Include("User").FirstOrDefault(u => u.UserId == userInfo.UserId);
                if (existingUserInfo == null)
                {
                    return HttpNotFound();
                }

                // UserInfo bilgilerini güncelle
                existingUserInfo.Email = userInfo.Email;
                existingUserInfo.RealPhoneNumber = userInfo.RealPhoneNumber;
                existingUserInfo.Adres = userInfo.Adres;
                existingUserInfo.Adres2 = userInfo.Adres2;
                existingUserInfo.Sehir = userInfo.Sehir;
                existingUserInfo.Ulke = userInfo.Ulke;
                existingUserInfo.Postakodu = userInfo.Postakodu;
                existingUserInfo.KanGrubu = userInfo.KanGrubu;
                existingUserInfo.DogumTarihi = userInfo.DogumTarihi;
                existingUserInfo.Cinsiyet = userInfo.Cinsiyet;
                existingUserInfo.MedeniDurum = userInfo.MedeniDurum;

                db.SaveChanges();
                TempData["SuccessMessage"] = "Kullanýcý bilgileri baþarýyla güncellendi.";
                return RedirectToAction("Index", "Home");
            }
            return View(userInfo);
        }

        // GET: User/ChangePassword
        [HttpGet]
        public ActionResult ChangePassword()
        {
            return View();
        }

        // POST: User/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (!User.Identity.IsAuthenticated)
                {
                    ModelState.AddModelError("", "Kullanýcý kimlik doðrulamasý yapýlmamýþ.");
                    return RedirectToAction("Login", "Account");
                }

                var username = User.Identity.Name; // Kullanýcý adýný alýr
                var user = db.Users.FirstOrDefault(u => u.Username == username);
                if (user == null)
                {
                    ModelState.AddModelError("", "Kullanýcý bulunamadý.");
                    return View(model);
                }

                if (user.Password == model.OldPassword)
                {
                    user.Password = model.NewPassword;
                    user.IsPasswordChanged = true; // Þifre deðiþtirildi olarak iþaretle
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Þifre baþarýyla deðiþtirildi.";
                    TempData["RedirectUrl"] = Url.Action("Edit", "User");
                    return RedirectToAction("ChangePassword");
                }
                ModelState.AddModelError("", "Eski þifre yanlýþ.");
            }
            return View(model);
        }
    }
}