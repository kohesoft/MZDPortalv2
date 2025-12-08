using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using MZDNETWORK.Models;
using MZDNETWORK.Data;
using System.Linq;
using System.Net;
using MZDNETWORK.Attributes;

namespace MZDNETWORK.Controllers
{
    [DynamicAuthorize(Permission = "KullaniciYonetimi")]
    public class UserController : Controller
    {
        private readonly MZDNETWORKContext db;

        public UserController()
        {
            db = new MZDNETWORKContext();
        }

        // GET: User/Edit
        [DynamicAuthorize(Permission = "KullaniciYonetimi")]
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

            // KullanÄ±cÄ± ÅŸifresini deÄŸiÅŸtirmemiÅŸse ChangePassword sayfasÄ±na yÃ¶nlendir
            if (!user.IsPasswordChanged)
            {
                return RedirectToAction("ChangePassword");
            }

            var userInfo = db.UserInfos.Include("User").FirstOrDefault(u => u.User.Username == username);
            if (userInfo == null)
            {
                // KullanÄ±cÄ± iÃ§in henÃ¼z UserInfo kaydÄ± yoksa, boÅŸ bir model oluÅŸtur
                userInfo = new UserInfo { UserId = user.Id };
            }
            return View(userInfo);
        }

        // POST: User/Edit
        [DynamicAuthorize(Permission = "KullaniciYonetimi.KullaniciBilgisi", Action = "Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UserInfo userInfo)
        {
            if (ModelState.IsValid)
            {
                var existingUserInfo = db.UserInfos.Include("User").FirstOrDefault(u => u.UserId == userInfo.UserId);
                if (existingUserInfo == null)
                {
                    // Ä°lk kez oluÅŸturuluyor
                    db.UserInfos.Add(userInfo);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "KullanÄ±cÄ± bilgileri baÅŸarÄ±yla kaydedildi.";
                    return RedirectToAction("Index", "Home");
                }

                // UserInfo bilgilerini gÃ¼ncelle
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
                TempData["SuccessMessage"] = "KullanÄ±cÄ± bilgileri baÅŸarÄ±yla gÃ¼ncellendi.";
                return RedirectToAction("Index", "Home");
            }
            return View(userInfo);
        }

        // GET: User/ChangePassword
        [DynamicAuthorize(Permission = "KullaniciYonetimi")]
        [HttpGet]
        public ActionResult ChangePassword()
        {
            return View();
        }

        // POST: User/ChangePassword
        [DynamicAuthorize(Permission = "KullaniciYonetimi.KullaniciBilgisi", Action = "Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (!User.Identity.IsAuthenticated)
                {
                    ModelState.AddModelError("", "KullanÄ±cÄ± kimlik doÄŸrulamasÄ± yapÄ±lmamÄ±ÅŸ.");
                    return RedirectToAction("Login", "Account");
                }

                var username = User.Identity.Name; // KullanÄ±cÄ± adÄ±nÄ± alÄ±r
                var user = db.Users.FirstOrDefault(u => u.Username == username);
                if (user == null)
                {
                    ModelState.AddModelError("", "KullanÄ±cÄ± bulunamadÄ±.");
                    return View(model);
                }

                if (user.Password == model.OldPassword)
                {
                    user.Password = model.NewPassword;
                    user.IsPasswordChanged = true; // ÅŸifre deÄŸiÅŸtirildi olarak iÅŸaretle
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "ÅŸifre baÅŸarÄ±yla deÄŸiÅŸtirildi.";
                    TempData["RedirectUrl"] = Url.Action("Edit", "User");
                    return RedirectToAction("ChangePassword");
                }

                // EÄŸer kullanÄ±cÄ± ilk kez ÅŸifre belirliyorsa eski ÅŸifre sorgusunu atla
                if (!user.IsPasswordChanged && string.IsNullOrEmpty(model.OldPassword))
                {
                    user.Password = model.NewPassword;
                    user.IsPasswordChanged = true;
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Åifreniz baÅŸarÄ±yla belirlendi.";
                    TempData["RedirectUrl"] = Url.Action("Index", "Home");
                    return RedirectToAction("ChangePassword");
                }

                ModelState.AddModelError("", "Eski ÅŸifre yanlÄ±ÅŸ.");
            }
            return View(model);
        }
    }
}
