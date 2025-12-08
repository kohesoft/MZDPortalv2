using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MZDNETWORK.Models;
using MZDNETWORK.Data;
using Newtonsoft.Json;

namespace MZDNETWORK.Helpers
{
    /// <summary>
    /// Dinamik yetki sistemi iÃ§in ana helper sÄ±nÄ±fÄ±
    /// HiyerarÅŸik yetki kontrolÃ¼, cache yÃ¶netimi ve inheritance logic
    /// </summary>
    public static class PermissionHelper
    {
        /// <summary>
        /// KullanÄ±cÄ±nÄ±n belirtilen yetki yoluna eriÅŸimi olup olmadÄ±ÄŸÄ±nÄ± kontrol eder
        /// </summary>
        /// <param name="userId">KullanÄ±cÄ± ID</param>
        /// <param name="permissionPath">Yetki yolu (Ã¶rn: "KullaniciYonetimi.Create")</param>
        /// <param name="action">CRUD aksiyonu (View, Create, Edit, Delete, Manage)</param>
        /// <returns>Yetki var mÄ±</returns>
        public static bool CheckPermission(int userId, string permissionPath, string action = "View")
        {
            try
            {
                // Cache'den kontrol et
                var cachedPermissions = PermissionCacheService.GetUserPermissions(userId);
                if (cachedPermissions != null)
                {
                    return CheckPermissionFromCache(cachedPermissions, permissionPath, action);
                }

                // Cache'de yoksa veritabanÄ±ndan al ve cache'le
                var permissions = GetUserPermissionsFromDatabase(userId);
                PermissionCacheService.CacheUserPermissions(userId, permissions);

                return CheckPermissionFromData(permissions, permissionPath, action);
            }
            catch (Exception ex)
            {
                // Log error
                System.Diagnostics.Debug.WriteLine($"Permission check error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Username ile yetki kontrolÃ¼ (backward compatibility)
        /// </summary>
        public static bool CheckPermission(string username, string permissionPath, string action = "View")
        {
            var userId = GetUserIdByUsername(username);
            return userId > 0 && CheckPermission(userId, permissionPath, action);
        }

        /// <summary>
        /// KullanÄ±cÄ±nÄ±n tÃ¼m yetki bilgilerini veritabanÄ±ndan alÄ±r
        /// </summary>
        private static UserPermissionData GetUserPermissionsFromDatabase(int userId)
        {
            using (var context = new MZDNETWORKContext())
            {
                var user = context.Users.Find(userId);
                if (user == null) return null;

                var userPermissionData = new UserPermissionData
                {
                    UserId = userId,
                    Username = user.Username,
                    Roles = new List<string>(),
                    Permissions = new Dictionary<string, PermissionDetails>(),
                    IsSuperAdmin = false,
                    LastUpdated = DateTime.Now
                };

                // KullanÄ±cÄ±nÄ±n rollerini al (hem yeni hem eski sistem)
                var userRoles = GetUserRoles(userId);
                userPermissionData.Roles.AddRange(userRoles);

                // SuperAdmin kontrolÃ¼
                userPermissionData.IsSuperAdmin = userRoles.Contains("SuperAdmin") || userRoles.Contains("Sys");

                if (userPermissionData.IsSuperAdmin)
                {
                    // SuperAdmin tÃ¼m yetkilere sahip
                    userPermissionData.Permissions = GetAllPermissions();
                }
                else
                {
                    // Normal kullanÄ±cÄ± iÃ§in rol bazlÄ± yetkileri al
                    userPermissionData.Permissions = GetRoleBasedPermissions(userRoles.ToList(), context);
                }

                return userPermissionData;
            }
        }

        /// <summary>
        /// Cache'den yetki kontrolÃ¼
        /// </summary>
        private static bool CheckPermissionFromCache(UserPermissionData permissions, string permissionPath, string action)
        {
            return CheckPermissionFromData(permissions, permissionPath, action);
        }

        /// <summary>
        /// UserPermissionData'dan yetki kontrolÃ¼ (inheritance logic ile)
        /// </summary>
        private static bool CheckPermissionFromData(UserPermissionData permissions, string permissionPath, string action)
        {
            if (permissions == null) return false;

            // SuperAdmin kontrolÃ¼
            if (permissions.IsSuperAdmin) return true;

            // Tam path kontrolÃ¼
            if (permissions.Permissions.ContainsKey(permissionPath))
            {
                return CheckActionPermission(permissions.Permissions[permissionPath], action);
            }

            // Inheritance kontrolÃ¼ (parent yetkileri)
            return CheckInheritedPermission(permissions.Permissions, permissionPath, action);
        }

        /// <summary>
        /// Parent yetki kontrolÃ¼ (inheritance logic)
        /// KullaniciYonetimi.Create â†’ KullaniciYonetimi â†’ Portal
        /// </summary>
        private static bool CheckInheritedPermission(Dictionary<string, PermissionDetails> permissions, string permissionPath, string action)
        {
            var pathParts = permissionPath.Split('.');
            
            // YukarÄ±dan aÅŸaÄŸÄ±ya parent path'leri kontrol et
            for (int i = pathParts.Length - 1; i > 0; i--)
            {
                var parentPath = string.Join(".", pathParts.Take(i));
                
                if (permissions.ContainsKey(parentPath))
                {
                    if (CheckActionPermission(permissions[parentPath], action))
                        return true;
                }

                // Wildcard kontrolÃ¼ (KullaniciYonetimi.*)
                var wildcardPath = parentPath + ".*";
                if (permissions.ContainsKey(wildcardPath))
                {
                    if (CheckActionPermission(permissions[wildcardPath], action))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Belirli aksiyonun yetkisini kontrol et
        /// </summary>
        private static bool CheckActionPermission(PermissionDetails permission, string action)
        {
            switch (action.ToLower())
            {
                case "view": return permission.CanView;
                case "create": return permission.CanCreate;
                case "edit": return permission.CanEdit;
                case "delete": return permission.CanDelete;
                case "manage": return permission.CanManage;
                case "approve": return permission.CanApprove;
                case "reject": return permission.CanReject;
                case "export": return permission.CanExport;
                case "import": return permission.CanImport;
                default: return permission.CanView; // Default olarak view
            }
        }

        /// <summary>
        /// KullanÄ±cÄ±nÄ±n tÃ¼m rollerini al (backward compatibility ile)
        /// </summary>
        public static string[] GetUserRoles(int userId)
        {
            using (var context = new MZDNETWORKContext())
            {
                var user = context.Users.Find(userId);
                if (user == null) return new string[0];

                var roles = new List<string>();

                // Yeni multiple roles sisteminden al
                var newRoles = context.UserRoles
                    .Where(ur => ur.UserId == userId)
                    .Select(ur => ur.Role.Name)
                    .ToList();

                if (newRoles.Any())
                {
                    roles.AddRange(newRoles);
                }

                return roles.ToArray();
            }
        }

        /// <summary>
        /// Username'den UserId al
        /// </summary>
        private static int GetUserIdByUsername(string username)
        {
            using (var context = new MZDNETWORKContext())
            {
                var user = context.Users.FirstOrDefault(u => u.Username == username);
                return user?.Id ?? 0;
            }
        }

        /// <summary>
        /// TÃ¼m yetkileri al (SuperAdmin iÃ§in)
        /// </summary>
        private static Dictionary<string, PermissionDetails> GetAllPermissions()
        {
            using (var context = new MZDNETWORKContext())
            {
                var permissions = new Dictionary<string, PermissionDetails>();
                
                var allPermissionNodes = context.PermissionNodes
                    .Where(p => p.IsActive)
                    .ToList();

                foreach (var node in allPermissionNodes)
                {
                    permissions[node.Path] = new PermissionDetails
                    {
                        CanView = true,
                        CanCreate = true,
                        CanEdit = true,
                        CanDelete = true,
                        CanManage = true,
                        CanApprove = true,
                        CanReject = true,
                        CanExport = true,
                        CanImport = true
                    };
                }

                return permissions;
            }
        }

        /// <summary>
        /// Rol bazlÄ± yetkileri al
        /// </summary>
        private static Dictionary<string, PermissionDetails> GetRoleBasedPermissions(List<string> userRoles, MZDNETWORKContext context)
        {
            var permissions = new Dictionary<string, PermissionDetails>();

            var roleIds = context.Roles
                .Where(r => userRoles.Contains(r.Name))
                .Select(r => r.Id)
                .ToList();

            var rolePermissions = context.RolePermissions
                .Where(rp => roleIds.Contains(rp.RoleId) && rp.IsActive)
                .Select(rp => new
                {
                    Path = rp.PermissionNode.Path,
                    rp.CanView,
                    rp.CanCreate,
                    rp.CanEdit,
                    rp.CanDelete,
                    rp.CanManage,
                    rp.CanApprove,
                    rp.CanReject,
                    rp.CanExport,
                    rp.CanImport
                })
                .ToList();

            foreach (var rp in rolePermissions)
            {
                if (!permissions.ContainsKey(rp.Path))
                {
                    permissions[rp.Path] = new PermissionDetails();
                }

                // Multiple roller varsa OR logic uygula (herhangi bir rolde yetki varsa)
                var existing = permissions[rp.Path];
                existing.CanView = existing.CanView || rp.CanView;
                existing.CanCreate = existing.CanCreate || rp.CanCreate;
                existing.CanEdit = existing.CanEdit || rp.CanEdit;
                existing.CanDelete = existing.CanDelete || rp.CanDelete;
                existing.CanManage = existing.CanManage || rp.CanManage;
                existing.CanApprove = existing.CanApprove || rp.CanApprove;
                existing.CanReject = existing.CanReject || rp.CanReject;
                existing.CanExport = existing.CanExport || rp.CanExport;
                existing.CanImport = existing.CanImport || rp.CanImport;
            }

            return permissions;
        }

        /// <summary>
        /// KullanÄ±cÄ±nÄ±n belirli bir rolÃ¼ olup olmadÄ±ÄŸÄ±nÄ± kontrol et
        /// </summary>
        public static bool UserHasRole(int userId, string roleName)
        {
            var userRoles = GetUserRoles(userId);
            return userRoles.Contains(roleName);
        }

        /// <summary>
        /// KullanÄ±cÄ±nÄ±n herhangi bir rolÃ¼ olup olmadÄ±ÄŸÄ±nÄ± kontrol et
        /// </summary>
        public static bool UserHasAnyRole(int userId, params string[] roleNames)
        {
            var userRoles = GetUserRoles(userId);
            return roleNames.Any(role => userRoles.Contains(role));
        }

        /// <summary>
        /// KullanÄ±cÄ±nÄ±n cache'ini temizle
        /// </summary>
        public static void InvalidateUserCache(int userId)
        {
            PermissionCacheService.InvalidateUserCache(userId);
        }

        /// <summary>
        /// TÃ¼m cache'i temizle
        /// </summary>
        public static void InvalidateAllCache()
        {
            PermissionCacheService.InvalidateAllCache();
        }
    }

    /// <summary>
    /// KullanÄ±cÄ± yetki verisi modeli
    /// </summary>
    public class UserPermissionData
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public List<string> Roles { get; set; }
        public Dictionary<string, PermissionDetails> Permissions { get; set; }
        public bool IsSuperAdmin { get; set; }
        public DateTime LastUpdated { get; set; }

        public UserPermissionData()
        {
            Roles = new List<string>();
            Permissions = new Dictionary<string, PermissionDetails>();
        }
    }

    /// <summary>
    /// Yetki detaylarÄ± modeli
    /// </summary>
    public class PermissionDetails
    {
        public bool CanView { get; set; }
        public bool CanCreate { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanManage { get; set; }
        public bool CanApprove { get; set; }
        public bool CanReject { get; set; }
        public bool CanExport { get; set; }
        public bool CanImport { get; set; }
    }
} 
