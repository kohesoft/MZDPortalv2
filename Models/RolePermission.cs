using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MZDNETWORK.Models
{
    /// <summary>
    /// Rol ve yetki düğümü arasındaki ilişki modeli
    /// Her rol için her yetki düğümüne granular (CRUD) erişim kontrolü
    /// </summary>
    public class RolePermission
    {
        [Key]
        public int Id { get; set; }

        // Role ilişkisi
        [Required]
        public int RoleId { get; set; }
        
        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; }

        // PermissionNode ilişkisi
        [Required]
        public int PermissionNodeId { get; set; }
        
        [ForeignKey("PermissionNodeId")]
        public virtual PermissionNode PermissionNode { get; set; }

        // Granular CRUD yetkileri
        public bool CanView { get; set; } = false;       // Görüntüleme yetkisi
        public bool CanCreate { get; set; } = false;     // Oluşturma yetkisi
        public bool CanEdit { get; set; } = false;       // Düzenleme yetkisi
        public bool CanDelete { get; set; } = false;     // Silme yetkisi
        public bool CanManage { get; set; } = false;     // Yönetim yetkisi (tüm işlemler)

        // Ek yetki kontrolleri
        public bool CanApprove { get; set; } = false;    // Onaylama yetkisi (izin talepleri vb.)
        public bool CanReject { get; set; } = false;     // Reddetme yetkisi
        public bool CanExport { get; set; } = false;     // Dışa aktarma yetkisi
        public bool CanImport { get; set; } = false;     // İçe aktarma yetkisi

        // Meta data
        public bool IsActive { get; set; } = true;
        public System.DateTime CreatedAt { get; set; } = System.DateTime.Now;
        public System.DateTime? UpdatedAt { get; set; }

        [StringLength(100)]
        public string CreatedBy { get; set; }
        
        [StringLength(100)]
        public string UpdatedBy { get; set; }

        [StringLength(500)]
        public string Notes { get; set; } // Yetki verme sebebi, notlar

        /// <summary>
        /// Herhangi bir yetkiye sahip mi kontrol et
        /// </summary>
        [NotMapped]
        public bool HasAnyPermission => CanView || CanCreate || CanEdit || CanDelete || CanManage;

        /// <summary>
        /// Tam yetki kontrolü (tüm CRUD işlemleri)
        /// </summary>
        [NotMapped]
        public bool HasFullPermission => CanView && CanCreate && CanEdit && CanDelete;

        /// <summary>
        /// Sadece okuma yetkisi mi
        /// </summary>
        [NotMapped]
        public bool IsReadOnly => CanView && !CanCreate && !CanEdit && !CanDelete;

        /// <summary>
        /// Yönetici yetkisi mi (Manage veya full CRUD)
        /// </summary>
        [NotMapped]
        public bool IsAdminLevel => CanManage || HasFullPermission;

        /// <summary>
        /// Yetki seviyesini döndür (0: Yok, 1: Okuma, 2: Yazma, 3: Tam, 4: Yönetici)
        /// </summary>
        [NotMapped]
        public int PermissionLevel
        {
            get
            {
                if (CanManage) return 4;
                if (HasFullPermission) return 3;
                if (CanCreate || CanEdit || CanDelete) return 2;
                if (CanView) return 1;
                return 0;
            }
        }

        /// <summary>
        /// Yetki seviyesi açıklaması
        /// </summary>
        [NotMapped]
        public string PermissionLevelDescription
        {
            get
            {
                switch (PermissionLevel)
                {
                    case 4: return "Yönetici (Tam Yetki)";
                    case 3: return "Tam Yetki (CRUD)";
                    case 2: return "Yazma Yetkisi";
                    case 1: return "Okuma Yetkisi";
                    case 0: return "Yetki Yok";
                    default: return "Bilinmiyor";
                }
            }
        }

        /// <summary>
        /// Quick permission assignment methods
        /// </summary>
        public void SetReadOnlyPermission()
        {
            CanView = true;
            CanCreate = CanEdit = CanDelete = CanManage = false;
        }

        public void SetFullPermission()
        {
            CanView = CanCreate = CanEdit = CanDelete = true;
            CanManage = false;
        }

        public void SetManagePermission()
        {
            CanView = CanCreate = CanEdit = CanDelete = CanManage = true;
            CanApprove = CanReject = CanExport = CanImport = true;
        }

        public void ClearAllPermissions()
        {
            CanView = CanCreate = CanEdit = CanDelete = CanManage = false;
            CanApprove = CanReject = CanExport = CanImport = false;
        }
    }
} 