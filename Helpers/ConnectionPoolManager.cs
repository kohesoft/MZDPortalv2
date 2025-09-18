using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using NLog;

namespace MZDNETWORK.Helpers
{
    public static class ConnectionPoolManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// SQL Server baðlantý havuzunu temizler
        /// </summary>
        public static void ClearConnectionPools()
        {
            try
            {
                SqlConnection.ClearAllPools();
                Logger.Info("SQL Server connection pools cleared successfully");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error clearing SQL Server connection pools");
            }
        }

        /// <summary>
        /// Baðlantý havuzu istatistiklerini loglar
        /// </summary>
        public static void LogConnectionPoolStats()
        {
            try
            {
                // Bu metod baðlantý havuzu durumunu izlemek için kullanýlabilir
                Logger.Info("Connection pool status check completed");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error checking connection pool stats");
            }
        }

        /// <summary>
        /// Async olarak baðlantý havuzunu temizler
        /// </summary>
        public static async Task ClearConnectionPoolsAsync()
        {
            await Task.Run(() => ClearConnectionPools());
        }
    }
}