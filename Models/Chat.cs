using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MZDNETWORK.Models
{
    /// <summary>
    /// Chat odası modeli
    /// </summary>
    public class Chat
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Chat Adı")]
        public string Name { get; set; }

        [StringLength(500)]
        [Display(Name = "Açıklama")]
        public string Description { get; set; }

        [StringLength(50)]
        [Display(Name = "Chat Türü")]
        public string Type { get; set; } = "Public"; // Public, Private, Group

        [Display(Name = "Oluşturan Kullanıcı")]
        public int CreatedBy { get; set; }

        [ForeignKey("CreatedBy")]
        public virtual User Creator { get; set; }

        [Display(Name = "Aktif mi?")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Maksimum Üye Sayısı")]
        public int? MaxMembers { get; set; }

        [Display(Name = "Moderasyon Gerekli mi?")]
        public bool RequiresModeration { get; set; } = false;

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Güncellenme Tarihi")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "Son Mesaj Tarihi")]
        public DateTime? LastMessageAt { get; set; }

        [StringLength(200)]
        [Display(Name = "Son Mesaj")]
        public string LastMessage { get; set; }

        [Display(Name = "Toplam Mesaj Sayısı")]
        public int MessageCount { get; set; } = 0;

        [Display(Name = "Üye Sayısı")]
        public int MemberCount { get; set; } = 0;

        [StringLength(50)]
        [Display(Name = "Durum")]
        public string Status { get; set; } = "Active"; // Active, Archived, Suspended

        // Navigation Properties
        public virtual ICollection<ChatMessage> Messages { get; set; }

        public Chat()
        {
            Messages = new HashSet<ChatMessage>();
        }
    }
} 