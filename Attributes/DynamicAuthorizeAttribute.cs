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
    /// Tamamen permission yolu (path) bazlı yetki kontrolü
    /// </summary>
    public class DynamicAuthorizeAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// Permission yolu (örn: "UserManagement.Create")
        /// </summary>
        public string Permission { get; set; }

        /// <summary>
        /// CRUD Action (View, Create, Edit, Delete, Manage, Approve, Reject)
        /// </summary>
        public string Action { get; set; } = "View";

        /// <summary>
        /// Permission adı/açıklaması (backward compatibility için)
        /// Şu an için sadece dokümantasyon amaçlı
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Permission check'ini atla (bazı özel durumlar için)
        /// </summary>
        public bool SkipPermissionCheck { get; set; } = false;

        /// <summary>
        /// Yetki kontrolü
        /// </summary>
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            // Temel authentication kontrolü
            if (!base.AuthorizeCore(httpContext))
            {
                return false;
            }

            // Permission check atlanacaksa
            if (SkipPermissionCheck)
            {
                return true;
            }

            // Permission belirtilmemişse sadece authenticated kontrolü
            if (string.IsNullOrEmpty(Permission))
            {
                return httpContext.User.Identity.IsAuthenticated;
            }

            // Kullanıcı ID'sini al
            var userId = GetUserIdFromContext(httpContext);
            if (userId <= 0)
            {
                return false;
            }

            // Permission kontrolü
            return DynamicPermissionHelper.HasPermission(userId, Permission, Action);
        }

        /// <summary>
        /// Unauthorized durumunda JSON response döndür (AJAX istekler için)
        /// </summary>
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            // AJAX request kontrolü
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                filterContext.Result = new JsonResult
                {
                    Data = new
                    {
                        success = false,
                        message = "Bu işlem için yetkiniz bulunmuyor",
                        errorCode = "PERMISSION_DENIED",
                        requiredPermission = Permission,
                        requiredAction = Action
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            else
            {
                // Normal request için standart unauthorized handling
                base.HandleUnauthorizedRequest(filterContext);
            }
        }

        /// <summary>
        /// HTTP context'inden kullanıcı ID'sini al
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
        /// Belirli bir permission'ın kontrolü için helper method
        /// Controller'larda kullanılabilir
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
        /// Static helper - mevcut kullanıcı ID'sini al
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

                // Session'dan userId'yi almaya çalış
                if (httpContext.Session["UserId"] != null)
                {
                    if (int.TryParse(httpContext.Session["UserId"].ToString(), out int sessionUserId))
                        return sessionUserId;
                }

                // Database'den kullanıcıyı bul
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
        /// Permission yolu ve action'ı birleştirerek tam permission string'i oluştur
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
        /// Mevcut kullanıcının belirli bir permission'a sahip olup olmadığını kontrol et
        /// View'larda kullanım için
        /// </summary>
        public static bool CurrentUserHasPermission(string permission, string action = "View")
        {
            return CheckPermission(permission, action);
        }

        /// <summary>
        /// Mevcut kullanıcının herhangi bir permission'a sahip olup olmadığını kontrol et
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
    /// Permission tabanlı yetki kontrolü için extension methods
    /// </summary>
    public static class PermissionExtensions
    {
        /// <summary>
        /// HtmlHelper extension - permission kontrolü
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