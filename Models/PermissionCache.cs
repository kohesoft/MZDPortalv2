using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MZDNETWORK.Models
{
    /// <summary>
    /// Kullanıcı yetki cache modeli - Performans optimizasyonu için
    /// </summary>
    public class PermissionCache
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        /// <summary>
        /// Serialize edilmiş yetki verileri (JSON format)
        /// </summary>
        [Required]
        [Column(TypeName = "text")]
        public string PermissionsJson { get; set; }

        /// <summary>
        /// Cache oluşturulma zamanı
        /// </summary>
        public DateTime CachedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Cache son kullanılma zamanı
        /// </summary>
        public DateTime LastAccessedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Cache geçerlilik süresi sonu
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Cache aktif mi
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Cache versiyonu (yetki güncellendiğinde increment edilir)
        /// </summary>
        public int Version { get; set; } = 1;

        /// <summary>
        /// Cache boyutu (byte cinsinden)
        /// </summary>
        public int SizeInBytes { get; set; }

        /// <summary>
        /// Cache hit sayısı (ne kadar kullanıldığı)
        /// </summary>
        public int HitCount { get; set; } = 0;

        /// <summary>
        /// Cache'i oluşturan kullanıcı/sistem
        /// </summary>
        [StringLength(100)]
        public string CreatedBy { get; set; } = "System";

        /// <summary>
        /// Cache oluşturma sebebi
        /// </summary>
        [StringLength(200)]
        public string CacheReason { get; set; }

        /// <summary>
        /// Hash değeri (data integrity için)
        /// </summary>
        [StringLength(64)]
        public string DataHash { get; set; }

        // Computed Properties

        /// <summary>
        /// Cache geçerli mi kontrol et
        /// </summary>
        [NotMapped]
        public bool IsValid => IsActive && DateTime.Now < ExpiresAt;

        /// <summary>
        /// Cache'in yaşı (dakika cinsinden)
        /// </summary>
        [NotMapped]
        public double AgeInMinutes => (DateTime.Now - CachedAt).TotalMinutes;

        /// <summary>
        /// Cache'in ne kadar süre daha geçerli olacağı (dakika cinsinden)
        /// </summary>
        [NotMapped]
        public double RemainingValidityMinutes => Math.Max(0, (ExpiresAt - DateTime.Now).TotalMinutes);

        /// <summary>
        /// Cache performans skoru (hit count vs age)
        /// </summary>
        [NotMapped]
        public double PerformanceScore => HitCount / Math.Max(1, AgeInMinutes);

        /// <summary>
        /// Cache'in sık kullanılıp kullanılmadığı
        /// </summary>
        [NotMapped]
        public bool IsHotCache => HitCount > 10 && AgeInMinutes < 60;

        /// <summary>
        /// Cache'in temizlenmeye uygun olup olmadığı
        /// </summary>
        [NotMapped]
        public bool ShouldBeCleaned => !IsValid || (HitCount == 0 && AgeInMinutes > 1440); // 24 saat

        // Methods

        /// <summary>
        /// Cache'i kullanıldı olarak işaretle
        /// </summary>
        public void MarkAsAccessed()
        {
            LastAccessedAt = DateTime.Now;
            HitCount++;
        }

        /// <summary>
        /// Cache'i geçersiz kıl
        /// </summary>
        public void Invalidate()
        {
            IsActive = false;
            ExpiresAt = DateTime.Now.AddMinutes(-1);
        }

        /// <summary>
        /// Cache'i yenile (yeni expiry time)
        /// </summary>
        public void Refresh(int durationMinutes = 30)
        {
            ExpiresAt = DateTime.Now.AddMinutes(durationMinutes);
            LastAccessedAt = DateTime.Now;
            IsActive = true;
        }

        /// <summary>
        /// Cache versiyonunu artır
        /// </summary>
        public void IncrementVersion()
        {
            Version++;
            CachedAt = DateTime.Now;
            HitCount = 0; // Reset hit count for new version
        }

        /// <summary>
        /// Data hash oluştur
        /// </summary>
        public void GenerateDataHash()
        {
            if (!string.IsNullOrEmpty(PermissionsJson))
            {
                using (var sha256 = System.Security.Cryptography.SHA256.Create())
                {
                    var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(PermissionsJson));
                    DataHash = Convert.ToBase64String(hashBytes);
                }
            }
        }

        /// <summary>
        /// Data integrity kontrol et
        /// </summary>
        public bool VerifyDataIntegrity()
        {
            if (string.IsNullOrEmpty(DataHash) || string.IsNullOrEmpty(PermissionsJson))
                return false;

            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(PermissionsJson));
                var currentHash = Convert.ToBase64String(hashBytes);
                return currentHash == DataHash;
            }
        }

        /// <summary>
        /// Cache boyutunu hesapla ve kaydet
        /// </summary>
        public void CalculateSize()
        {
            SizeInBytes = string.IsNullOrEmpty(PermissionsJson) ? 0 : System.Text.Encoding.UTF8.GetByteCount(PermissionsJson);
        }

        // Static Methods

        /// <summary>
        /// Yeni cache instance oluştur
        /// </summary>
        public static PermissionCache CreateNew(int userId, string permissionsJson, int durationMinutes = 30, string reason = "UserPermissionUpdate")
        {
            var cache = new PermissionCache
            {
                UserId = userId,
                PermissionsJson = permissionsJson,
                ExpiresAt = DateTime.Now.AddMinutes(durationMinutes),
                CacheReason = reason,
                CreatedBy = "System"
            };
            
            cache.CalculateSize();
            cache.GenerateDataHash();
            
            return cache;
        }
    }
} 