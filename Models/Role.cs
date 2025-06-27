using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MZDNETWORK.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        [StringLength(200)]
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; }
    }

}