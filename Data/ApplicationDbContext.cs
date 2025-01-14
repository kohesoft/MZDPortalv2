using System.Data.Entity;

namespace MZDNETWORK.Models
{
    public class MZDNETWORKContext : DbContext
    {
        public MZDNETWORKContext() : base("name=MZDNETWORKContext")
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<UserInfo> UserInfos { get; set; }
        public DbSet<DilekOneri> DilekOneriler { get; set; }
        public DbSet<Gonderi> Gonderiler { get; set; }
        public DbSet<Dokumantasyon> Dokumantasyons { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }

    }
}
