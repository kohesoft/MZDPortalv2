using System.Data.Entity;
using System.Threading.Tasks;

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
    }
}
