using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MZDNETWORK.Models
{
    public class AccessChangeTicketViewModel
    {
        [Required]
        [Display(Name = "Talep Tipi")]
        public AccessChangeRequestType RequestType { get; set; }

        [Required]
        [Display(Name = "Hedef Kullanıcı")]
        public int TargetUserId { get; set; }

        // Role değişikliği
        public List<int> RequestedRoleIds { get; set; } = new List<int>();

        // Yetki değişikliği (PermissionNode.Path listesi)
        public List<string> RequestedPermissionPaths { get; set; } = new List<string>();

        // Kullanıcı bilgisi değişikliği
        public AccessChangeUserInfoDto RequestedUserInfo { get; set; } = new AccessChangeUserInfoDto();

        [Required]
        [StringLength(1000)]
        [Display(Name = "Açıklama")]
        public string Description { get; set; }
    }
}
