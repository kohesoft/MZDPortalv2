using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MZDNETWORK.Models
{
    public enum AccessChangeRequestType
    {
        PermissionChange = 1,
        RoleChange = 2,
        UserInfoChange = 3
    }

    public enum AccessChangeStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        Applied = 3
    }

    public class AccessChangeTicket
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RequesterUserId { get; set; }

        [Required]
        public int TargetUserId { get; set; }

        [Required]
        public AccessChangeRequestType RequestType { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; }

        // Snapshots (JSON strings for simplicity)
        public string CurrentRolesJson { get; set; }
        public string RequestedRolesJson { get; set; }
        public string CurrentPermissionsJson { get; set; }
        public string RequestedPermissionsJson { get; set; }
        public string CurrentUserInfoJson { get; set; }
        public string RequestedUserInfoJson { get; set; }

        public AccessChangeStatus Status { get; set; } = AccessChangeStatus.Pending;
        public string AdminNote { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ResolvedAt { get; set; }

        [ForeignKey("RequesterUserId")]
        public virtual User Requester { get; set; }

        [ForeignKey("TargetUserId")]
        public virtual User TargetUser { get; set; }
    }

    // Lightweight DTOs for JSON payloads
    public class AccessChangeUserInfoDto
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Username { get; set; }
        public string InternalEmail { get; set; }
        public string ExternalEmail { get; set; }
        public string Department { get; set; }
        public string Title { get; set; }
        public string PhoneNumber { get; set; }
        public string Intercom { get; set; }
    }
}
