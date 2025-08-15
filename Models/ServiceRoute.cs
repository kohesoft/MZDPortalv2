using System;
using System.ComponentModel.DataAnnotations;

namespace MZDNETWORK.Models
{
    public class ServiceRoute
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; }
        
        [StringLength(500)]
        public string KmlPath { get; set; }
        
        [StringLength(1000)]
        public string Description { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public DateTime? UpdatedDate { get; set; }
    }
}