using System;
using System.Linq;
using MZDNETWORK.Data;

namespace MZDNETWORK.Helpers
{
    public class OvertimeServiceScheduler
    {
        private static readonly object _lock = new object();

        public static void ResetOvertimeData(object state)
        {
            try
            {
                Console.WriteLine($"🧹 Hangfire Job: Starting daily cleanup at {DateTime.Now}");
                
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
                        Console.WriteLine($"🧹 Hangfire Job: {oldOvertimeRecords.Count} eski mesai kaydı temizlendi.");
                    }
                    else
                    {
                        Console.WriteLine("🧹 Hangfire Job: Temizlenecek eski kayıt bulunamadı.");
                    }

                    // Bugün için aktif mesai kaydı sayısını logla
                    var todayCount = context.OvertimeServicePersonnels
                        .Count(osp => osp.ServiceDate == DateTime.Today && osp.IsActive);
                    
                    Console.WriteLine($"📊 Hangfire Job: Bugün için {todayCount} aktif mesai kaydı bulunuyor.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Hangfire Job: Error during cleanup: {ex.Message}");
                Console.WriteLine($"❌ Hangfire Job: Stack trace: {ex.StackTrace}");
                // Re-throw the exception to let Hangfire know the job failed
                throw;
            }
        }

        // Manual cleanup method for testing
        public static void ManualCleanup()
        {
            Console.WriteLine("🔧 Manual cleanup triggered via Hangfire.");
            ResetOvertimeData(null);
        }

        // Get next cleanup time for debugging - No longer relevant with Hangfire
        public static DateTime? GetNextCleanupTime()
        {
            // Hangfire manages this, so we can't easily predict the next run time here.
            // You can check the Hangfire dashboard for this information.
            return null;
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
                        NextCleanupTime = "Managed by Hangfire",
                        IsRunning = "Managed by Hangfire"
                    };
                }
            }
            catch (Exception ex)
            {
                return new
                {
                    Error = ex.Message,
                    NextCleanupTime = "Managed by Hangfire",
                    IsRunning = "Managed by Hangfire"
                };
            }
        }
    }
}
