using MZDNETWORK.Models;
using System.Linq;
using System.Web.Mvc;
using System;
using System.Data.Entity; // Added for DbContext

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
                Username = User.Identity.Name // Pre-fill username from current user
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Recommended for POST actions to prevent CSRF attacks
        public ActionResult Create(DilekOneri model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Ensure the username is set from the authenticated user
                    model.Username = User.Identity.Name;

                    // Ensure GonderimTarihi is set, if not already provided
                    if (model.GonderimTarihi == default(DateTime))
                    {
                        model.GonderimTarihi = DateTime.Now;
                    }

                    // Add and save the new suggestion/wish
                    db.DilekOneriler.Add(model);
                    db.SaveChanges();

                    // Redirect after successful creation
                    return RedirectToAction("Bildirimlerim");
                }
                catch (Exception ex)
                {
                    // Log the exception for debugging purposes (e.g., using a logging framework)
                    System.Diagnostics.Debug.WriteLine($"Error creating DilekOneri: {GetFullErrorMessage(ex)}");
                    ViewBag.ErrorMessage = "Bir hata oluþtu: " + ex.Message; // Provide a user-friendly error message
                }
            }
            else
            {
                // Collect and display model validation errors
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                ViewBag.ErrorMessage = "Lütfen formdaki hatalarý düzeltin: " + string.Join("; ", errors);
            }

            // If model is not valid or an error occurred, return the view with the model
            return View(model);
        }

        // Helper to get detailed error messages
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
            // Use the class-level db context, no need for 'using (var context = new MZDNETWORKContext())' here
            // Unless you explicitly want a fresh context for this method, but usually one per request is enough.
            var replies = db.DilekOneriler.ToList();
            return View(replies);
        }

        // This action seems to be for sending a *new* reply, not updating an existing one.
        // It's inconsistent with the "Yanýtla" button's purpose of updating an existing entry.
        // I've kept it as is but note that UpdateMessage is likely the correct one for the 'Yanýtla' button.
        [HttpPost]
        public JsonResult SendReply(string username, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return Json(new { success = false, error = "Mesaj alaný gereklidir." });
            }

            try
            {
                // In a real application, you'd likely associate this reply with a specific DilekOneri
                // or send it as a new distinct message if it's truly a "new" reply from an admin perspective.
                // Given your current setup, it looks like this is intended for a new message.
                var dilekOneri = new DilekOneri
                {
                    Username = username,
                    Mesaj = "Yanýt: " + message, // Added "Yanýt:" to differentiate if this is a reply
                    Bilidirim = null, // This would be the admin's reply, but here it's empty
                    GonderimTarihi = DateTime.Now,
                    IsAnonymous = false // Assuming replies are not anonymous
                };
                db.DilekOneriler.Add(dilekOneri);
                db.SaveChanges();

                // If this is a new reply meant *for* a specific user, you'd likely notify them.
                // The current setup sends a notification to the user who made the *original* suggestion.
                // Consider if 'username' passed here truly matches an existing user.
                var user = db.Users.FirstOrDefault(u => u.Username == username);
                if (user != null)
                {
                    var notification = new Notification
                    {
                        UserId = user.Id.ToString(),
                        Message = $"Yanýtýnýz gönderildi: {message.Substring(0, Math.Min(message.Length, 50))}...", // Truncate message
                        IsRead = false,
                        CreatedDate = DateTime.Now
                    };
                    db.Notifications.Add(notification);
                    db.SaveChanges();
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
                System.Diagnostics.Debug.WriteLine($"DbEntityValidationException in SendReply: {exceptionMessage}");
                return Json(new { success = false, error = exceptionMessage });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in SendReply: {ex.Message}");
                return Json(new { success = false, error = ex.Message });
            }
        }


        [Authorize(Roles = "Yonetici, Sys, IdariIsler, IK")]
        [HttpPost]
        public JsonResult UpdateMessage(int id, string message)
        {
            try
            {
                // Find the existing suggestion by ID
                var dilekOneri = db.DilekOneriler.Find(id);

                if (dilekOneri != null)
                {
                    // Update the Bilidirim (Reply) field
                    dilekOneri.Bilidirim = message;
                    db.Entry(dilekOneri).State = EntityState.Modified; // Explicitly mark as modified
                    db.SaveChanges();

                    // Find the user who made the original suggestion to send a notification
                    var user = db.Users.FirstOrDefault(u => u.Username == dilekOneri.Username);
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
                // Log the full exception details
                System.Diagnostics.Debug.WriteLine($"Exception in UpdateMessage: {GetFullErrorMessage(ex)}");
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
}