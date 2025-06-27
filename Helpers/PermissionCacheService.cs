using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using MZDNETWORK.Data;
using MZDNETWORK.Models;
using Newtonsoft.Json;

namespace MZDNETWORK.Helpers
{
    /// <summary>
    /// Permission cache yönetimi için servis sınıfı
    /// Memory cache ve database cache hibrit yaklaşımı
    /// </summary>
    public static class PermissionCacheService
    {
        private static readonly string CACHE_KEY_PREFIX = "UserPermissions_";
        private static readonly int CACHE_DURATION_MINUTES = 30;

        /// <summary>
        /// Kullanıcının permission'larını cache'den al
        /// </summary>
        public static UserPermissionData GetUserPermissions(int userId)
        {
            try
            {
                // Memory cache'den kontrol et
                var cacheKey = CACHE_KEY_PREFIX + userId;
                var cachedData = HttpRuntime.Cache.Get(cacheKey) as UserPermissionData;
                
                if (cachedData != null && IsValidCache(cachedData))
                {
                    return cachedData;
                }

                // Database cache'den kontrol et
                return GetFromDatabaseCache(userId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PermissionCacheService.GetUserPermissions error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Kullanıcının permission'larını cache'e kaydet
        /// </summary>
        public static void CacheUserPermissions(int userId, UserPermissionData permissions)
        {
            try
            {
                if (permissions == null) return;

                var cacheKey = CACHE_KEY_PREFIX + userId;
                
                // Memory cache'e kaydet
                HttpRuntime.Cache.Insert(
                    cacheKey,
                    permissions,
                    null,
                    DateTime.Now.AddMinutes(CACHE_DURATION_MINUTES),
                    Cache.NoSlidingExpiration,
                    CacheItemPriority.Normal,
                    null
                );

                // Database cache'e kaydet
                SaveToDatabaseCache(userId, permissions);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PermissionCacheService.CacheUserPermissions error: {ex.Message}");
            }
        }

        /// <summary>
        /// Kullanıcının cache'ini temizle
        /// </summary>
        public static void InvalidateUserCache(int userId)
        {
            try
            {
                // Memory cache'den sil
                var cacheKey = CACHE_KEY_PREFIX + userId;
                HttpRuntime.Cache.Remove(cacheKey);

                // Database cache'den sil
                RemoveFromDatabaseCache(userId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PermissionCacheService.InvalidateUserCache error: {ex.Message}");
            }
        }

        /// <summary>
        /// Tüm cache'i temizle
        /// </summary>
        public static void InvalidateAllCache()
        {
            try
            {
                // Memory cache'den tüm permission cache'lerini sil
                var cache = HttpRuntime.Cache;
                var keysToRemove = new List<string>();

                var enumerator = cache.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var key = enumerator.Key.ToString();
                    if (key.StartsWith(CACHE_KEY_PREFIX))
                    {
                        keysToRemove.Add(key);
                    }
                }

                foreach (var key in keysToRemove)
                {
                    cache.Remove(key);
                }

                // Database cache'i temizle
                ClearDatabaseCache();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PermissionCacheService.InvalidateAllCache error: {ex.Message}");
            }
        }

        /// <summary>
        /// Cache'in geçerli olup olmadığını kontrol et
        /// </summary>
        private static bool IsValidCache(UserPermissionData cachedData)
        {
            if (cachedData == null) return false;
            
            // Cache 30 dakikadan eskiyse geçersiz
            return (DateTime.Now - cachedData.LastUpdated).TotalMinutes < CACHE_DURATION_MINUTES;
        }

        /// <summary>
        /// Database cache'den veri al
        /// </summary>
        private static UserPermissionData GetFromDatabaseCache(int userId)
        {
            try
            {
                using (var context = new MZDNETWORKContext())
                {
                    var cacheEntry = context.PermissionCaches
                        .FirstOrDefault(pc => pc.UserId == userId && pc.IsActive && pc.ExpiresAt > DateTime.Now);

                    if (cacheEntry != null)
                    {
                        // Hit count'u artır ve last access'i güncelle
                        cacheEntry.MarkAsAccessed();
                        context.SaveChanges();

                        // JSON'dan deserialize et
                        return JsonConvert.DeserializeObject<UserPermissionData>(cacheEntry.PermissionsJson);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetFromDatabaseCache error: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Database cache'e veri kaydet
        /// </summary>
        private static void SaveToDatabaseCache(int userId, UserPermissionData permissions)
        {
            try
            {
                using (var context = new MZDNETWORKContext())
                {
                    // Mevcut cache entry'yi bul veya yeni oluştur
                    var cacheEntry = context.PermissionCaches.FirstOrDefault(pc => pc.UserId == userId);
                    
                    var permissionsJson = JsonConvert.SerializeObject(permissions);

                    if (cacheEntry == null)
                    {
                        cacheEntry = new PermissionCache
                        {
                            UserId = userId,
                            PermissionsJson = permissionsJson,
                            ExpiresAt = DateTime.Now.AddMinutes(CACHE_DURATION_MINUTES),
                            CachedAt = DateTime.Now,
                            HitCount = 0,
                            IsActive = true,
                            Version = 1,
                            CreatedBy = "System",
                            CacheReason = "InitialCache"
                        };
                        
                        cacheEntry.CalculateSize();
                        cacheEntry.GenerateDataHash();
                        context.PermissionCaches.Add(cacheEntry);
                    }
                    else
                    {
                        cacheEntry.PermissionsJson = permissionsJson;
                        cacheEntry.ExpiresAt = DateTime.Now.AddMinutes(CACHE_DURATION_MINUTES);
                        cacheEntry.LastAccessedAt = DateTime.Now;
                        cacheEntry.IsActive = true;
                        cacheEntry.IncrementVersion();
                        cacheEntry.CalculateSize();
                        cacheEntry.GenerateDataHash();
                    }

                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SaveToDatabaseCache error: {ex.Message}");
            }
        }

        /// <summary>
        /// Database cache'den kullanıcı entry'sini sil
        /// </summary>
        private static void RemoveFromDatabaseCache(int userId)
        {
            try
            {
                using (var context = new MZDNETWORKContext())
                {
                    var cacheEntry = context.PermissionCaches.FirstOrDefault(pc => pc.UserId == userId);
                    if (cacheEntry != null)
                    {
                        cacheEntry.Invalidate();
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RemoveFromDatabaseCache error: {ex.Message}");
            }
        }

        /// <summary>
        /// Database cache'i tamamen temizle
        /// </summary>
        private static void ClearDatabaseCache()
        {
            try
            {
                using (var context = new MZDNETWORKContext())
                {
                    var allCacheEntries = context.PermissionCaches.Where(pc => pc.IsActive).ToList();
                    foreach (var entry in allCacheEntries)
                    {
                        entry.Invalidate();
                    }
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ClearDatabaseCache error: {ex.Message}");
            }
        }

        /// <summary>
        /// Cache için hash oluştur (integrity check)
        /// </summary>
        private static string GenerateHash(string input)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
                return Convert.ToBase64String(hashBytes).Substring(0, 16); // İlk 16 karakter
            }
        }

        /// <summary>
        /// Cache istatistiklerini al
        /// </summary>
        public static object GetCacheStatistics()
        {
            try
            {
                using (var context = new MZDNETWORKContext())
                {
                    var validCaches = context.PermissionCaches.Where(pc => pc.IsActive && pc.ExpiresAt > DateTime.Now).ToList();
                    
                    return new
                    {
                        TotalValidCaches = validCaches.Count,
                        TotalSizeKB = validCaches.Sum(c => c.SizeInBytes) / 1024.0,
                        AverageHitCount = validCaches.Any() ? validCaches.Average(c => c.HitCount) : 0,
                        OldestCache = validCaches.Any() ? validCaches.Min(c => c.CachedAt) : (DateTime?)null,
                        NewestCache = validCaches.Any() ? validCaches.Max(c => c.CachedAt) : (DateTime?)null,
                        MostAccessedUser = validCaches.Any() ? validCaches.OrderByDescending(c => c.HitCount).First().UserId : (int?)null
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetCacheStatistics error: {ex.Message}");
                return new { Error = ex.Message };
            }
        }
    }
} 