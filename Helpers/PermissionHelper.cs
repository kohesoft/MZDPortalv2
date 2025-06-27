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
    /// Dinamik yetki sistemi için ana helper sınıfı
    /// Hiyerarşik yetki kontrolü, cache yönetimi ve inheritance logic
    /// </summary>
    public static class PermissionHelper
    {
        /// <summary>
        /// Kullanıcının belirtilen yetki yoluna erişimi olup olmadığını kontrol eder
        /// </summary>
        /// <param name="userId">Kullanıcı ID</param>
        /// <param name="permissionPath">Yetki yolu (örn: "UserManagement.Create")</param>
        /// <param name="action">CRUD aksiyonu (View, Create, Edit, Delete, Manage)</param>
        /// <returns>Yetki var mı</returns>
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

                // Cache'de yoksa veritabanından al ve cache'le
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
        /// Username ile yetki kontrolü (backward compatibility)
        /// </summary>
        public static bool CheckPermission(string username, string permissionPath, string action = "View")
        {
            var userId = GetUserIdByUsername(username);
            return userId > 0 && CheckPermission(userId, permissionPath, action);
        }

        /// <summary>
        /// Kullanıcının tüm yetki bilgilerini veritabanından alır
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

                // Kullanıcının rollerini al (hem yeni hem eski sistem)
                var userRoles = GetUserRoles(userId);
                userPermissionData.Roles.AddRange(userRoles);

                // SuperAdmin kontrolü
                userPermissionData.IsSuperAdmin = userRoles.Contains("SuperAdmin") || userRoles.Contains("Sys");

                if (userPermissionData.IsSuperAdmin)
                {
                    // SuperAdmin tüm yetkilere sahip
                    userPermissionData.Permissions = GetAllPermissions();
                }
                else
                {
                    // Normal kullanıcı için rol bazlı yetkileri al
                    userPermissionData.Permissions = GetRoleBasedPermissions(userRoles.ToList(), context);
                }

                return userPermissionData;
            }
        }

        /// <summary>
        /// Cache'den yetki kontrolü
        /// </summary>
        private static bool CheckPermissionFromCache(UserPermissionData permissions, string permissionPath, string action)
        {
            return CheckPermissionFromData(permissions, permissionPath, action);
        }

        /// <summary>
        /// UserPermissionData'dan yetki kontrolü (inheritance logic ile)
        /// </summary>
        private static bool CheckPermissionFromData(UserPermissionData permissions, string permissionPath, string action)
        {
            if (permissions == null) return false;

            // SuperAdmin kontrolü
            if (permissions.IsSuperAdmin) return true;

            // Tam path kontrolü
            if (permissions.Permissions.ContainsKey(permissionPath))
            {
                return CheckActionPermission(permissions.Permissions[permissionPath], action);
            }

            // Inheritance kontrolü (parent yetkileri)
            return CheckInheritedPermission(permissions.Permissions, permissionPath, action);
        }

        /// <summary>
        /// Parent yetki kontrolü (inheritance logic)
        /// UserManagement.Create → UserManagement → Portal
        /// </summary>
        private static bool CheckInheritedPermission(Dictionary<string, PermissionDetails> permissions, string permissionPath, string action)
        {
            var pathParts = permissionPath.Split('.');
            
            // Yukarıdan aşağıya parent path'leri kontrol et
            for (int i = pathParts.Length - 1; i > 0; i--)
            {
                var parentPath = string.Join(".", pathParts.Take(i));
                
                if (permissions.ContainsKey(parentPath))
                {
                    if (CheckActionPermission(permissions[parentPath], action))
                        return true;
                }

                // Wildcard kontrolü (UserManagement.*)
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
        /// Kullanıcının tüm rollerini al (backward compatibility ile)
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
        /// Tüm yetkileri al (SuperAdmin için)
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
        /// Rol bazlı yetkileri al
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
        /// Kullanıcının belirli bir rolü olup olmadığını kontrol et
        /// </summary>
        public static bool UserHasRole(int userId, string roleName)
        {
            var userRoles = GetUserRoles(userId);
            return userRoles.Contains(roleName);
        }

        /// <summary>
        /// Kullanıcının herhangi bir rolü olup olmadığını kontrol et
        /// </summary>
        public static bool UserHasAnyRole(int userId, params string[] roleNames)
        {
            var userRoles = GetUserRoles(userId);
            return roleNames.Any(role => userRoles.Contains(role));
        }

        /// <summary>
        /// Kullanıcının cache'ini temizle
        /// </summary>
        public static void InvalidateUserCache(int userId)
        {
            PermissionCacheService.InvalidateUserCache(userId);
        }

        /// <summary>
        /// Tüm cache'i temizle
        /// </summary>
        public static void InvalidateAllCache()
        {
            PermissionCacheService.InvalidateAllCache();
        }
    }

    /// <summary>
    /// Kullanıcı yetki verisi modeli
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
    /// Yetki detayları modeli
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