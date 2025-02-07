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
        Debug.WriteLine($"Remember Me: {rememberMe}");
        if (ModelState.IsValid)
        {
            if (IsValidUser(model.Username, model.Password, out string role))
            {
                // Kimlik doðrulama çerezi oluþturma
                FormsAuthentication.SetAuthCookie(model.Username, rememberMe);

                // Kullanýcýnýn rolünü saklamak için bir çerez oluþturma
                var authTicket = new FormsAuthenticationTicket(
                    1,                             // Sürüm
                    model.Username,                // Kullanýcý adý
                    DateTime.Now,                  // Oluþturulma zamaný
                    rememberMe ? DateTime.Now.AddDays(30) : DateTime.Now.AddMinutes(30),   // Bitiþ zamaný
                    rememberMe,                    // Kalýcý mý?
                    role                           // Kullanýcý rolü
                );

                string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
                var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
                {
                    HttpOnly = true,
                    Secure = FormsAuthentication.RequireSSL,
                    Expires = rememberMe ? DateTime.Now.AddDays(30) : DateTime.Now.AddMinutes(30)
                };
                HttpContext.Response.Cookies.Add(authCookie);

                TempData["SuccessMessage"] = "Baþarýyla giriþ yaptýnýz, Anasayfaya yönlendiriliyor!";
                TempData["RedirectUrl"] = Url.Action("Index", "Home");
                return RedirectToAction("Login", "Account");
            }
            else
            {
                ModelState.AddModelError("", "Geçersiz kullanýcý adý veya þifre.");
            }
        }
        else
        {
            Debug.WriteLine("ModelState is not valid");
            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    Debug.WriteLine($"Property: {state.Key}, Error: {error.ErrorMessage}");
                }
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