using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MZDNETWORK.Models
{
    public class ServiceConfiguration
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string ServiceName { get; set; }
        
        [Required]
        [StringLength(200)]
        public string RouteName { get; set; }
        
        [StringLength(500)]
        public string Description { get; set; }
        
        [StringLength(100)]
        public string VehiclePlate { get; set; }
        
        [StringLength(100)]
        public string DriverName { get; set; }
        
        [StringLength(20)]
        public string DriverPhone { get; set; }
        
        public TimeSpan? DepartureTime { get; set; }
        
        public TimeSpan? ReturnTime { get; set; }
        
        public int Capacity { get; set; } = 50;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public DateTime? UpdatedDate { get; set; }
        
        public DateTime? DeletedDate { get; set; }
        
        public string DeletedBy { get; set; }
        
        public string DeletedReason { get; set; }
        
        // Navigation Propertiesa
        public virtual ICollection<ServicePersonnel> ServicePersonnels { get; set; }
        public virtual ICollection<OvertimeServicePersonnel> OvertimeServicePersonnels { get; set; }
        
        // Helper Properties
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
