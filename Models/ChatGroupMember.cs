using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MZDNETWORK.Models
{
    /// <summary>
    /// Chat grup üyeliði ve yetkisi
    /// </summary>
    public class ChatGroupMember
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ChatGroupId { get; set; }
        [ForeignKey("ChatGroupId")]
        public virtual ChatGroup ChatGroup { get; set; }

        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [Display(Name = "Yazma Yetkisi Var mý?")]
        public bool CanWrite { get; set; } = true;
    }
}
