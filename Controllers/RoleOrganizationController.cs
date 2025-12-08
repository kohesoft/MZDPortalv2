using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MZDNETWORK.Attributes;
using MZDNETWORK.Helpers;
using MZDNETWORK.Models;
using MZDNETWORK.Data;


namespace MZDNETWORK.Controllers
{
    [DynamicAuthorize(Permission = "RolYonetimi.RolIslemleri")]
    public class RoleOrganizationController : Controller
    {
        private readonly MZDNETWORKContext db;

        public RoleOrganizationController()
        {
            db = new MZDNETWORKContext();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Ana rol organizasyon sayfasÄ±
        /// </summary>
        public ActionResult Index()
        {
            var model = new RoleOrganizationViewModel
            {
                Roles = RoleHelper.GetAllRoles(),
                // Entity Framework Include ile UserRoles ve Role iliÅŸkilerini yÃ¼kle
                Users = db.Users
                    .Include("UserRoles.Role")
                    .OrderBy(u => u.Name)
                    .ToList(),
                AccessMatrix = GetAccessMatrix(),
                ControllerPages = GetControllerPages(),
                RoleStatistics = RoleHelper.GetRoleStatistics()
            };

            return View(model);
        }

        /// <summary>
        /// Dinamik rol oluÅŸturma
        /// </summary>
        [HttpPost]
        [DynamicAuthorize(Permission = "RolYonetimi.RolIslemleri", Action = "Create")]
        public ActionResult CreateDynamicRole(string roleName, string description)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                TempData["ErrorMessage"] = "Rol adÄ± boÅŸ olamaz";
                return RedirectToAction("Index");
            }

            bool success = RoleHelper.CreateRole(roleName.Trim(), description?.Trim());
            
            if (success)
            {
                TempData["SuccessMessage"] = $"'{roleName}' rolÃ¼ baÅŸarÄ±yla oluÅŸturuldu";
            }
            else
            {
                TempData["ErrorMessage"] = $"'{roleName}' rolÃ¼ zaten mevcut";
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// EriÅŸim matrisini oluÅŸturur (permission-based)
        /// PermissionSeeder ile uyumlu gÃ¼ncel permission'lar
        /// </summary>
        private List<AccessMatrixItem> GetAccessMatrix()
        {
            return new List<AccessMatrixItem>
            {
                // KullanÄ±cÄ± YÃ¶netimi
                new AccessMatrixItem
                {
                    ControllerName = "Kullanici_Islemleri",
                    ActionName = "All Actions",
                    Description = "KullanÄ±cÄ± Ä°ÅŸlemleri",
                    Permission = "KullaniciYonetimi",
                    RequiredPermissions = "KullaniciYonetimi.View, KullaniciYonetimi.Create, KullaniciYonetimi.Edit, KullaniciYonetimi.Delete, KullaniciYonetimi.Manage, KullaniciYonetimi.Roles, KullaniciYonetimi.Passwords"
                },

                // Rol YÃ¶netimi
                new AccessMatrixItem
                {
                    ControllerName = "RolePermission",
                    ActionName = "Matrix Management",
                    Description = "Rol-Yetki Matrisi",
                    Permission = "RolYonetimi.Permissions",
                    RequiredPermissions = "RolYonetimi.Permissions.Edit, RolYonetimi.Permissions.Manage, RolYonetimi.Permissions.Delete"
                },
                new AccessMatrixItem
                {
                    ControllerName = "RolYonetimi",
                    ActionName = "Role Operations",
                    Description = "Rol YÃ¶netimi",
                    Permission = "SistemYonetimi.RolYonetimi",
                    RequiredPermissions = "SistemYonetimi.RolYonetimi.View, SistemYonetimi.RolYonetimi.Create, SistemYonetimi.RolYonetimi.Edit, SistemYonetimi.RolYonetimi.Delete"
                },
                new AccessMatrixItem
                {
                    ControllerName = "RoleOrganization",
                    ActionName = "Organization",
                    Description = "Rol Organizasyonu",
                    Permission = "SistemYonetimi.RolYonetimi",
                    RequiredPermissions = "SistemYonetimi.RolYonetimi.View, SistemYonetimi.RolYonetimi.Create, SistemYonetimi.RolYonetimi.Edit"
                },

                // Ä°nsan KaynaklarÄ±
                new AccessMatrixItem
                {
                    ControllerName = "InsanKaynaklari",
                    ActionName = "HR Operations",
                    Description = "Ä°nsan KaynaklarÄ±",
                    Permission = "InsanKaynaklari",
                    RequiredPermissions = "InsanKaynaklari.LeaveRequests.View, InsanKaynaklari.Oneris, InsanKaynaklari.Duyurular, InsanKaynaklari.Performance, InsanKaynaklari.Surveys"
                },

                // Sistem YÃ¶netimi
                new AccessMatrixItem
                {
                    ControllerName = "PermissionTree",
                    ActionName = "Permission Management",
                    Description = "Yetki AÄŸacÄ± YÃ¶netimi",
                    Permission = "SistemYonetimi.Permissions",
                    RequiredPermissions = "SistemYonetimi.Permissions.Create, SistemYonetimi.Permissions.Edit, SistemYonetimi.Permissions.Delete, SistemYonetimi.Permissions.Manage"
                },

                // Bilgi Ä°ÅŸlem
                new AccessMatrixItem
                {
                    ControllerName = "BilgiIslem",
                    ActionName = "IT Operations",
                    Description = "Bilgi Ä°ÅŸlem Ä°ÅŸlemleri",
                    Permission = "IT",
                    RequiredPermissions = "IT.View, IT.YemekPhoto.Merkez.Create, IT.YemekPhoto.Merkez.Delete, IT.YemekPhoto.Yerleske.Create, IT.YemekPhoto.Yerleske.Delete, IT.BreakPhoto.Create, IT.BreakPhoto.Delete"
                },
                new AccessMatrixItem
                {
                    ControllerName = "BilgiIslem",
                    ActionName = "System Management",
                    Description = "Sistem AyarlarÄ±",
                    Permission = "BilgiIslem",
                    RequiredPermissions = "BilgiIslem.Settings, BilgiIslem.Notifications, BilgiIslem.MenuManagement, BilgiIslem.TVManagement, BilgiIslem.BackupRestore, BilgiIslem.Monitoring"
                },

                // DokÃ¼mantasyon
                new AccessMatrixItem
                {
                    ControllerName = "Dokumantasyon",
                    ActionName = "Document Management",
                    Description = "DokÃ¼mantasyon YÃ¶netimi",
                    Permission = "Documentation",
                    RequiredPermissions = "Documentation.View, Documentation.Create, Documentation.Edit, Documentation.Delete, Documentation.Manage, Documentation.StockCards, Documentation.Requests"
                },

                // Duyurular
                new AccessMatrixItem
                {
                    ControllerName = "Gonderi",
                    ActionName = "Announcements",
                    Description = "Duyuru YÃ¶netimi",
                    Permission = "Announcements",
                    RequiredPermissions = "Announcements.View, Announcements.Create, Announcements.Edit, Announcements.Delete, Announcements.Manage"
                },

                // Survey (Anket Sistemi)
                new AccessMatrixItem
                {
                    ControllerName = "Answer",
                    ActionName = "Survey System",
                    Description = "Anket Sistemi",
                    Permission = "Survey",
                    RequiredPermissions = "Survey.View, Survey.Create, Survey.Manage"
                },

                // Chat Sistemi
                new AccessMatrixItem
                {
                    ControllerName = "Chat",
                    ActionName = "Chat System",
                    Description = "Chat Sistemi",
                    Permission = "Chat",
                    RequiredPermissions = "Chat.View, Chat.Create, Chat.Moderate"
                },

                // Beyaz Tahta
                new AccessMatrixItem
                {
                    ControllerName = "BeyazTahta",
                    ActionName = "Whiteboard",
                    Description = "Beyaz Tahta Sistemi",
                    Permission = "WhiteBoard",
                    RequiredPermissions = "WhiteBoard.View, WhiteBoard.Header.Edit, WhiteBoard.Entry.Create, WhiteBoard.Entry.Edit, WhiteBoard.Entry.Delete"
                },

                // Geri Bildirim
                new AccessMatrixItem
                {
                    ControllerName = "Feedback",
                    ActionName = "Feedback System",
                    Description = "Geri Bildirim Sistemi",
                    Permission = "Feedback",
                    RequiredPermissions = "Feedback.View, Feedback.Create, Feedback.Manage"
                },

                // Ã–neri/Åikayet
                new AccessMatrixItem
                {
                    ControllerName = "DilekOneri",
                    ActionName = "Suggestions",
                    Description = "Ã–neri/Åikayet Sistemi",
                    Permission = "Suggestion",
                    RequiredPermissions = "Suggestion.View, Suggestion.Create, Suggestion.Reply.Create, Suggestion.Reply.Manage, Suggestion.Manage"
                },

                // Bildirim Sistemi
                new AccessMatrixItem
                {
                    ControllerName = "Notification",
                    ActionName = "Notifications",
                    Description = "Bildirim Sistemi",
                    Permission = "Notification",
                    RequiredPermissions = "Notification.View, Notification.Send.Send, Notification.Read.Read, Notification.Manage"
                },
                new AccessMatrixItem
                {
                    ControllerName = "Bildirimlerim",
                    ActionName = "Personal Notifications",
                    Description = "KiÅŸisel Bildirimler",
                    Permission = "Notification",
                    RequiredPermissions = "Notification.View"
                },

                // KiÅŸi Rehberi
                new AccessMatrixItem
                {
                    ControllerName = "Contact",
                    ActionName = "Contact Directory",
                    Description = "KiÅŸi Rehberi",
                    Permission = "Contact",
                    RequiredPermissions = "Contact.View, Contact.Export.Export"
                },

                // Online KullanÄ±cÄ±lar
                new AccessMatrixItem
                {
                    ControllerName = "OnlineUsers",
                    ActionName = "Online Users",
                    Description = "Online KullanÄ±cÄ±lar",
                    Permission = "OnlineUsers",
                    RequiredPermissions = "OnlineUsers.View"
                },

                // Performans
                new AccessMatrixItem
                {
                    ControllerName = "Performance",
                    ActionName = "Performance",
                    Description = "Performans Sistemi",
                    Permission = "Performance",
                    RequiredPermissions = "Performance.View, Performance.Manage"
                },

                // GÃ¼nlÃ¼k Ruh Hali
                new AccessMatrixItem
                {
                    ControllerName = "DailyMood",
                    ActionName = "Daily Mood",
                    Description = "GÃ¼nlÃ¼k Ruh Hali",
                    Permission = "DailyMood",
                    RequiredPermissions = "DailyMood.View, DailyMood.Create"
                },

                // Operasyonel Ä°ÅŸlemler
                new AccessMatrixItem
                {
                    ControllerName = "Operasyon",
                    ActionName = "Operations",
                    Description = "Operasyonel Ä°ÅŸlemler",
                    Permission = "Operasyon",
                    RequiredPermissions = "Operasyon.ToplantiOdasi.View, Operasyon.Sohbet.View, Operasyon.Notifications, Operasyon.Gorevs, Operasyon.Calendar"
                },

                // Test ve Debug
                new AccessMatrixItem
                {
                    ControllerName = "TestPermission",
                    ActionName = "Testing",
                    Description = "Permission Test",
                    Permission = "SistemYonetimi.Permissions",
                    RequiredPermissions = "SistemYonetimi.Permissions.Manage"
                },

                // Hesap Ä°ÅŸlemleri (Permission'sÄ±z)
                new AccessMatrixItem
                {
                    ControllerName = "Account",
                    ActionName = "Authentication",
                    Description = "Hesap Ä°ÅŸlemleri",
                    Permission = "Public",
                    RequiredPermissions = "Herkes eriÅŸebilir"
                },

                // Ana Sayfa (Permission'sÄ±z)
                new AccessMatrixItem
                {
                    ControllerName = "Home",
                    ActionName = "Dashboard",
                    Description = "Ana Sayfa",
                    Permission = "Public",
                    RequiredPermissions = "GiriÅŸ yapmÄ±ÅŸ kullanÄ±cÄ±lar"
                }
            };
        }

        /// <summary>
        /// Controller sayfalarÄ±nÄ± kategorilere gÃ¶re listeler
        /// PermissionSeeder ile uyumlu gÃ¼ncel controller listesi
        /// </summary>
        
        private List<ControllerPageInfo> GetControllerPages()
        {
            return new List<ControllerPageInfo>
            {
                // KullanÄ±cÄ± YÃ¶netimi
                new ControllerPageInfo { ControllerName = "Kullanici_Islemleri", Description = "KullanÄ±cÄ± Ä°ÅŸlemleri", Category = "KullanÄ±cÄ± YÃ¶netimi" },
                new ControllerPageInfo { ControllerName = "Account", Description = "Hesap Ä°ÅŸlemleri", Category = "KullanÄ±cÄ± YÃ¶netimi" },

                // Rol YÃ¶netimi
                new ControllerPageInfo { ControllerName = "RolePermission", Description = "Rol-Yetki Matrisi", Category = "Rol YÃ¶netimi" },
                new ControllerPageInfo { ControllerName = "RolYonetimi", Description = "Rol YÃ¶netimi", Category = "Rol YÃ¶netimi" },
                new ControllerPageInfo { ControllerName = "RoleOrganization", Description = "Rol Organizasyonu", Category = "Rol YÃ¶netimi" },

                // Ä°nsan KaynaklarÄ±
                new ControllerPageInfo { ControllerName = "InsanKaynaklari", Description = "Ä°nsan KaynaklarÄ±", Category = "Ä°nsan KaynaklarÄ±" },
                new ControllerPageInfo { ControllerName = "DilekOneri", Description = "Ã–neri/Åikayet", Category = "Ä°nsan KaynaklarÄ±" },
                new ControllerPageInfo { ControllerName = "Performance", Description = "Performans", Category = "Ä°nsan KaynaklarÄ±" },
                new ControllerPageInfo { ControllerName = "DailyMood", Description = "GÃ¼nlÃ¼k Ruh Hali", Category = "Ä°nsan KaynaklarÄ±" },

                // Bilgi Ä°ÅŸlem
                new ControllerPageInfo { ControllerName = "BilgiIslem", Description = "Bilgi Ä°ÅŸlem Ä°ÅŸlemleri", Category = "Bilgi Ä°ÅŸlem" },
                new ControllerPageInfo { ControllerName = "BeyazTahta", Description = "Beyaz Tahta", Category = "Bilgi Ä°ÅŸlem" },

                // DokÃ¼mantasyon
                new ControllerPageInfo { ControllerName = "Dokumantasyon", Description = "DokÃ¼mantasyon", Category = "DokÃ¼mantasyon" },

                // Sistem YÃ¶netimi
                new ControllerPageInfo { ControllerName = "PermissionTree", Description = "Yetki AÄŸacÄ±", Category = "Sistem YÃ¶netimi" },
                new ControllerPageInfo { ControllerName = "TestPermission", Description = "Permission Test", Category = "Sistem YÃ¶netimi" },

                // Operasyonel Ä°ÅŸlemler
                new ControllerPageInfo { ControllerName = "Home", Description = "Ana Sayfa", Category = "Operasyonel" },
                new ControllerPageInfo { ControllerName = "Notification", Description = "Bildirim YÃ¶netimi", Category = "Operasyonel" },
                new ControllerPageInfo { ControllerName = "Bildirimlerim", Description = "KiÅŸisel Bildirimler", Category = "Operasyonel" },
                new ControllerPageInfo { ControllerName = "OnlineUsers", Description = "Online KullanÄ±cÄ±lar", Category = "Operasyonel" },
                new ControllerPageInfo { ControllerName = "Contact", Description = "KiÅŸi Rehberi", Category = "Operasyonel" },
                new ControllerPageInfo { ControllerName = "Chat", Description = "Chat Sistemi", Category = "Operasyonel" },

                // Anket ve Geri Bildirim
                new ControllerPageInfo { ControllerName = "Answer", Description = "Anket Sistemi", Category = "Anket/DeÄŸerlendirme" },
                new ControllerPageInfo { ControllerName = "Feedback", Description = "Geri Bildirim", Category = "Anket/DeÄŸerlendirme" },

                // Duyurular
                new ControllerPageInfo { ControllerName = "Gonderi", Description = "Duyuru YÃ¶netimi", Category = "Duyurular" }
            };
        }
        

        /// <summary>
        /// Rol istatistikleri API
        /// </summary>
        [DynamicAuthorize(Permission = "RolYonetimi.RolIslemleri")]
        public JsonResult GetRoleStatistics()
        {
            var stats = RoleHelper.GetRoleStatistics();
            return Json(stats, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// En Ã§ok kullanÄ±lan rolleri getir
        /// </summary>
        [DynamicAuthorize(Permission = "RolYonetimi.RolIslemleri")]
        public JsonResult GetMostUsedRoles(int count = 5)
        {
            var roles = RoleHelper.GetMostUsedRoles(count)
                .Select(r => new { 
                    Name = r.Name, 
                    Description = r.Description,
                    UserCount = RoleHelper.GetRoleUserCount(r.Name)
                })
                .ToList();

            return Json(roles, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// KullanÄ±cÄ± rollerini toplu gÃ¼ncelleme
        /// </summary>
        [HttpPost]
        [DynamicAuthorize(Permission = "RolYonetimi.RolIslemleri")]
        [ValidateAntiForgeryToken]
        public ActionResult BulkUpdateUserRoles(int userId, List<string> selectedRoles)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"BulkUpdateUserRoles called: UserId={userId}, Roles={string.Join(",", selectedRoles ?? new List<string>())}");

                // KullanÄ±cÄ±nÄ±n var olup olmadÄ±ÄŸÄ±nÄ± kontrol et
                var user = db.Users.Find(userId);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "KullanÄ±cÄ± bulunamadÄ±";
                    System.Diagnostics.Debug.WriteLine($"User not found: {userId}");
                    return RedirectToAction("Index");
                }

                // Ã–NCESÄ°NDE: KullanÄ±cÄ±nÄ±n cache'ini temizle
                System.Diagnostics.Debug.WriteLine($"Clearing cache for user {userId} before role update");
                DynamicPermissionHelper.InvalidateUserCache(userId);

                // Ã–nce mevcut tÃ¼m rolleri kaldÄ±r
                System.Diagnostics.Debug.WriteLine("Removing all existing roles...");
                bool removalSuccess = true;
                try
                {
                    RoleHelper.RemoveAllRolesFromUser(userId);
                    System.Diagnostics.Debug.WriteLine("All roles removed successfully");
                }
                catch (Exception removeEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Error removing roles: {removeEx.Message}");
                    removalSuccess = false;
                }

                // Sonra yeni rolleri ata
                int successCount = 0;
                int failureCount = 0;
                
                if (selectedRoles != null && selectedRoles.Any())
                {
                    foreach (var roleName in selectedRoles)
                    {
                        try
                        {
                            System.Diagnostics.Debug.WriteLine($"Assigning role: {roleName} to user: {userId}");
                            bool assigned = RoleHelper.AssignRoleToUser(userId, roleName);
                            if (assigned)
                            {
                                successCount++;
                                System.Diagnostics.Debug.WriteLine($"Successfully assigned role: {roleName}");
                            }
                            else
                            {
                                failureCount++;
                                System.Diagnostics.Debug.WriteLine($"Failed to assign role: {roleName} (might already exist)");
                            }
                        }
                        catch (Exception assignEx)
                        {
                            failureCount++;
                            System.Diagnostics.Debug.WriteLine($"Exception assigning role {roleName}: {assignEx.Message}");
                        }
                    }
                }

                // SONRASINDA: KullanÄ±cÄ±nÄ±n cache'ini tekrar temizle
                System.Diagnostics.Debug.WriteLine($"Clearing cache for user {userId} after role update");
                DynamicPermissionHelper.InvalidateUserCache(userId);
                
                // GÃ¼venlik iÃ§in tÃ¼m cache'i de temizle
                DynamicPermissionHelper.ClearPermissionCache();
                System.Diagnostics.Debug.WriteLine("All permission cache cleared for security");

                // SonuÃ§ mesajlarÄ±
                if (successCount > 0)
                {
                    TempData["SuccessMessage"] = $"{successCount} rol baÅŸarÄ±yla atandÄ±";
                    if (failureCount > 0)
                    {
                        TempData["WarningMessage"] = $"{failureCount} rol atanamadÄ±";
                    }
                }
                else if (selectedRoles == null || !selectedRoles.Any())
                {
                    TempData["SuccessMessage"] = "TÃ¼m roller kaldÄ±rÄ±ldÄ±";
                }
                else
                {
                    TempData["ErrorMessage"] = "HiÃ§bir rol atanamadÄ±";
                }

                System.Diagnostics.Debug.WriteLine($"BulkUpdateUserRoles completed: Success={successCount}, Failures={failureCount}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BulkUpdateUserRoles exception: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                TempData["ErrorMessage"] = $"Hata: {ex.Message}";
                
                // Hata durumunda da cache'i temizle
                try
                {
                    DynamicPermissionHelper.ClearPermissionCache();
                }
                catch { }
            }

            return RedirectToAction("Index");
        }
    }

    // View Model Classes
    public class RoleOrganizationViewModel
    {
        public List<Role> Roles { get; set; }
        public List<User> Users { get; set; }
        public List<AccessMatrixItem> AccessMatrix { get; set; }
        public List<ControllerPageInfo> ControllerPages { get; set; }
        public Dictionary<string, int> RoleStatistics { get; set; }
    }

    public class AccessMatrixItem
    {
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public string Description { get; set; }
        public string Permission { get; set; }
        public string RequiredPermissions { get; set; }
    }

    public class ControllerPageInfo
    {
        public string ControllerName { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
    }
}
