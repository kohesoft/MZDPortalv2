using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MZDNETWORK.Models
{
    /// <summary>
    /// Represents a user initiated password reset request that must be approved by an administrator.
    /// Once approved, the user can log-in without a password a single time and will be forced to
    /// change their password immediately after logging in.
    /// </summary>
    public class PasswordResetRequest
    {
        [Key]
        public int Id { get; set; }

        [Index]
        public int UserId { get; set; }

        [ForeignKey("UserId")] public virtual User User { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.Now;

        public bool Approved { get; set; }

        public DateTime? ApprovedAt { get; set; }

        /// <summary>
        /// Admin user that approved the request (nullable until approved).
        /// </summary>
        public int? ApprovedByUserId { get; set; }

        /// <summary>
        /// True once the user has logged in using this request.
        /// </summary>
        public bool Used { get; set; }
    }
} 