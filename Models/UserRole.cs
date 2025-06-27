using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MZDNETWORK.Models
{
    public class UserRole
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public int RoleId { get; set; }
        
        [Display(Name = "Atanma Tarihi")]
        public DateTime AssignedDate { get; set; } = DateTime.Now;
        
        [Display(Name = "Atan Kullanıcı")]
        public int AssignedBy { get; set; }
        
        [Display(Name = "Aktif mi?")]
        public bool IsActive { get; set; } = true;
        
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        
        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; }
    }
} 