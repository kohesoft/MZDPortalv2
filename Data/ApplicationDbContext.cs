using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using MZDNETWORK.Models;

namespace MZDNETWORK.Data
{
    public class MZDNETWORKContext : DbContext
    {
        public MZDNETWORKContext() : base("MZDNETWORKContext")
        {
            Database.SetInitializer<MZDNETWORKContext>(new CreateDatabaseIfNotExists<MZDNETWORKContext>());
        }
        public DbSet<User> Users { get; set; }
        public DbSet<UserInfo> UserInfos { get; set; }
        public DbSet<DilekOneri> DilekOneriler { get; set; }
        public DbSet<Gonderi> Gonderiler { get; set; }
        public DbSet<Dokumantasyon> Dokumantasyons { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<TodoItem> TodoItems { get; set; }
        public DbSet<Survey> Surveys { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<BeyazTahtaEntry> BeyazTahtaEntries { get; set; }
        public DbSet<TvHeader> TvHeaders { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<DailyMood> DailyMoods { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        
        // Yeni multiple roles için
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        
        // Dinamik yetki sistemi için
        public DbSet<PermissionNode> PermissionNodes { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<PermissionCache> PermissionCaches { get; set; }
        
        // Yeni model'lar
        public DbSet<SuggestionComplaint> SuggestionComplaints { get; set; }
        public DbSet<MeetingRoomReservation> MeetingRoomReservations { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<ChatGroup> ChatGroups { get; set; }
        public DbSet<VisitorEntry> VisitorEntries { get; set; }
        public DbSet<VisitorEntryHeader> VisitorEntryHeaders { get; set; }
        public DbSet<LateArrivalReport> LateArrivalReports { get; set; } // İşe Geç Gelme Tutanakları
        public DbSet<LateArrivalReportHeader> LateArrivalReportHeaders { get; set; } // Üst bilgi
        public DbSet<PasswordResetRequest> PasswordResetRequests { get; set; }
        public DbSet<ChatGroupMember> ChatGroupMembers { get; set; }

        // Servis Personel Yönetimi için yeni DbSet
        public DbSet<ServicePersonnel> ServicePersonnels { get; set; }
        
        // Dinamik Servis Yönetimi için yeni DbSet
        public DbSet<ServiceConfiguration> ServiceConfigurations { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // PermissionNode self-referencing relationship
            modelBuilder.Entity<PermissionNode>()
                .HasOptional(p => p.Parent)
                .WithMany(p => p.Children)
                .HasForeignKey(p => p.ParentId);

            // RolePermission relationships
            modelBuilder.Entity<RolePermission>()
                .HasRequired(rp => rp.Role)
                .WithMany()
                .HasForeignKey(rp => rp.RoleId);

            modelBuilder.Entity<RolePermission>()
                .HasRequired(rp => rp.PermissionNode)
                .WithMany()
                .HasForeignKey(rp => rp.PermissionNodeId);

            // PermissionCache relationship
            modelBuilder.Entity<PermissionCache>()
                .HasRequired(pc => pc.User)
                .WithMany()
                .HasForeignKey(pc => pc.UserId);

            // UserRole relationships
            modelBuilder.Entity<UserRole>()
                .HasRequired(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasRequired(ur => ur.Role)
                .WithMany()
                .HasForeignKey(ur => ur.RoleId);

            // AnswerOption relationship - prevent cascade conflicts
            modelBuilder.Entity<AnswerOption>()
                .HasRequired(ao => ao.Answer)
                .WithMany(a => a.Options)
                .HasForeignKey(ao => ao.AnswerId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<AnswerOption>()
                .HasRequired(ao => ao.Question)
                .WithMany()
                .HasForeignKey(ao => ao.QuestionID)
                .WillCascadeOnDelete(false);

            // BeyazTahtaEntry User relationship
            modelBuilder.Entity<BeyazTahtaEntry>()
                .HasOptional(bte => bte.User)
                .WithMany()
                .HasForeignKey(bte => bte.UserId)
                .WillCascadeOnDelete(false);

            // MeetingRoomReservation User relationship  
            modelBuilder.Entity<MeetingRoomReservation>()
                .HasRequired(mrr => mrr.User)
                .WithMany()
                .HasForeignKey(mrr => mrr.UserId)
                .WillCascadeOnDelete(false);

            // Notification UserId string to User relationship
            modelBuilder.Entity<Notification>()
                .Ignore(n => n.User);

            // SuggestionComplaint User relationship
            modelBuilder.Entity<SuggestionComplaint>()
                .HasRequired(sc => sc.User)
                .WithMany()
                .HasForeignKey(sc => sc.UserId)
                .WillCascadeOnDelete(false);

            // LeaveRequest User relationship - using Entity Framework conventions
            // The User navigation property will be automatically mapped

            // ChatMessage relationships
            modelBuilder.Entity<ChatMessage>()
                .HasRequired(cm => cm.ChatGroup)
                .WithMany(g => g.Messages)
                .HasForeignKey(cm => cm.ChatGroupId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ChatMessage>()
                .HasRequired(cm => cm.User)
                .WithMany()
                .HasForeignKey(cm => cm.UserId)
                .WillCascadeOnDelete(false);

            // ChatGroup relationships
            modelBuilder.Entity<ChatGroup>()
                .HasRequired(cg => cg.Creator)
                .WithMany()
                .HasForeignKey(cg => cg.CreatedBy)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ChatGroup>()
                .HasMany(g => g.Managers)
                .WithMany();

            modelBuilder.Entity<ChatGroup>()
                .HasMany(g => g.Members)
                .WithMany();

            modelBuilder.Entity<ChatGroup>()
                .HasMany(g => g.AllowedRoles)
                .WithMany();

            modelBuilder.Entity<ChatGroup>()
                .HasMany(g => g.Messages)
                .WithOptional();

            // Chat relationships
            modelBuilder.Entity<Chat>()
                .HasRequired(c => c.Creator)
                .WithMany()
                .HasForeignKey(c => c.CreatedBy)
                .WillCascadeOnDelete(false);

            // ServicePersonnel configurations
            // Since ServiceConfiguration is marked as [NotMapped], we only need to configure basic properties
            // No relationship configuration needed since we handle it manually in the controller
            

            // ServiceConfiguration unique index on ServiceCode
            modelBuilder.Entity<ServiceConfiguration>()
                .HasIndex(sc => sc.ServiceCode)
                .IsUnique();
        }

        /// <summary>
        /// Database seed işlemlerini yapar
        /// </summary>
        public static void SeedDatabase()
        {
            using (var context = new MZDNETWORKContext())
            {
                try
                {
                    Console.WriteLine("🌱 MZD Portal Database Seeding başlatılıyor...");

                    // 1. Permission ağacını oluştur
                    PermissionSeeder.SeedPermissions(context);

                    // 2. Temel admin rolü oluştur
                    PermissionSeeder.CreateDefaultAdminRole(context);

                    // 3. Admin kullanıcısı oluştur
                    PermissionSeeder.CreateDefaultAdminUser(context);

                    Console.WriteLine("✅ Database seeding tamamlandı!");
                    Console.WriteLine("🎯 Giriş yapmak için:");
                    Console.WriteLine("   👤 Kullanıcı: admin");
                    Console.WriteLine("   🔐 Şifre: admin123");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Database seeding hatası: {ex.Message}");
                    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                    throw;
                }
            }
        }
    }
}
