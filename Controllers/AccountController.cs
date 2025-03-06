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

    [HttpPost]
    public ActionResult Login(User model, bool rememberMe = false)
    {
        if (ModelState.IsValid)
        {
            if (IsValidUser(model.Username, model.Password, out string role))
            {
                FormsAuthentication.SetAuthCookie(model.Username, rememberMe);

                var authTicket = new FormsAuthenticationTicket(
                    1,
                    model.Username,
                    DateTime.Now,
                    rememberMe ? DateTime.Now.AddDays(30) : DateTime.Now.AddMinutes(30),
                    rememberMe,
                    role
                );

                string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
                var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
                {
                    HttpOnly = true,
                    Secure = FormsAuthentication.RequireSSL,
                    Expires = rememberMe ? DateTime.Now.AddDays(30) : DateTime.Now.AddMinutes(30)
                };
                HttpContext.Response.Cookies.Add(authCookie);

                var user = db.Users.SingleOrDefault(u => u.Username == model.Username);
                if (user != null)
                {
                    var notifications = db.Notifications
                        .Where(n => n.UserId == user.Id.ToString() && !n.IsRead)
                        .ToList();

                    TempData["Notifications"] = notifications;

                    // Debug: TempData içeriðini kontrol et
                    Debug.WriteLine("TempData[\"Notifications\"]: " + string.Join(", ", notifications.Select(n => n.Message)));
                }

                TempData["SuccessMessage"] = "Baþarýyla giriþ yaptýnýz, Anasayfaya yönlendiriliyor!";
                TempData["RedirectUrl"] = Url.Action("Index", "Home");
               
                return RedirectToAction("Login", "Account");
            }
            else
            {
                ModelState.AddModelError("", "Geçersiz kullanýcý adý veya þifre.");
            }
        }

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