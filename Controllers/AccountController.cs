using System;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using MZDNETWORK.Models;

public class AccountController : Controller
{
    private readonly MZDNETWORKContext db;

    public AccountController()
    {
        db = new MZDNETWORKContext();
    }

    [HttpGet]
    public ActionResult Login()
    {
        return View();
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Login(User model, bool rememberMe = false)
    {
        try
        {
            if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
            {
                ModelState.AddModelError("", "Kullanıcı adı ve şifre gereklidir.");
                return View(model);
            }

            if (IsValidUser(model.Username, model.Password, out string role))
            {
                // Önce mevcut tüm authentication cookie'lerini temizle
                FormsAuthentication.SignOut();
                Session.Clear();
                Session.Abandon();

                // Kullanıcı bilgilerini ve rolü içeren bir ticket oluştur
                var userData = $"{model.Username}|{role}";
                var authTicket = new FormsAuthenticationTicket(
                    1,                              // Versiyon
                    model.Username,                 // Kullanıcı adı
                    DateTime.Now,                   // Oluşturulma zamanı
                    rememberMe ? DateTime.Now.AddDays(30) : DateTime.Now.AddMinutes(30), // Bitiş zamanı
                    rememberMe,                     // Kalıcı cookie mi?
                    userData,                       // Kullanıcı bilgileri ve rol
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

                if (rememberMe)
                {
                    authCookie.Expires = DateTime.Now.AddDays(30);
                }

                // Cookie'yi response'a ekle
                HttpContext.Response.Cookies.Remove(FormsAuthentication.FormsCookieName);
                HttpContext.Response.Cookies.Add(authCookie);

                // Session'a kullanıcı bilgilerini ekle
                Session["UserName"] = model.Username;
                Session["UserRole"] = role;

                var user = db.Users.SingleOrDefault(u => u.Username == model.Username);
                if (user != null)
                {
                    Session["UserId"] = user.Id;
                    var notifications = db.Notifications
                        .Where(n => n.UserId == user.Id.ToString() && !n.IsRead)
                        .ToList();

                    TempData["Notifications"] = notifications;
                }

                TempData["SuccessMessage"] = "Başarıyla giriş yaptınız, Anasayfaya yönlendiriliyor!";
                
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


    [HttpGet]
    public ActionResult Logout()
    {
        FormsAuthentication.SignOut();
        return RedirectToAction("Login", "Account");
    }

    private bool IsValidUser(string username, string password, out string role)
    {
        using (var context = new MZDNETWORKContext())
        {
            var user = context.Users.SingleOrDefault(u => u.Username == username && u.Password == password);
            if (user != null)
            {
                role = user.Role;
                Debug.WriteLine($"Role: {role}");
                return true;
            }
        }
        role = null;
        return false;
    }

    [HttpGet]
    public ActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
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