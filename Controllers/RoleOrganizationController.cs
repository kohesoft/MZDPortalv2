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
    [DynamicAuthorize(Permission = "RoleManagement.RoleManagement")]
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
        /// Ana rol organizasyon sayfası
        /// </summary>
        public ActionResult Index()
        {
            var model = new RoleOrganizationViewModel
            {
                Roles = RoleHelper.GetAllRoles(),
                // Entity Framework Include ile UserRoles ve Role ilişkilerini yükle
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
        /// Dinamik rol oluşturma
        /// </summary>
        [HttpPost]
        [DynamicAuthorize(Permission = "RoleManagement.RoleManagement", Action = "Create")]
        public ActionResult CreateDynamicRole(string roleName, string description)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                TempData["ErrorMessage"] = "Rol adı boş olamaz";
                return RedirectToAction("Index");
            }

            bool success = RoleHelper.CreateRole(roleName.Trim(), description?.Trim());
            
            if (success)
            {
                TempData["SuccessMessage"] = $"'{roleName}' rolü başarıyla oluşturuldu";
            }
            else
            {
                TempData["ErrorMessage"] = $"'{roleName}' rolü zaten mevcut";
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Erişim matrisini oluşturur (permission-based)
        /// PermissionSeeder ile uyumlu güncel permission'lar
        /// </summary>
        private List<AccessMatrixItem> GetAccessMatrix()
        {
            return new List<AccessMatrixItem>
            {
                // Kullanıcı Yönetimi
                new AccessMatrixItem
                {
                    ControllerName = "Kullanici_Islemleri",
                    ActionName = "All Actions",
                    Description = "Kullanıcı İşlemleri",
                    Permission = "UserManagement",
                    RequiredPermissions = "UserManagement.View, UserManagement.Create, UserManagement.Edit, UserManagement.Delete, UserManagement.Manage, UserManagement.Roles, UserManagement.Passwords"
                },

                // Rol Yönetimi
                new AccessMatrixItem
                {
                    ControllerName = "RolePermission",
                    ActionName = "Matrix Management",
                    Description = "Rol-Yetki Matrisi",
                    Permission = "RoleManagement.Permissions",
                    RequiredPermissions = "RoleManagement.Permissions.Edit, RoleManagement.Permissions.Manage, RoleManagement.Permissions.Delete"
                },
                new AccessMatrixItem
                {
                    ControllerName = "RoleManagement",
                    ActionName = "Role Operations",
                    Description = "Rol Yönetimi",
                    Permission = "SystemManagement.RoleManagement",
                    RequiredPermissions = "SystemManagement.RoleManagement.View, SystemManagement.RoleManagement.Create, SystemManagement.RoleManagement.Edit, SystemManagement.RoleManagement.Delete"
                },
                new AccessMatrixItem
                {
                    ControllerName = "RoleOrganization",
                    ActionName = "Organization",
                    Description = "Rol Organizasyonu",
                    Permission = "SystemManagement.RoleManagement",
                    RequiredPermissions = "SystemManagement.RoleManagement.View, SystemManagement.RoleManagement.Create, SystemManagement.RoleManagement.Edit"
                },

                // İnsan Kaynakları
                new AccessMatrixItem
                {
                    ControllerName = "InsanKaynaklari",
                    ActionName = "HR Operations",
                    Description = "İnsan Kaynakları",
                    Permission = "HumanResources",
                    RequiredPermissions = "HumanResources.LeaveRequests.View, HumanResources.Suggestions, HumanResources.Announcements, HumanResources.Performance, HumanResources.Surveys"
                },

                // Sistem Yönetimi
                new AccessMatrixItem
                {
                    ControllerName = "PermissionTree",
                    ActionName = "Permission Management",
                    Description = "Yetki Ağacı Yönetimi",
                    Permission = "SystemManagement.Permissions",
                    RequiredPermissions = "SystemManagement.Permissions.Create, SystemManagement.Permissions.Edit, SystemManagement.Permissions.Delete, SystemManagement.Permissions.Manage"
                },

                // Bilgi İşlem
                new AccessMatrixItem
                {
                    ControllerName = "BilgiIslem",
                    ActionName = "IT Operations",
                    Description = "Bilgi İşlem İşlemleri",
                    Permission = "IT",
                    RequiredPermissions = "IT.View, IT.FoodPhoto.Merkez.Create, IT.FoodPhoto.Merkez.Delete, IT.FoodPhoto.Yerleske.Create, IT.FoodPhoto.Yerleske.Delete, IT.BreakPhoto.Create, IT.BreakPhoto.Delete"
                },
                new AccessMatrixItem
                {
                    ControllerName = "InformationTechnology",
                    ActionName = "System Management",
                    Description = "Sistem Ayarları",
                    Permission = "InformationTechnology",
                    RequiredPermissions = "InformationTechnology.Settings, InformationTechnology.Notifications, InformationTechnology.MenuManagement, InformationTechnology.TVManagement, InformationTechnology.BackupRestore, InformationTechnology.Monitoring"
                },

                // Dokümantasyon
                new AccessMatrixItem
                {
                    ControllerName = "Dokumantasyon",
                    ActionName = "Document Management",
                    Description = "Dokümantasyon Yönetimi",
                    Permission = "Documentation",
                    RequiredPermissions = "Documentation.View, Documentation.Create, Documentation.Edit, Documentation.Delete, Documentation.Manage, Documentation.StockCards, Documentation.Requests"
                },

                // Duyurular
                new AccessMatrixItem
                {
                    ControllerName = "Gonderi",
                    ActionName = "Announcements",
                    Description = "Duyuru Yönetimi",
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

                // Öneri/Şikayet
                new AccessMatrixItem
                {
                    ControllerName = "DilekOneri",
                    ActionName = "Suggestions",
                    Description = "Öneri/Şikayet Sistemi",
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
                    Description = "Kişisel Bildirimler",
                    Permission = "Notification",
                    RequiredPermissions = "Notification.View"
                },

                // Kişi Rehberi
                new AccessMatrixItem
                {
                    ControllerName = "Contact",
                    ActionName = "Contact Directory",
                    Description = "Kişi Rehberi",
                    Permission = "Contact",
                    RequiredPermissions = "Contact.View, Contact.Export.Export"
                },

                // Online Kullanıcılar
                new AccessMatrixItem
                {
                    ControllerName = "OnlineUsers",
                    ActionName = "Online Users",
                    Description = "Online Kullanıcılar",
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

                // Günlük Ruh Hali
                new AccessMatrixItem
                {
                    ControllerName = "DailyMood",
                    ActionName = "Daily Mood",
                    Description = "Günlük Ruh Hali",
                    Permission = "DailyMood",
                    RequiredPermissions = "DailyMood.View, DailyMood.Create"
                },

                // Operasyonel İşlemler
                new AccessMatrixItem
                {
                    ControllerName = "Operational",
                    ActionName = "Operations",
                    Description = "Operasyonel İşlemler",
                    Permission = "Operational",
                    RequiredPermissions = "Operational.MeetingRoom.View, Operational.Chat.View, Operational.Notifications, Operational.Tasks, Operational.Calendar"
                },

                // Test ve Debug
                new AccessMatrixItem
                {
                    ControllerName = "TestPermission",
                    ActionName = "Testing",
                    Description = "Permission Test",
                    Permission = "SystemManagement.Permissions",
                    RequiredPermissions = "SystemManagement.Permissions.Manage"
                },

                // Hesap İşlemleri (Permission'sız)
                new AccessMatrixItem
                {
                    ControllerName = "Account",
                    ActionName = "Authentication",
                    Description = "Hesap İşlemleri",
                    Permission = "Public",
                    RequiredPermissions = "Herkes erişebilir"
                },

                // Ana Sayfa (Permission'sız)
                new AccessMatrixItem
                {
                    ControllerName = "Home",
                    ActionName = "Dashboard",
                    Description = "Ana Sayfa",
                    Permission = "Public",
                    RequiredPermissions = "Giriş yapmış kullanıcılar"
                }
            };
        }

        /// <summary>
        /// Controller sayfalarını kategorilere göre listeler
        /// PermissionSeeder ile uyumlu güncel controller listesi
        /// </summary>
        
        private List<ControllerPageInfo> GetControllerPages()
        {
            return new List<ControllerPageInfo>
            {
                // Kullanıcı Yönetimi
                new ControllerPageInfo { ControllerName = "Kullanici_Islemleri", Description = "Kullanıcı İşlemleri", Category = "Kullanıcı Yönetimi" },
                new ControllerPageInfo { ControllerName = "Account", Description = "Hesap İşlemleri", Category = "Kullanıcı Yönetimi" },

                // Rol Yönetimi
                new ControllerPageInfo { ControllerName = "RolePermission", Description = "Rol-Yetki Matrisi", Category = "Rol Yönetimi" },
                new ControllerPageInfo { ControllerName = "RoleManagement", Description = "Rol Yönetimi", Category = "Rol Yönetimi" },
                new ControllerPageInfo { ControllerName = "RoleOrganization", Description = "Rol Organizasyonu", Category = "Rol Yönetimi" },

                // İnsan Kaynakları
                new ControllerPageInfo { ControllerName = "InsanKaynaklari", Description = "İnsan Kaynakları", Category = "İnsan Kaynakları" },
                new ControllerPageInfo { ControllerName = "DilekOneri", Description = "Öneri/Şikayet", Category = "İnsan Kaynakları" },
                new ControllerPageInfo { ControllerName = "Performance", Description = "Performans", Category = "İnsan Kaynakları" },
                new ControllerPageInfo { ControllerName = "DailyMood", Description = "Günlük Ruh Hali", Category = "İnsan Kaynakları" },

                // Bilgi İşlem
                new ControllerPageInfo { ControllerName = "BilgiIslem", Description = "Bilgi İşlem İşlemleri", Category = "Bilgi İşlem" },
                new ControllerPageInfo { ControllerName = "BeyazTahta", Description = "Beyaz Tahta", Category = "Bilgi İşlem" },

                // Dokümantasyon
                new ControllerPageInfo { ControllerName = "Dokumantasyon", Description = "Dokümantasyon", Category = "Dokümantasyon" },

                // Sistem Yönetimi
                new ControllerPageInfo { ControllerName = "PermissionTree", Description = "Yetki Ağacı", Category = "Sistem Yönetimi" },
                new ControllerPageInfo { ControllerName = "TestPermission", Description = "Permission Test", Category = "Sistem Yönetimi" },

                // Operasyonel İşlemler
                new ControllerPageInfo { ControllerName = "Home", Description = "Ana Sayfa", Category = "Operasyonel" },
                new ControllerPageInfo { ControllerName = "Notification", Description = "Bildirim Yönetimi", Category = "Operasyonel" },
                new ControllerPageInfo { ControllerName = "Bildirimlerim", Description = "Kişisel Bildirimler", Category = "Operasyonel" },
                new ControllerPageInfo { ControllerName = "OnlineUsers", Description = "Online Kullanıcılar", Category = "Operasyonel" },
                new ControllerPageInfo { ControllerName = "Contact", Description = "Kişi Rehberi", Category = "Operasyonel" },
                new ControllerPageInfo { ControllerName = "Chat", Description = "Chat Sistemi", Category = "Operasyonel" },

                // Anket ve Geri Bildirim
                new ControllerPageInfo { ControllerName = "Answer", Description = "Anket Sistemi", Category = "Anket/Değerlendirme" },
                new ControllerPageInfo { ControllerName = "Feedback", Description = "Geri Bildirim", Category = "Anket/Değerlendirme" },

                // Duyurular
                new ControllerPageInfo { ControllerName = "Gonderi", Description = "Duyuru Yönetimi", Category = "Duyurular" }
            };
        }
        

        /// <summary>
        /// Rol istatistikleri API
        /// </summary>
        [DynamicAuthorize(Permission = "RoleManagement.RoleManagement")]
        public JsonResult GetRoleStatistics()
        {
            var stats = RoleHelper.GetRoleStatistics();
            return Json(stats, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// En çok kullanılan rolleri getir
        /// </summary>
        [DynamicAuthorize(Permission = "RoleManagement.RoleManagement")]
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
        /// Kullanıcı rollerini toplu güncelleme
        /// </summary>
        [HttpPost]
        [DynamicAuthorize(Permission = "RoleManagement.RoleManagement")]
        [ValidateAntiForgeryToken]
        public ActionResult BulkUpdateUserRoles(int userId, List<string> selectedRoles)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"BulkUpdateUserRoles called: UserId={userId}, Roles={string.Join(",", selectedRoles ?? new List<string>())}");

                // Kullanıcının var olup olmadığını kontrol et
                var user = db.Users.Find(userId);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Kullanıcı bulunamadı";
                    System.Diagnostics.Debug.WriteLine($"User not found: {userId}");
                    return RedirectToAction("Index");
                }

                // ÖNCESİNDE: Kullanıcının cache'ini temizle
                System.Diagnostics.Debug.WriteLine($"Clearing cache for user {userId} before role update");
                DynamicPermissionHelper.InvalidateUserCache(userId);

                // Önce mevcut tüm rolleri kaldır
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

                // SONRASINDA: Kullanıcının cache'ini tekrar temizle
                System.Diagnostics.Debug.WriteLine($"Clearing cache for user {userId} after role update");
                DynamicPermissionHelper.InvalidateUserCache(userId);
                
                // Güvenlik için tüm cache'i de temizle
                DynamicPermissionHelper.ClearPermissionCache();
                System.Diagnostics.Debug.WriteLine("All permission cache cleared for security");

                // Sonuç mesajları
                if (successCount > 0)
                {
                    TempData["SuccessMessage"] = $"{successCount} rol başarıyla atandı";
                    if (failureCount > 0)
                    {
                        TempData["WarningMessage"] = $"{failureCount} rol atanamadı";
                    }
                }
                else if (selectedRoles == null || !selectedRoles.Any())
                {
                    TempData["SuccessMessage"] = "Tüm roller kaldırıldı";
                }
                else
                {
                    TempData["ErrorMessage"] = "Hiçbir rol atanamadı";
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