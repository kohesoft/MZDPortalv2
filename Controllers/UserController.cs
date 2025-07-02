using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using MZDNETWORK.Models;
using MZDNETWORK.Data;
using System.Linq;
using System.Net;
using MZDNETWORK.Attributes;

namespace MZDNETWORK.Controllers
{
    [DynamicAuthorize(Permission = "UserManagement")]
    public class UserController : Controller
    {
        private readonly MZDNETWORKContext db;

        public UserController()
        {
            db = new MZDNETWORKContext();
        }

        // GET: User/Edit
        [DynamicAuthorize(Permission = "UserManagement")]
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

            // Kullanıcı şifresini değiştirmemişse ChangePassword sayfasına yönlendir
            if (!user.IsPasswordChanged)
            {
                return RedirectToAction("ChangePassword");
            }

            var userInfo = db.UserInfos.Include("User").FirstOrDefault(u => u.User.Username == username);
            if (userInfo == null)
            {
                // Kullanıcı için henüz UserInfo kaydı yoksa, boş bir model oluştur
                userInfo = new UserInfo { UserId = user.Id };
            }
            return View(userInfo);
        }

        // POST: User/Edit
        [DynamicAuthorize(Permission = "UserManagement.UserInfo", Action = "Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UserInfo userInfo)
        {
            if (ModelState.IsValid)
            {
                var existingUserInfo = db.UserInfos.Include("User").FirstOrDefault(u => u.UserId == userInfo.UserId);
                if (existingUserInfo == null)
                {
                    // İlk kez oluşturuluyor
                    db.UserInfos.Add(userInfo);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Kullanıcı bilgileri başarıyla kaydedildi.";
                    return RedirectToAction("Index", "Home");
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
                TempData["SuccessMessage"] = "Kullanıcı bilgileri başarıyla güncellendi.";
                return RedirectToAction("Index", "Home");
            }
            return View(userInfo);
        }

        // GET: User/ChangePassword
        [DynamicAuthorize(Permission = "UserManagement")]
        [HttpGet]
        public ActionResult ChangePassword()
        {
            return View();
        }

        // POST: User/ChangePassword
        [DynamicAuthorize(Permission = "UserManagement.UserInfo", Action = "Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (!User.Identity.IsAuthenticated)
                {
                    ModelState.AddModelError("", "Kullanıcı kimlik doğrulaması yapılmamış.");
                    return RedirectToAction("Login", "Account");
                }

                var username = User.Identity.Name; // Kullanıcı adını alır
                var user = db.Users.FirstOrDefault(u => u.Username == username);
                if (user == null)
                {
                    ModelState.AddModelError("", "Kullanıcı bulunamadı.");
                    return View(model);
                }

                if (user.Password == model.OldPassword)
                {
                    user.Password = model.NewPassword;
                    user.IsPasswordChanged = true; // şifre değiştirildi olarak işaretle
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "şifre başarıyla değiştirildi.";
                    TempData["RedirectUrl"] = Url.Action("Edit", "User");
                    return RedirectToAction("ChangePassword");
                }

                // Eğer kullanıcı ilk kez şifre belirliyorsa eski şifre sorgusunu atla
                if (!user.IsPasswordChanged && string.IsNullOrEmpty(model.OldPassword))
                {
                    user.Password = model.NewPassword;
                    user.IsPasswordChanged = true;
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Şifreniz başarıyla belirlendi.";
                    TempData["RedirectUrl"] = Url.Action("Index", "Home");
                    return RedirectToAction("ChangePassword");
                }

                ModelState.AddModelError("", "Eski şifre yanlış.");
            }
            return View(model);
        }
    }
}