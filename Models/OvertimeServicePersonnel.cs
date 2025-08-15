using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MZDNETWORK.Models
{
    public class OvertimeServicePersonnel
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public int ServiceConfigurationId { get; set; }
        
        [Required]
        public DateTime ServiceDate { get; set; } = DateTime.Today;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public DateTime? UpdatedDate { get; set; }
        
        public DateTime? DeletedDate { get; set; }
        
        public string DeletedBy { get; set; }
        
        public string DeletedReason { get; set; }
        
        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        
        [ForeignKey("ServiceConfigurationId")]
        public virtual ServiceConfiguration ServiceConfiguration { get; set; }
        
        // Helper Properties
        [NotMapped]
        public string UserFullName => User?.FullName ?? "";
        
        [NotMapped]
        public string ServiceName => ServiceConfiguration?.ServiceName ?? "";
        
        [NotMapped]
        public string RouteName => ServiceConfiguration?.RouteName ?? "";
        
        [NotMapped]
        public bool IsDeleted => !IsActive && DeletedDate.HasValue;
        
        // Soft Delete Methods
        public void SoftDelete(string deletedBy = null, string reason = null)
        {
            IsActive = false;
            DeletedDate = DateTime.Now;
            DeletedBy = deletedBy;
            DeletedReason = reason;
            UpdatedDate = DateTime.Now;
        }
        
        public void Restore()
        {
            IsActive = true;
            DeletedDate = null;
            DeletedBy = null;
            DeletedReason = null;
            UpdatedDate = DateTime.Now;
        }
    }
}
