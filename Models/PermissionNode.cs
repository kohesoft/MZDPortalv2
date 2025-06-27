using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MZDNETWORK.Models
{
    /// <summary>
    /// Dinamik yetki sistemi için hiyerarşik yetki düğümü modeli
    /// </summary>
    public class PermissionNode
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } // Görünen isim (örn: "Kullanıcı Yönetimi")

        [Required]
        [StringLength(200)]
        public string Path { get; set; } // Yetki yolu (örn: "UserManagement.Create")

        [StringLength(500)]
        public string Description { get; set; } // Açıklama

        // Hiyerarşik yapı için
        public int? ParentId { get; set; }
        
        [ForeignKey("ParentId")]
        public virtual PermissionNode Parent { get; set; }
        
        public virtual ICollection<PermissionNode> Children { get; set; }

        // Node tipi (Module, Controller, Action, Page)
        [Required]
        [StringLength(50)]
        public string Type { get; set; } // "Module", "Controller", "Action", "Page"

        // UI için
        [StringLength(50)]
        public string Icon { get; set; } // Bootstrap/BoxIcons class

        public int SortOrder { get; set; } // Sıralama

        // Aktif/Pasif durumu
        public bool IsActive { get; set; } = true;

        // CRUD yetki türleri
        public bool HasViewPermission { get; set; } = true;
        public bool HasCreatePermission { get; set; } = false;
        public bool HasEditPermission { get; set; } = false;
        public bool HasDeletePermission { get; set; } = false;

        // İlişkiler
        public virtual ICollection<RolePermission> RolePermissions { get; set; }

        // Constructor
        public PermissionNode()
        {
            Children = new HashSet<PermissionNode>();
            RolePermissions = new HashSet<RolePermission>();
        }

        /// <summary>
        /// Full path with parent hierarchy
        /// </summary>
        [NotMapped]
        public string FullPath
        {
            get
            {
                if (Parent == null)
                    return Path;
                return $"{Parent.FullPath}.{Path}";
            }
        }

        /// <summary>
        /// Derinlik seviyesi (root = 0)
        /// </summary>
        [NotMapped]
        public int Level
        {
            get
            {
                int level = 0;
                var current = Parent;
                while (current != null)
                {
                    level++;
                    current = current.Parent;
                }
                return level;
            }
        }

        /// <summary>
        /// Root node mu kontrol et
        /// </summary>
        [NotMapped]
        public bool IsRoot => ParentId == null;

        /// <summary>
        /// Leaf node mu kontrol et (alt düğümü yok)
        /// </summary>
        [NotMapped]
        public bool IsLeaf => Children == null || Children.Count == 0;
    }
} 