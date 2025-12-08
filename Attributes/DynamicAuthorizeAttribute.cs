using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Principal;
using MZDNETWORK.Helpers;
using MZDNETWORK.Data;

namespace MZDNETWORK.Attributes
{
    /// <summary>
    /// Dinamik permission-based authorization attribute
    /// Tamamen permission yolu (path) bazlÄ± yetki kontrolÃ¼
    /// </summary>
    public class DynamicAuthorizeAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// Permission yolu (Ã¶rn: "KullaniciYonetimi.Create")
        /// </summary>
        public string Permission { get; set; }

        /// <summary>
        /// CRUD Action (View, Create, Edit, Delete, Manage, Approve, Reject)
        /// </summary>
        public string Action { get; set; } = "View";

        /// <summary>
        /// Permission adÄ±/aÃ§Ä±klamasÄ± (backward compatibility iÃ§in)
        /// Åu an iÃ§in sadece dokÃ¼mantasyon amaÃ§lÄ±
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Permission check'ini atla (bazÄ± Ã¶zel durumlar iÃ§in)
        /// </summary>
        public bool SkipPermissionCheck { get; set; } = false;

        /// <summary>
        /// Yetki kontrolÃ¼
        /// </summary>
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            // Temel authentication kontrolÃ¼
            if (!base.AuthorizeCore(httpContext))
            {
                return false;
            }

            // Permission check atlanacaksa
            if (SkipPermissionCheck)
            {
                return true;
            }

            // Permission belirtilmemiÅŸse sadece authenticated kontrolÃ¼
            if (string.IsNullOrEmpty(Permission))
            {
                return httpContext.User.Identity.IsAuthenticated;
            }

            // KullanÄ±cÄ± ID'sini al
            var userId = GetUserIdFromContext(httpContext);
            if (userId <= 0)
            {
                return false;
            }

            // Permission kontrolÃ¼
            return DynamicPermissionHelper.HasPermission(userId, Permission, Action);
        }

        /// <summary>
        /// Unauthorized durumunda JSON response dÃ¶ndÃ¼r (AJAX istekler iÃ§in)
        /// </summary>
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            // AJAX request kontrolÃ¼
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                filterContext.Result = new JsonResult
                {
                    Data = new
                    {
                        success = false,
                        message = "Bu iÅŸlem iÃ§in yetkiniz bulunmuyor",
                        errorCode = "PERMISSION_DENIED",
                        requiredPermission = Permission,
                        requiredAction = Action
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            else
            {
                // Normal request iÃ§in standart unauthorized handling
                base.HandleUnauthorizedRequest(filterContext);
            }
        }

        /// <summary>
        /// HTTP context'inden kullanÄ±cÄ± ID'sini al
        /// </summary>
        private int GetUserIdFromContext(HttpContextBase httpContext)
        {
            try
            {
                if (httpContext.User?.Identity?.IsAuthenticated == true)
                {
                    var identity = httpContext.User.Identity;
                    
                    // Forms authentication ticket'dan al
                    if (identity is System.Web.Security.FormsIdentity formsIdentity)
                    {
                        var userData = formsIdentity.Ticket.UserData.Split('|');
                        if (userData.Length >= 2 && int.TryParse(userData[1], out int userId))
                        {
                            return userId;
                        }
                    }

                    // Username'den al (fallback)
                    return GetUserIdByUsername(identity.Name);
                }
            }
            catch (Exception ex)
            {
                // Log error
                System.Diagnostics.Debug.WriteLine($"GetUserIdFromContext error: {ex.Message}");
            }

            return 0;
        }

        /// <summary>
        /// Username'den user ID al (fallback method)
        /// </summary>
        private int GetUserIdByUsername(string username)
        {
            try
            {
                using (var context = new MZDNETWORKContext())
                {
                    var user = context.Users.FirstOrDefault(u => u.Username == username);
                    return user?.Id ?? 0;
                }
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Belirli bir permission'Ä±n kontrolÃ¼ iÃ§in helper method
        /// Controller'larda kullanÄ±labilir
        /// </summary>
        public static bool CheckPermission(string permission, string action = "View", int? userId = null)
        {
            try
            {
                int currentUserId = userId ?? GetCurrentUserIdStatic();
                if (currentUserId <= 0)
                    return false;

                return DynamicPermissionHelper.CheckPermission(currentUserId, permission, action);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Static helper - mevcut kullanÄ±cÄ± ID'sini al
        /// </summary>
        private static int GetCurrentUserIdStatic()
        {
            try
            {
                var httpContext = HttpContext.Current;
                if (httpContext == null || !httpContext.User.Identity.IsAuthenticated)
                    return 0;

                var username = httpContext.User.Identity.Name;
                if (string.IsNullOrEmpty(username))
                    return 0;

                // Session'dan userId'yi almaya Ã§alÄ±ÅŸ
                if (httpContext.Session["UserId"] != null)
                {
                    if (int.TryParse(httpContext.Session["UserId"].ToString(), out int sessionUserId))
                        return sessionUserId;
                }

                // Database'den kullanÄ±cÄ±yÄ± bul
                using (var context = new MZDNETWORKContext())
                {
                    var user = context.Users.FirstOrDefault(u => u.Username == username);
                    return user?.Id ?? 0;
                }
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Permission yolu ve action'Ä± birleÅŸtirerek tam permission string'i oluÅŸtur
        /// </summary>
        public static string BuildPermissionString(string permissionPath, string action)
        {
            if (string.IsNullOrEmpty(permissionPath))
                return "";

            if (string.IsNullOrEmpty(action) || action == "View")
                return permissionPath;

            return $"{permissionPath}.{action}";
        }

        /// <summary>
        /// Mevcut kullanÄ±cÄ±nÄ±n belirli bir permission'a sahip olup olmadÄ±ÄŸÄ±nÄ± kontrol et
        /// View'larda kullanÄ±m iÃ§in
        /// </summary>
        public static bool CurrentUserHasPermission(string permission, string action = "View")
        {
            return CheckPermission(permission, action);
        }

        /// <summary>
        /// Mevcut kullanÄ±cÄ±nÄ±n herhangi bir permission'a sahip olup olmadÄ±ÄŸÄ±nÄ± kontrol et
        /// </summary>
        public static bool CurrentUserHasAnyPermission(params string[] permissions)
        {
            foreach (var permission in permissions)
            {
                if (CheckPermission(permission))
                    return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Permission tabanlÄ± yetki kontrolÃ¼ iÃ§in extension methods
    /// </summary>
    public static class PermissionExtensions
    {
        /// <summary>
        /// HtmlHelper extension - permission kontrolÃ¼
        /// </summary>
        public static bool HasPermission(this System.Web.Mvc.HtmlHelper htmlHelper, string permission, string action = "View")
        {
            return DynamicAuthorizeAttribute.CurrentUserHasPermission(permission, action);
        }

        /// <summary>
        /// HtmlHelper extension - herhangi bir permission'a sahip mi
        /// </summary>
        public static bool HasAnyPermission(this System.Web.Mvc.HtmlHelper htmlHelper, params string[] permissions)
        {
            return DynamicAuthorizeAttribute.CurrentUserHasAnyPermission(permissions);
        }
    }
} 
