using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MZDNETWORK.Models
{
    /// <summary>
    /// Dinamik yetki sistemi iÃ§in hiyerarÅŸik yetki dÃ¼ÄŸÃ¼mÃ¼ modeli
    /// </summary>
    public class PermissionNode
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } // GÃ¶rÃ¼nen isim (Ã¶rn: "KullanÄ±cÄ± YÃ¶netimi")

        [Required]
        [StringLength(200)]
        public string Path { get; set; } // Yetki yolu (Ã¶rn: "KullaniciYonetimi.Create")

        [StringLength(500)]
        public string Description { get; set; } // AÃ§Ä±klama

        // HiyerarÅŸik yapÄ± iÃ§in
        public int? ParentId { get; set; }
        
        [ForeignKey("ParentId")]
        public virtual PermissionNode Parent { get; set; }
        
        public virtual ICollection<PermissionNode> Children { get; set; }

        // Node tipi (Module, Controller, Action, Page)
        [Required]
        [StringLength(50)]
        public string Type { get; set; } // "Module", "Controller", "Action", "Page"

        // UI iÃ§in
        [StringLength(50)]
        public string Icon { get; set; } // Bootstrap/BoxIcons class

        public int SortOrder { get; set; } // SÄ±ralama

        // Aktif/Pasif durumu
        public bool IsActive { get; set; } = true;

        // CRUD yetki tÃ¼rleri
        public bool HasViewPermission { get; set; } = true;
        public bool HasCreatePermission { get; set; } = false;
        public bool HasEditPermission { get; set; } = false;
        public bool HasDeletePermission { get; set; } = false;

        // Ä°liÅŸkiler
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
        /// Leaf node mu kontrol et (alt dÃ¼ÄŸÃ¼mÃ¼ yok)
        /// </summary>
        [NotMapped]
        public bool IsLeaf => Children == null || Children.Count == 0;
    }
} 
