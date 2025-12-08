using System;
using System.Linq;
using System.Web.Mvc;
using MZDNETWORK.Data;
using MZDNETWORK.Helpers;
using MZDNETWORK.Attributes;
using MZDNETWORK.Models;

namespace MZDNETWORK.Controllers
{
    /// <summary>
    /// Permission sistemi test controller'Ä±
    /// </summary>
    public class TestPermissionController : Controller
    {
        private MZDNETWORKContext db = new MZDNETWORKContext();

        /// <summary>
        /// Permission debug sayfasÄ±
        /// </summary>
        public ActionResult Debug()
        {
            var model = new PermissionDebugModel();

            try
            {
                // Mevcut kullanÄ±cÄ± bilgileri
                model.IsAuthenticated = User.Identity.IsAuthenticated;
                model.Username = User.Identity.Name;

                if (User.Identity.IsAuthenticated)
                {
                    // User ID al
                    var user = db.Users.FirstOrDefault(u => u.Username == model.Username);
                    if (user != null)
                    {
                        model.UserId = user.Id;
                        
                        // Roller
                        model.UserRoles = DynamicPermissionHelper.GetUserRoles(user.Id);
                        
                        // Database'den direkt rol kontrolÃ¼
                        var dbRoles = db.UserRoles
                            .Where(ur => ur.UserId == user.Id)
                            .Select(ur => ur.Role.Name)
                            .ToList();
                        model.DatabaseRoles = dbRoles.ToArray();

                        // Permission testleri
                        model.PermissionTests = new[]
                        {
                            new PermissionTest { Permission = "KullaniciYonetimi", Action = "View", HasPermission = DynamicPermissionHelper.CheckPermission(user.Id, "KullaniciYonetimi", "View") },
                            new PermissionTest { Permission = "KullaniciYonetimi", Action = "Create", HasPermission = DynamicPermissionHelper.CheckPermission(user.Id, "KullaniciYonetimi", "Create") },
                            new PermissionTest { Permission = "KullaniciYonetimi", Action = "Edit", HasPermission = DynamicPermissionHelper.CheckPermission(user.Id, "KullaniciYonetimi", "Edit") },
                            new PermissionTest { Permission = "SistemYonetimi.RolYonetimi", Action = "View", HasPermission = DynamicPermissionHelper.CheckPermission(user.Id, "SistemYonetimi.RolYonetimi", "View") },
                            new PermissionTest { Permission = "InsanKaynaklari", Action = "View", HasPermission = DynamicPermissionHelper.CheckPermission(user.Id, "InsanKaynaklari", "View") },
                            new PermissionTest { Permission = "IT", Action = "View", HasPermission = DynamicPermissionHelper.CheckPermission(user.Id, "IT", "View") }
                        };

                        // Permission Node'larÄ± kontrol et
                        model.PermissionNodesCount = db.PermissionNodes.Count(p => p.IsActive);
                        
                        // Role Permission'larÄ± kontrol et
                        var roleIds = db.Roles.Where(r => dbRoles.Contains(r.Name)).Select(r => r.Id).ToList();
                        model.RolePermissionsCount = db.RolePermissions.Count(rp => roleIds.Contains(rp.RoleId) && rp.IsActive);
                    }
                }
            }
            catch (Exception ex)
            {
                model.ErrorMessage = ex.Message + "\n" + ex.StackTrace;
            }

            return View(model);
        }

        /// <summary>
        /// Cache temizleme
        /// </summary>
        public ActionResult ClearCache()
        {
            try
            {
                DynamicPermissionHelper.InvalidateAllCache();
                TempData["SuccessMessage"] = "Cache baÅŸarÄ±yla temizlendi";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Cache temizlenirken hata: " + ex.Message;
            }

            return RedirectToAction("Debug");
        }

        /// <summary>
        /// Permission Seeder'Ä± Ã§alÄ±ÅŸtÄ±r
        /// </summary>
        public ActionResult RunSeeder()
        {
            try
            {
                // Static method olarak Ã§aÄŸÄ±r
                PermissionSeeder.SeedPermissions(db);
                
                // Cache'i temizle
                DynamicPermissionHelper.InvalidateAllCache();
                
                TempData["SuccessMessage"] = "Permission Seeder baÅŸarÄ±yla Ã§alÄ±ÅŸtÄ±rÄ±ldÄ± ve cache temizlendi";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Seeder Ã§alÄ±ÅŸtÄ±rÄ±lÄ±rken hata: " + ex.Message + "\n" + ex.StackTrace;
            }

            return RedirectToAction("Debug");
        }

        /// <summary>
        /// Test kullanÄ±cÄ±sÄ±na admin rolÃ¼ ata
        /// </summary>
        public ActionResult AssignAdminRole(string username = null)
        {
            try
            {
                var targetUsername = username ?? User.Identity.Name;
                
                if (string.IsNullOrEmpty(targetUsername))
                {
                    TempData["ErrorMessage"] = "KullanÄ±cÄ± adÄ± bulunamadÄ±";
                    return RedirectToAction("Debug");
                }

                var user = db.Users.FirstOrDefault(u => u.Username == targetUsername);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "KullanÄ±cÄ± bulunamadÄ±: " + targetUsername;
                    return RedirectToAction("Debug");
                }

                // Admin rolÃ¼ var mÄ± kontrol et
                var adminRole = db.Roles.FirstOrDefault(r => r.Name == "Admin");
                if (adminRole == null)
                {
                    // Admin rolÃ¼ yoksa oluÅŸtur
                    adminRole = new Role
                    {
                        Name = "Admin",
                        Description = "Sistem YÃ¶neticisi",
                        IsActive = true,
                        CreatedDate = DateTime.Now,
                        CreatedBy = user.Id
                    };
                    db.Roles.Add(adminRole);
                    db.SaveChanges();
                }

                // KullanÄ±cÄ±nÄ±n admin rolÃ¼ var mÄ± kontrol et
                var existingUserRole = db.UserRoles.FirstOrDefault(ur => ur.UserId == user.Id && ur.RoleId == adminRole.Id);
                if (existingUserRole == null)
                {
                    // Admin rolÃ¼ ata
                    db.UserRoles.Add(new UserRole
                    {
                        UserId = user.Id,
                        RoleId = adminRole.Id
                    });
                    db.SaveChanges();
                }

                // Cache temizle
                DynamicPermissionHelper.InvalidateAllCache();

                TempData["SuccessMessage"] = $"'{targetUsername}' kullanÄ±cÄ±sÄ±na Admin rolÃ¼ baÅŸarÄ±yla atandÄ±";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Admin rolÃ¼ atanÄ±rken hata: " + ex.Message;
            }

            return RedirectToAction("Debug");
        }

        /// <summary>
        /// Cache bilgilerini JSON olarak dÃ¶ndÃ¼r
        /// </summary>
        public JsonResult GetCacheInfo()
        {
            try
            {
                var cacheStats = PermissionCacheService.GetCacheStatistics();
                return Json(cacheStats, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// TÃ¼m cache'i temizle (AJAX iÃ§in)
        /// </summary>
        [HttpPost]
        public JsonResult ClearAllCache()
        {
            try
            {
                DynamicPermissionHelper.InvalidateAllCache();
                return Json(new { success = true, message = "Cache baÅŸarÄ±yla temizlendi" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Cache temizlenirken hata: " + ex.Message });
            }
        }

        /// <summary>
        /// Belirli kullanÄ±cÄ±nÄ±n cache'ini temizle
        /// </summary>
        [HttpPost]
        public JsonResult ClearUserCache(int userId)
        {
            try
            {
                DynamicPermissionHelper.InvalidateUserCache(userId);
                return Json(new { success = true, message = $"KullanÄ±cÄ± {userId} cache'i temizlendi" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Cache temizlenirken hata: " + ex.Message });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    public class PermissionDebugModel
    {
        public bool IsAuthenticated { get; set; }
        public string Username { get; set; }
        public int UserId { get; set; }
        public string[] UserRoles { get; set; } = new string[0];
        public string[] DatabaseRoles { get; set; } = new string[0];
        public PermissionTest[] PermissionTests { get; set; } = new PermissionTest[0];
        public int PermissionNodesCount { get; set; }
        public int RolePermissionsCount { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class PermissionTest
    {
        public string Permission { get; set; }
        public string Action { get; set; }
        public bool HasPermission { get; set; }
    }
}
