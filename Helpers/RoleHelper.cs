using System.Collections.Generic;
using System.Linq;
using System.Web;
using MZDNETWORK.Models;
using MZDNETWORK.Data;
using System;
using System.Data.Entity;

namespace MZDNETWORK.Helpers
{
    /// <summary>
    /// Tamamen dinamik rol yönetimi sistemi
    /// Artık hiçbir statik rol tanımı yok - tüm roller dinamik olarak oluşturulur
    /// </summary>
    public static class RoleHelper
    {
        /// <summary>
        /// Kullanıcının sahip olduğu tüm rolleri string array olarak döndürür
        /// </summary>
        public static string[] GetUserRoles(int userId)
        {
            try
            {
                using (var context = new MZDNETWORKContext())
                {
                    System.Diagnostics.Debug.WriteLine($"GetUserRoles called for userId: {userId}");
                    
                    var user = context.Users
                        .Include("UserRoles.Role")
                        .FirstOrDefault(u => u.Id == userId);
                    
                    if (user != null)
                    {
                        // UserRoles collection'ını kullan (multiple roles desteği)
                        if (user.UserRoles != null && user.UserRoles.Any())
                        {
                            var roles = user.UserRoles.Select(ur => ur.Role.Name).ToArray();
                            System.Diagnostics.Debug.WriteLine($"Found {roles.Length} roles for user {userId}: {string.Join(", ", roles)}");
                            return roles;
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"No roles found for user {userId}");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"User {userId} not found");
                    }
                    
                    return new string[0];
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetUserRoles: {ex.Message}");
                return new string[0];
            }
        }
        
        /// <summary>
        /// Kullanıcının belirli bir rolü olup olmadığını kontrol eder
        /// </summary>
        public static bool UserHasRole(int userId, string roleName)
        {
            var roles = GetUserRoles(userId);
            return roles.Contains(roleName);
        }
        
        /// <summary>
        /// Kullanıcının herhangi bir rolü olup olmadığını kontrol eder
        /// </summary>
        public static bool UserHasAnyRole(int userId, params string[] roleNames)
        {
            var userRoles = GetUserRoles(userId);
            return roleNames.Any(role => userRoles.Contains(role));
        }
        
        /// <summary>
        /// Kullanıcıya yeni rol atar
        /// </summary>
        public static bool AssignRoleToUser(int userId, string roleName)
        {
            try
            {
                using (var context = new MZDNETWORKContext())
                {
                    System.Diagnostics.Debug.WriteLine($"AssignRoleToUser: userId={userId}, roleName={roleName}");
                    
                    var user = context.Users.Find(userId);
                    var role = context.Roles.FirstOrDefault(r => r.Name == roleName);
                    
                    if (user == null)
                    {
                        System.Diagnostics.Debug.WriteLine($"User {userId} not found");
                        return false;
                    }
                    
                    // Eğer rol yoksa otomatik olarak oluştur
                    if (role == null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Role {roleName} not found, creating...");
                        role = new Role
                        {
                            Name = roleName,
                            Description = $"{roleName} rolü",
                            IsActive = true,
                            CreatedDate = DateTime.Now,
                            CreatedBy = 0
                        };
                        context.Roles.Add(role);
                        context.SaveChanges();
                        System.Diagnostics.Debug.WriteLine($"Role {roleName} created with ID {role.Id}");
                    }
                    
                    // Kullanıcının bu rolü zaten var mı kontrol et
                    var existingUserRole = context.UserRoles
                        .FirstOrDefault(ur => ur.UserId == userId && ur.RoleId == role.Id);
                    
                    if (existingUserRole == null)
                    {
                        var userRole = new UserRole
                        {
                            UserId = userId,
                            RoleId = role.Id,
                            AssignedDate = DateTime.Now,
                            AssignedBy = 0, // TODO: Get current user ID
                            IsActive = true
                        };
                        context.UserRoles.Add(userRole);
                        
                        context.SaveChanges();
                        System.Diagnostics.Debug.WriteLine($"UserRole created: UserId={userId}, RoleId={role.Id}");
                        
                        // CACHE TEMİZLE: Rol ataması yapıldıktan sonra
                        try
                        {
                            DynamicPermissionHelper.InvalidateUserCache(userId);
                            System.Diagnostics.Debug.WriteLine($"Cache cleared for user {userId} after role assignment");
                        }
                        catch (Exception cacheEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"Cache clear error: {cacheEx.Message}");
                        }
                        
                        return true;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"UserRole already exists: UserId={userId}, RoleId={role.Id}");
                        return false; // Rol zaten mevcut
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in AssignRoleToUser: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }
        
        /// <summary>
        /// Kullanıcıdan rol kaldırır
        /// </summary>
        public static bool RemoveRoleFromUser(int userId, string roleName)
        {
            try
            {
                using (var context = new MZDNETWORKContext())
                {
                    System.Diagnostics.Debug.WriteLine($"RemoveRoleFromUser: userId={userId}, roleName={roleName}");
                    
                    var role = context.Roles.FirstOrDefault(r => r.Name == roleName);
                    if (role == null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Role {roleName} not found");
                        return false;
                    }
                    
                    var userRole = context.UserRoles
                        .FirstOrDefault(ur => ur.UserId == userId && ur.RoleId == role.Id);
                    
                    if (userRole != null)
                    {
                        context.UserRoles.Remove(userRole);
                        context.SaveChanges();
                        System.Diagnostics.Debug.WriteLine($"UserRole removed: UserId={userId}, RoleId={role.Id}");
                        
                        // CACHE TEMİZLE: Rol kaldırıldıktan sonra
                        try
                        {
                            DynamicPermissionHelper.InvalidateUserCache(userId);
                            System.Diagnostics.Debug.WriteLine($"Cache cleared for user {userId} after role removal");
                        }
                        catch (Exception cacheEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"Cache clear error: {cacheEx.Message}");
                        }
                        
                        return true;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"UserRole not found: UserId={userId}, RoleId={role.Id}");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in RemoveRoleFromUser: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Kullanıcının tüm rollerini kaldırır
        /// </summary>
        public static void RemoveAllRolesFromUser(int userId)
        {
            try
            {
                using (var context = new MZDNETWORKContext())
                {
                    System.Diagnostics.Debug.WriteLine($"RemoveAllRolesFromUser: userId={userId}");
                    
                    var userRoles = context.UserRoles.Where(ur => ur.UserId == userId).ToList();
                    System.Diagnostics.Debug.WriteLine($"Found {userRoles.Count} roles to remove for user {userId}");
                    
                    if (userRoles.Any())
                    {
                        context.UserRoles.RemoveRange(userRoles);
                        context.SaveChanges();
                        System.Diagnostics.Debug.WriteLine($"Removed {userRoles.Count} roles for user {userId}");
                        
                        // CACHE TEMİZLE: Tüm roller kaldırıldıktan sonra
                        try
                        {
                            DynamicPermissionHelper.InvalidateUserCache(userId);
                            System.Diagnostics.Debug.WriteLine($"Cache cleared for user {userId} after removing all roles");
                        }
                        catch (Exception cacheEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"Cache clear error: {cacheEx.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in RemoveAllRolesFromUser: {ex.Message}");
                throw; // Re-throw to handle in caller
            }
        }
        
        /// <summary>
        /// Tüm rolleri getirir
        /// </summary>
        public static List<Role> GetAllRoles()
        {
            using (var context = new MZDNETWORKContext())
            {
                return context.Roles.OrderBy(r => r.Name).ToList();
            }
        }
        
        /// <summary>
        /// Yeni rol oluşturur
        /// </summary>
        public static bool CreateRole(string roleName, string description = null)
        {
            using (var context = new MZDNETWORKContext())
            {
                var existingRole = context.Roles.FirstOrDefault(r => r.Name == roleName);
                if (existingRole != null)
                    return false; // Rol zaten mevcut
                
                context.Roles.Add(new Role
                {
                    Name = roleName,
                    Description = description ?? $"{roleName} rolü",
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    CreatedBy = 0
                });
                
                context.SaveChanges();
                return true;
            }
        }

        /// <summary>
        /// Rol günceller
        /// </summary>
        public static bool UpdateRole(int roleId, string newName = null, string newDescription = null)
        {
            using (var context = new MZDNETWORKContext())
            {
                var role = context.Roles.Find(roleId);
                if (role == null)
                    return false;

                if (!string.IsNullOrEmpty(newName))
                {
                    // Aynı isimde başka rol var mı kontrol et
                    var existingRole = context.Roles.FirstOrDefault(r => r.Name == newName && r.Id != roleId);
                    if (existingRole != null)
                        return false; // Aynı isimde rol zaten var

                    role.Name = newName;
                }

                if (newDescription != null)
                    role.Description = newDescription;

                context.SaveChanges();
                return true;
            }
        }

        /// <summary>
        /// Rol siler (sadece kullanımda olmayan rolleri)
        /// </summary>
        public static bool DeleteRole(int roleId)
        {
            using (var context = new MZDNETWORKContext())
            {
                var role = context.Roles.Find(roleId);
                if (role == null)
                    return false;

                // Rol kullanımda mı kontrol et
                var isInUse = context.UserRoles.Any(ur => ur.RoleId == roleId) ||
                             context.RolePermissions.Any(rp => rp.RoleId == roleId);

                if (isInUse)
                    return false; // Rol kullanımda, silinemez

                context.Roles.Remove(role);
                context.SaveChanges();
                return true;
            }
        }

        /// <summary>
        /// Rol ismine göre rol ID'sini getirir
        /// </summary>
        public static int? GetRoleIdByName(string roleName)
        {
            using (var context = new MZDNETWORKContext())
            {
                var role = context.Roles.FirstOrDefault(r => r.Name == roleName);
                return role?.Id;
            }
        }

        /// <summary>
        /// Kullanıcı sayısına göre en çok kullanılan rolleri getirir
        /// </summary>
        public static List<Role> GetMostUsedRoles(int count = 10)
        {
            using (var context = new MZDNETWORKContext())
            {
                return context.Roles
                    .GroupJoin(context.UserRoles, 
                        role => role.Id, 
                        userRole => userRole.RoleId, 
                        (role, userRoles) => new { Role = role, UserCount = userRoles.Count() })
                    .OrderByDescending(x => x.UserCount)
                    .Take(count)
                    .Select(x => x.Role)
                    .ToList();
            }
        }

        /// <summary>
        /// Belirli bir role sahip kullanıcı sayısını getirir
        /// </summary>
        public static int GetRoleUserCount(string roleName)
        {
            using (var context = new MZDNETWORKContext())
            {
                var role = context.Roles.FirstOrDefault(r => r.Name == roleName);
                if (role == null)
                    return 0;

                return context.UserRoles.Count(ur => ur.RoleId == role.Id);
            }
        }

        /// <summary>
        /// Rol istatistiklerini getirir
        /// </summary>
        public static Dictionary<string, int> GetRoleStatistics()
        {
            using (var context = new MZDNETWORKContext())
            {
                return context.Roles
                    .GroupJoin(context.UserRoles,
                        role => role.Id,
                        userRole => userRole.RoleId,
                        (role, userRoles) => new { RoleName = role.Name, UserCount = userRoles.Count() })
                    .ToDictionary(x => x.RoleName, x => x.UserCount);
            }
        }
    }
}