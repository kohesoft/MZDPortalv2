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
            // Convert empty strings to null for easier checks
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                model.Password = null;
            }

            if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
            {
                // Kullanıcı şifre girmediyse reset login denemesi olabilir.
                if (string.IsNullOrEmpty(model.Username))
                {
                    ModelState.AddModelError("", "Kullanıcı adı gereklidir.");
                    return View(model);
                }
            }

            // MVC model binder sometimes maps checkbox value "on" to false for primitive bools.
            // Therefore explicitly check the raw request value so that "on" or "true" is accepted.
            bool remember = rememberMe;
            var rawRemember = Request["rememberMe"];
            if (!string.IsNullOrEmpty(rawRemember))
            {
                remember = rawRemember.Equals("true", StringComparison.OrdinalIgnoreCase) || rawRemember.Equals("on", StringComparison.OrdinalIgnoreCase) || rawRemember == "1";
            }

            bool passwordlessLogin = false;
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

                    // Eğer passwordless login ise veya kullanıcı henüz şifre değiştirmemişse yönlendir
                    if (passwordlessLogin || !user.IsPasswordChanged)
                    {
                        return RedirectToAction("ChangePassword", "User");
                    }
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
                // Şifre yoksa onaylanmış PasswordResetRequest üzerinden giriş deneyelim
                if (string.IsNullOrEmpty(model.Password))
                {
                    if (IsPasswordResetLoginAllowed(model.Username, out userId, out rolesArray))
                    {
                        passwordlessLogin = true;
                    }
                }

                if (!passwordlessLogin)
                {
                    ModelState.AddModelError("", "Geçersiz kullanıcı adı veya şifre.");
                }
            }

            if (passwordlessLogin || (userId != 0 && rolesArray != null))
            {
                // Onaylanmış reset veya normal login: devam et

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

                    // Eğer passwordless login ise veya kullanıcı henüz şifre değiştirmemişse yönlendir
                    if (passwordlessLogin || !user.IsPasswordChanged)
                    {
                        return RedirectToAction("ChangePassword", "User");
                    }
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
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Giriş yapılırken bir hata oluştu: " + ex.Message);
        }

        // Hata durumunda model'i geri döndür
        return View(model);
    }

    /// <summary>
    /// Checks whether the user has an approved password reset request that hasn't been used yet.
    /// If so, returns true and supplies user id and roles.
    /// </summary>
    private bool IsPasswordResetLoginAllowed(string username, out int userId, out string[] roles)
    {
        userId = 0;
        roles = null;

        var user = db.Users.SingleOrDefault(u => u.Username == username);
        if (user == null)
        {
            return false;
        }

        var resetRequest = db.PasswordResetRequests.FirstOrDefault(r => r.UserId == user.Id && r.Approved && !r.Used);
        if (resetRequest == null)
        {
            return false;
        }

        // İşaretle ve kaydet
        resetRequest.Used = true;
        db.SaveChanges();

        userId = user.Id;
        roles = db.UserRoles.Where(ur => ur.UserId == user.Id).Select(ur => ur.Role.Name).ToArray();
        if (roles.Length == 0)
            roles = new[] { "User" };

        // Force password change next login
        user.IsPasswordChanged = false;
        db.SaveChanges();

        return true;
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

    ///////////////////////////////////////////////
    // Şifremi Unuttum (Forgot Password)
    ///////////////////////////////////////////////

    [HttpGet]
    [AllowAnonymous]
    public ActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public ActionResult ForgotPassword(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            ModelState.AddModelError("", "Kullanıcı adı gereklidir.");
            return View();
        }

        var user = db.Users.SingleOrDefault(u => u.Username == username);
        if (user == null)
        {
            ModelState.AddModelError("", "Kullanıcı bulunamadı.");
            return View();
        }

        var existingPending = db.PasswordResetRequests.FirstOrDefault(r => r.UserId == user.Id && !r.Used && !r.Approved);
        if (existingPending == null)
        {
            var req = new PasswordResetRequest
            {
                UserId = user.Id,
                RequestedAt = DateTime.Now,
                Approved = false,
                Used = false
            };
            db.PasswordResetRequests.Add(req);
            db.SaveChanges();
        }

        TempData["SuccessMessage"] = "Şifre sıfırlama isteğiniz alınmıştır. Yetkili personel onayladıktan sonra kullanıcı adınızla giriş yapabilirsiniz.";
        TempData["RedirectUrl"] = Url.Action("Login", "Account");
        return RedirectToAction("ForgotPassword");
    }
}