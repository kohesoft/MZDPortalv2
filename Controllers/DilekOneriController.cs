using MZDNETWORK.Models;
using System.Linq;
using System.Web.Mvc;
using System;


namespace MZDNETWORK.Controllers
{
   [Authorize(Roles = "IK, Yonetici, Sys, BilgiIslem, Merkez,Yerleske , IdariIsler, Lider, Dokumantasyon")]
    public class DilekOneriController : Controller
    {

        private MZDNETWORKContext db = new MZDNETWORKContext();

        [HttpGet]
        public ActionResult Create()
        {
            var model = new DilekOneri
            {
                Username = User.Identity.Name
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(DilekOneri model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kullanýcý adýný oturumdan al
                    model.Username = User.Identity.Name;

                    // GonderimTarihi alanýný kontrol et ve varsayýlan deðer ata
                    if (model.GonderimTarihi == default(DateTime))
                    {
                        model.GonderimTarihi = DateTime.Now;
                    }

                    // Modeli veritabanýna kaydet
                    db.DilekOneriler.Add(model);
                    db.SaveChanges();

                    // Baþarýlý kayýttan sonra yönlendirme
                    return RedirectToAction("Bildirimlerim");
                }
                catch (Exception ex)
                {
                    // Hata mesajýný yakalama
                    ViewBag.ErrorMessage = GetFullErrorMessage(ex);
                }
            }
            else
            {
                // Model doðrulama hatalarýný ViewBag'e ekle
                ViewBag.ErrorMessage = "Model doðrulama hatasý.";
            }

            // Model geçerli deðilse veya hata oluþtuysa formu tekrar göster
            return View(model);
        }

        private string GetFullErrorMessage(Exception ex)
        {
            var errorMessage = ex.Message;
            var innerException = ex.InnerException;
            while (innerException != null)
            {
                errorMessage += " Inner Exception: " + innerException.Message;
                innerException = innerException.InnerException;
            }
            return errorMessage;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult Bildirimlerim()
        {
            using (var context = new MZDNETWORKContext())
            {
                var replies = context.DilekOneriler.ToList();
                return View(replies);
            }
        }

        [HttpPost]
        public ActionResult SendReply(string username, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return Json(new { success = false, error = "Mesaj alaný gereklidir." });
            }

            try
            {
                using (var context = new MZDNETWORKContext())
                {
                    var dilekOneri = new DilekOneri
                    {
                        Username = username,
                        Bilidirim = message,
                        GonderimTarihi = DateTime.Now
                    };
                    context.DilekOneriler.Add(dilekOneri);
                    context.SaveChanges();
                }

                return Json(new { success = true });
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var errorMessages = ex.EntityValidationErrors
                    .SelectMany(x => x.ValidationErrors)
                    .Select(x => x.ErrorMessage);
                var fullErrorMessage = string.Join("; ", errorMessages);
                var exceptionMessage = string.Concat(ex.Message, " The validation errors are: ", fullErrorMessage);
                return Json(new { success = false, error = exceptionMessage });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }


        [Authorize(Roles = "Yonetici, Sys, IdariIsler, IK")]
        [HttpPost]
        public JsonResult UpdateMessage(int id, string message)
        {
            try
            {
                var mesaj = db.DilekOneriler.Find(id);
                if (mesaj != null)
                {
                    mesaj.Bilidirim = message;
                    db.SaveChanges();

                    var user = db.Users.FirstOrDefault(u => u.Username == mesaj.Username);
                    if (user != null)
                    {
                        var notification = new Notification
                        {
                            UserId = user.Id.ToString(),
                            Message = "Dilek ve öneriniz yanýtlandý.",
                            IsRead = false,
                            CreatedDate = DateTime.Now
                        };
                        db.Notifications.Add(notification);
                        db.SaveChanges();
                    }

                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, error = "Mesaj bulunamadý." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }



    }
}
