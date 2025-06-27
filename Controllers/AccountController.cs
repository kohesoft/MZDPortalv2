using System;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using MZDNETWORK.Models;
using MZDNETWORK.Data;
using System.ComponentModel.DataAnnotations;

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly MZDNETWORKContext db;

    public AccountController()
    {
        db = new MZDNETWORKContext();
    }

    [Display(Name = "Giriş Sayfası")]
    [HttpGet]
    public ActionResult Login()
    {
        return View();
    }

    
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Display(Name = "Giriş İşlemi")]
    public ActionResult Login(User model, bool rememberMe = false)
    {
        try
        {
            if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
            {
                ModelState.AddModelError("", "Kullanıcı adı ve şifre gereklidir.");
                return View(model);
            }

            // MVC model binder sometimes maps checkbox value "on" to false for primitive bools.
            // Therefore explicitly check the raw request value so that "on" or "true" is accepted.
            bool remember = rememberMe;
            var rawRemember = Request["rememberMe"];
            if (!string.IsNullOrEmpty(rawRemember))
            {
                remember = rawRemember.Equals("true", StringComparison.OrdinalIgnoreCase) || rawRemember.Equals("on", StringComparison.OrdinalIgnoreCase) || rawRemember == "1";
            }

            if (IsValidUser(model.Username, model.Password, out int userId, out string[] rolesArray))
            {
                // Önce mevcut tüm authentication cookie'lerini temizle
                FormsAuthentication.SignOut();
                Session.Clear();
                Session.Abandon();

                // Kullanıcı bilgilerini ve userId içeren bir ticket oluştur (multiple roles için)
                var rolesCsv = string.Join(",", rolesArray ?? new string[0]);
                var userData = $"{model.Username}|{userId}|{rolesCsv}"; // Multiple roles tutulur
                var authTicket = new FormsAuthenticationTicket(
                    1,                              // Versiyon
                    model.Username,                 // Kullanıcı adı
                    DateTime.Now,                   // Oluşturulma zamanı
                    remember ? DateTime.Now.AddDays(30) : DateTime.Now.AddMinutes(30), // Bitiş zamanı
                    remember,                     // Kalıcı cookie mi?
                    userData,                       // Kullanıcı bilgileri ve userId
                    FormsAuthentication.FormsCookiePath // Cookie yolu
                );

                // Ticket'ı şifrele
                string encryptedTicket = FormsAuthentication.Encrypt(authTicket);

                // Yeni bir authentication cookie oluştur
                var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
                {
                    HttpOnly = true,
                    Secure = FormsAuthentication.RequireSSL,
                    Path = FormsAuthentication.FormsCookiePath,
                    Domain = FormsAuthentication.CookieDomain
                };

                if (remember)
                {
                    authCookie.Expires = DateTime.Now.AddDays(30);
                }

                // Cookie'yi response'a ekle
                HttpContext.Response.Cookies.Remove(FormsAuthentication.FormsCookieName);
                HttpContext.Response.Cookies.Add(authCookie);

                // Session'a kullanıcı bilgilerini ekle
                Session["UserName"] = model.Username;
                Session["UserRoles"] = rolesArray;

                var user = db.Users.SingleOrDefault(u => u.Username == model.Username);
                if (user != null)
                {
                    Session["UserId"] = user.Id;
                    if (rolesArray == null || rolesArray.Length == 0)
                        rolesArray = MZDNETWORK.Helpers.RoleHelper.GetUserRoles(user.Id);
                    Session["UserRoles"] = rolesArray;
                    var notifications = db.Notifications
                        .Where(n => n.UserId == user.Id.ToString() && !n.IsRead)
                        .ToList();

                    TempData["Notifications"] = notifications;
                }

                if (remember)
                {
                    TempData["SuccessMessage"] = "Başarıyla giriş yaptınız! Oturumunuz 30 gün boyunca açık kalacaktır.";
                }
                else
                {
                    TempData["SuccessMessage"] = "Başarıyla giriş yaptınız!";
                }
                
                if (Request.QueryString["ReturnUrl"] != null)
                {
                    return Redirect(Request.QueryString["ReturnUrl"]);
                }
                
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("", "Geçersiz kullanıcı adı veya şifre.");
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Giriş yapılırken bir hata oluştu: " + ex.Message);
        }

        // Hata durumunda model'i geri döndür
        return View(model);
    }


    [Display(Name = "Çıkış")]
    [HttpGet]
    public ActionResult Logout()
    {
        FormsAuthentication.SignOut();
        return RedirectToAction("Login", "Account");
    }

    private bool IsValidUser(string username, string password, out int userId, out string[] roles)
    {
        using (var context = new MZDNETWORKContext())
        {
            var user = context.Users.SingleOrDefault(u => u.Username == username && u.Password == password);
            if (user != null)
            {
                userId = user.Id;
                roles = context.UserRoles
                    .Where(ur => ur.UserId == user.Id)
                    .Select(ur => ur.Role.Name)
                    .ToArray();
                if (roles.Length == 0)
                    roles = new[] { "User" };
                Debug.WriteLine($"UserId: {userId}, Roles: {string.Join(",", roles)}");
                return true;
            }
        }
        userId = 0;
        roles = null;
        return false;
    }

    [Display(Name = "Kullanıcı Oluştur Sayfası")]
    [HttpGet]
    public ActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Display(Name = "Kullanıcı Oluştur")]
    public ActionResult Create(User model)
    {
        if (ModelState.IsValid)
        {
            db.Users.Add(model);
            db.SaveChanges();
            return RedirectToAction("Index", "Home");
        }

        return View(model);
    }
}