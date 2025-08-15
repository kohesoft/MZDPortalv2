using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MZDNETWORK.Data;

namespace MZDNETWORK.Helpers
{
    public class OvertimeServiceScheduler
    {
        private static Timer _timer;
        private static readonly object _lock = new object();

        public static void Start()
        {
            lock (_lock)
            {
                if (_timer != null)
                    return;

                // Calculate time until next midnight
                var now = DateTime.Now;
                var nextMidnight = now.Date.AddDays(1); // Tomorrow at 00:00
                var timeUntilMidnight = nextMidnight - now;

                Console.WriteLine($"⏰ OvertimeServiceScheduler: Starting scheduler. Next cleanup at {nextMidnight}");

                // Set timer to trigger at midnight, then every 24 hours
                _timer = new Timer(ResetOvertimeData, null, timeUntilMidnight, TimeSpan.FromDays(1));
            }
        }

        public static void Stop()
        {
            lock (_lock)
            {
                if (_timer != null)
                {
                    _timer.Dispose();
                    _timer = null;
                    Console.WriteLine("⏰ OvertimeServiceScheduler: Scheduler stopped.");
                }
            }
        }

        private static void ResetOvertimeData(object state)
        {
            try
            {
                Console.WriteLine($"🧹 OvertimeServiceScheduler: Starting daily cleanup at {DateTime.Now}");
                
                using (var context = new MZDNETWORKContext())
                {
                    var yesterday = DateTime.Today.AddDays(-1);
                    
                    // Dünden önceki mesai kayıtlarını pasif yap
                    var oldOvertimeRecords = context.OvertimeServicePersonnels
                        .Where(osp => osp.ServiceDate < yesterday && osp.IsActive)
                        .ToList();

                    if (oldOvertimeRecords.Any())
                    {
                        foreach (var record in oldOvertimeRecords)
                        {
                            record.IsActive = false;
                            record.UpdatedDate = DateTime.Now;
                        }

                        context.SaveChanges();
                        Console.WriteLine($"🧹 OvertimeServiceScheduler: {oldOvertimeRecords.Count} eski mesai kaydı temizlendi.");
                    }
                    else
                    {
                        Console.WriteLine("🧹 OvertimeServiceScheduler: Temizlenecek eski kayıt bulunamadı.");
                    }

                    // Bugün için aktif mesai kaydı sayısını logla
                    var todayCount = context.OvertimeServicePersonnels
                        .Count(osp => osp.ServiceDate == DateTime.Today && osp.IsActive);
                    
                    Console.WriteLine($"📊 OvertimeServiceScheduler: Bugün için {todayCount} aktif mesai kaydı bulunuyor.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ OvertimeServiceScheduler: Error during cleanup: {ex.Message}");
                Console.WriteLine($"❌ OvertimeServiceScheduler: Stack trace: {ex.StackTrace}");
            }
        }

        // Manual cleanup method for testing
        public static void ManualCleanup()
        {
            Console.WriteLine("🔧 OvertimeServiceScheduler: Manual cleanup triggered.");
            ResetOvertimeData(null);
        }

        // Get next cleanup time for debugging
        public static DateTime GetNextCleanupTime()
        {
            var now = DateTime.Now;
            var nextMidnight = now.Date.AddDays(1);
            return nextMidnight;
        }

        // Get current stats
        public static object GetStats()
        {
            try
            {
                using (var context = new MZDNETWORKContext())
                {
                    var today = DateTime.Today;
                    var yesterday = today.AddDays(-1);
                    
                    var todayActive = context.OvertimeServicePersonnels
                        .Count(osp => osp.ServiceDate == today && osp.IsActive);
                    
                    var yesterdayActive = context.OvertimeServicePersonnels
                        .Count(osp => osp.ServiceDate == yesterday && osp.IsActive);
                    
                    var totalInactive = context.OvertimeServicePersonnels
                        .Count(osp => !osp.IsActive);

                    return new
                    {
                        TodayActive = todayActive,
                        YesterdayActive = yesterdayActive,
                        TotalInactive = totalInactive,
                        NextCleanupTime = GetNextCleanupTime(),
                        IsRunning = _timer != null
                    };
                }
            }
            catch (Exception ex)
            {
                return new
                {
                    Error = ex.Message,
                    NextCleanupTime = GetNextCleanupTime(),
                    IsRunning = _timer != null
                };
            }
        }
    }
}
