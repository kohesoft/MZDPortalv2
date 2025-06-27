using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Diagnostics;
using System.Management;
using System.Threading.Tasks;
using MZDNETWORK.Models;
using MZDNETWORK.Data;
using System.IO;
using System.Runtime.InteropServices;
using System.Net;
using MZDNETWORK.Helpers;
using MZDNETWORK.Attributes;

namespace MZDNETWORK.Controllers
{
    [DynamicAuthorize(Permission = "SystemManagement.Performance")]
    public class PerformanceController : Controller
    {
        private static PerformanceCounter cpuCounter;
        private static PerformanceCounter availableMemoryCounter;
        // Aktif oturum sayısı SessionTracker üzerinden yönetiliyor
        private static PerformanceCounter requestsPerSecCounter;
        private static PerformanceCounter requestExecutionTimeCounter;
        private static TimeSpan _prevProcTotal = TimeSpan.Zero;
        private static DateTime _prevProcSampleTime = DateTime.UtcNow;

        // GET: Performance  
        public ActionResult Index()
        {
            return View();
        }

        // Performans verilerini döndürecek API endpoint'i  
        [HttpGet]
        public JsonResult GetPerformanceData()
        {
            try
            {
                var performanceData = new
                {
                    Cpu = GetCpuUsage(),
                    Memory = GetMemoryUsage(),
                    Disk = GetDiskUsage(),
                    ServerInfo = GetServerInfo(),
                    Sessions = new { Active = GetActiveSessionCount() },
                    Performance = GetPerformanceMetrics(),
                    Services = new
                    {
                        WebServer = true, // if this code runs, web server is up
                        Database = CheckDatabaseStatus(),
                        Cache = CheckCacheStatus()
                    },
                    Traffic = Helpers.HourlyRequestCounter.GetSnapshot()
                };

                return Json(performanceData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Hatanın loglanması (gerektiğinde logging mekanizması ekleyin)  
                // Örneğin: Logger.Error(ex);
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // CPU kullanım oranını döndürür  
        private float GetCpuUsage()
        {
            // First try using PerformanceCounter, fallback to WMI if it fails
            try
            {
                if (cpuCounter == null)
                {
                    try
                    {
                        if (PerformanceCounterCategory.Exists("Processor") && PerformanceCounterCategory.CounterExists("% Processor Time", "Processor"))
                        {
                            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                            cpuCounter.NextValue();
                            System.Threading.Thread.Sleep(1000);
                        }
                    }
                    catch
                    {
                        // Ignore here, will fallback below
                        cpuCounter = null;
                    }
                }

                if (cpuCounter != null)
                {
                    // take two samples; first often 0
                    var first = cpuCounter.NextValue();
                    System.Threading.Thread.Sleep(500);
                    var second = cpuCounter.NextValue();
                    var value = second > 0 ? second : first;
                    if (value < 1) // maybe still zero, try WMI below
                    {
                        throw new Exception("Counter returned 0");
                    }
                    return value;
                }
            }
            catch
            {
                // ignore and fallback to WMI
            }

            // Fallback: use WMI to query average CPU load percentage
            try
            {
                var searcher = new ManagementObjectSearcher("SELECT PercentProcessorTime FROM Win32_PerfFormattedData_PerfOS_Processor WHERE Name='_Total'");
                var loadValues = new List<int>();
                foreach (var obj in searcher.Get())
                {
                    if (obj["PercentProcessorTime"] != null)
                        loadValues.Add(Convert.ToInt32(obj["PercentProcessorTime"]));
                }
                if (loadValues.Count == 0)
                {
                    // fallback to LoadPercentage field
                    searcher = new ManagementObjectSearcher("SELECT LoadPercentage FROM Win32_Processor");
                    foreach (var obj in searcher.Get())
                    {
                        if (obj["LoadPercentage"] != null)
                            loadValues.Add(Convert.ToInt32(obj["LoadPercentage"]));
                    }
                }
                if (loadValues.Count > 0)
                {
                    return (float)loadValues.Average();
                }
            }
            catch
            {
                // ignore final failure
            }

            // If everything fails, return 0
            return GetCpuUsageFromProcess();
        }

        // Fallback: calculates CPU usage for current process and scales to total CPU percentage
        private float GetCpuUsageFromProcess()
        {
            try
            {
                var proc = Process.GetCurrentProcess();
                var now = DateTime.UtcNow;
                var totalProcTime = proc.TotalProcessorTime;

                var diffTime = (now - _prevProcSampleTime).TotalMilliseconds;
                var diffProc = (totalProcTime - _prevProcTotal).TotalMilliseconds;

                // update previous samples
                _prevProcSampleTime = now;
                _prevProcTotal = totalProcTime;

                if (diffTime <= 0) return 0f;

                var cpuUsage = diffProc / diffTime / Environment.ProcessorCount * 100.0;
                return (float)Math.Round(cpuUsage, 1);
            }
            catch
            {
                return 0f;
            }
        }

        // Bellek kullanım bilgilerini döndürür  
        private object GetMemoryUsage()
        {
            try
            {
                // Try using performance counter first
                if (availableMemoryCounter == null)
                {
                    if (PerformanceCounterCategory.Exists("Memory") && PerformanceCounterCategory.CounterExists("Available MBytes", "Memory"))
                    {
                        availableMemoryCounter = new PerformanceCounter("Memory", "Available MBytes");
                        availableMemoryCounter.NextValue();
                    }
                }

                if (availableMemoryCounter != null)
                {
                    var availableMb = availableMemoryCounter.NextValue();
                    var totalBytes = GetTotalPhysicalMemory();
                    var availableBytes = (ulong)(availableMb * 1024 * 1024);
                    var usedBytes = totalBytes - availableBytes;
                    return new { Total = totalBytes, Available = availableBytes, Used = usedBytes };
                }
            }
            catch
            {
                // Ignore and fallback to GlobalMemoryStatusEx
            }

            // Fallback using GlobalMemoryStatusEx
            var status = new MEMORYSTATUSEX();
            if (GlobalMemoryStatusEx(status))
            {
                var total = status.ullTotalPhys;
                var available = status.ullAvailPhys;
                var used = total - available;
                return new { Total = total, Available = available, Used = used };
            }

            // last resort
            return new { Total = 0UL, Available = 0UL, Used = 0UL };
        }

        // Toplam fiziksel belleği döndürür  
        private ulong GetTotalPhysicalMemory()
        {
            var memoryStatus = new MEMORYSTATUSEX();
            if (GlobalMemoryStatusEx(memoryStatus))
            {
                return memoryStatus.ullTotalPhys;
            }
            throw new InvalidOperationException("Toplam fiziksel bellek alınamadı.");
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class MEMORYSTATUSEX
        {
            public uint dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

        // Disk kullanım bilgilerini döndürür  
        private object GetDiskUsage()
        {
            try
            {
                var drive = new DriveInfo("C"); // C sürücüsü, değiştirilebilir  

                return new
                {
                    Total = drive.TotalSize,
                    Free = drive.AvailableFreeSpace,
                    Used = drive.TotalSize - drive.AvailableFreeSpace
                };
            }
            catch (Exception ex)
            {
                // Hatanın loglanması
                // Örneğin: Logger.Error(ex);
                throw new InvalidOperationException("Disk kullanım verisi alınamadı.", ex);
            }
        }

        // Sunucu bilgilerini döndürür  
        private object GetServerInfo()
        {
            // Capture each field separately to avoid total failure
            object uptime;
            string cpu;
            string ip;

            try { uptime = GetSystemUptime(); }
            catch { uptime = new { Days = 0, Hours = 0, Minutes = 0 }; }

            try { cpu = GetCpuModel(); }
            catch { cpu = "Unknown"; }

            try { ip = GetLocalIPAddress(); }
            catch { ip = "N/A"; }

            return new
            {
                Hostname = Environment.MachineName,
                Platform = Environment.OSVersion.Platform.ToString(),
                Version = Environment.OSVersion.VersionString,
                Uptime = uptime,
                CpuModel = cpu,
                IpAddress = ip
            };
        }

        // Sistem çalışma süresini döndürür  
        private TimeSpan GetSystemUptime()
        {
            try
            {
                using (var uptime = new PerformanceCounter("System", "System Up Time"))
                {
                    uptime.NextValue();
                    System.Threading.Thread.Sleep(1000);
                    var seconds = uptime.NextValue();
                    if (seconds > 0)
                        return TimeSpan.FromSeconds(seconds);
                }
            }
            catch { /* ignore */ }

            // Fallback using Environment.TickCount64
            try
            {
                var millis = Environment.TickCount;
                return TimeSpan.FromMilliseconds(millis);
            }
            catch { return TimeSpan.Zero; }
        }

        // CPU modelini döndürür  
        private string GetCpuModel()
        {
            try
            {
                var searcher = new ManagementObjectSearcher("select Name from Win32_Processor");
                foreach (var item in searcher.Get())
                {
                    return item["Name"]?.ToString() ?? "Unknown";
                }
                return "Unknown";
            }
            catch (Exception ex)
            {
                // Hatanın loglanması
                // Örneğin: Logger.Error(ex);
                return "Unknown";
            }
        }

        // Yerel IP adresini döndürür  
        private string GetLocalIPAddress()
        {
            try
            {
                var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
                return "N/A";
            }
            catch (Exception ex)
            {
                // Hatanın loglanması
                // Örneğin: Logger.Error(ex);
                return "N/A";
            }
        }

        // Aktif oturum sayısını döndürür  
        private int GetActiveSessionCount()
        {
            return SessionTracker.Current; // SessionTracker kullan
        }

        // Performans metriklerini döndürür  
        private object GetPerformanceMetrics()
        {
            return new
            {
                LoadTime = GetAverageLoadTime(),
                RequestsPerSecond = GetRequestsPerSecond()
            };
        }

        // Ortalama sayfa yükleme süresini döndürür  
        private float GetAverageLoadTime()
        {
            try
            {
                if (requestExecutionTimeCounter == null)
                {
                    // "ASP.NET Applications" category contains one instance per application in the form of "__Total__" or the application virtual path (e.g. "/LM/W3SVC/1/ROOT/YourApp").
                    // We will fall back to "__Total__" if a specific instance is not found.
                    string instanceName = GetAspNetInstanceName();
                    if (!PerformanceCounterCategory.CounterExists("Request Execution Time", "ASP.NET Applications"))
                    {
                        throw new InvalidOperationException("'Request Execution Time' performance counter bulunamadı.");
                    }
                    requestExecutionTimeCounter = new PerformanceCounter("ASP.NET Applications", "Request Execution Time", instanceName, readOnly: true);
                    requestExecutionTimeCounter.NextValue(); // ilk okumayı başlat
                }

                // Request Execution Time counter already returns the average time in milliseconds for last requests
                return requestExecutionTimeCounter.NextValue();
            }
            catch (Exception ex)
            {
                // Hata logla ve varsayılan değer döndür
                return 0f;
            }
        }

        // Saniyede istek sayısını döndürür  
        private float GetRequestsPerSecond()
        {
            try
            {
                if (requestsPerSecCounter == null)
                {
                    string instanceName = GetAspNetInstanceName();
                    if (!PerformanceCounterCategory.CounterExists("Requests/Sec", "ASP.NET Applications"))
                    {
                        throw new InvalidOperationException("'Requests/Sec' performance counter bulunamadı.");
                    }
                    requestsPerSecCounter = new PerformanceCounter("ASP.NET Applications", "Requests/Sec", instanceName, readOnly: true);
                    requestsPerSecCounter.NextValue();
                }
                return requestsPerSecCounter.NextValue();
            }
            catch (Exception ex)
            {
                // Hata logla ve varsayılan değer döndür
                return 0f;
            }
        }

        // ASP.NET Applications kategorisindeki doğru instance adını elde et
        private string GetAspNetInstanceName()
        {
            try
            {
                var category = new PerformanceCounterCategory("ASP.NET Applications");
                string[] instances = category.GetInstanceNames();
                string appPath = HttpRuntime.AppDomainAppVirtualPath ?? "__Total__";

                // Karşılık gelen instance var mı kontrol et
                string match = instances.FirstOrDefault(i => i.Equals(appPath, StringComparison.OrdinalIgnoreCase));
                return match ?? "__Total__";
            }
            catch
            {
                return "__Total__";
            }
        }

        private bool CheckDatabaseStatus()
        {
            try
            {
                using (var context = new MZDNETWORKContext())
                {
                    return context.Database.Exists();
                }
            }
            catch
            {
                return false;
            }
        }

        private bool CheckCacheStatus()
        {
            try
            {
                var cache = System.Web.HttpRuntime.Cache;
                if (cache == null) return false;

                const string key = "__healthcheck__";
                cache[key] = 1;
                return cache[key] != null && (int)cache[key] == 1;
            }
            catch
            {
                return false;
            }
        }
    }
}
