using System;
using System.Data.SQLite;
using System.Web.Mvc;

namespace MZDNETWORK.Controllers
{
    // Serves /tile/{z}/{x}/{y}.png requests from an .mbtiles file stored under App_Data.
    // Requires System.Data.SQLite (install via NuGet: System.Data.SQLite.Core)
    public class TileController : Controller
    {
        // Path to your MBTiles file (change file name as needed)
        private static readonly string MbTilesPath = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/ankara.mbtiles");

        // GET: /tile/{z}/{x}/{y}.png
        public ActionResult Index(int z, int x, int y)
        {
            var data = GetTile(z, x, y);
            if (data == null)
            {
                return new HttpStatusCodeResult(404);
            }
            return File(data, "image/png");
        }

        private static byte[] GetTile(int z, int x, int y)
        {
            if (string.IsNullOrEmpty(MbTilesPath) || !System.IO.File.Exists(MbTilesPath))
                return null;

            long tmsY = ((1L << z) - 1L) - y; // TMS conversion
            // Query string reused for both attempts
            const string sql = "SELECT tile_data FROM tiles WHERE zoom_level = @z AND tile_column = @x AND tile_row = @y LIMIT 1";

            try
            {
                using (var conn = new SQLiteConnection($"Data Source={MbTilesPath};Version=3;Read Only=True;"))
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@z", z);
                        cmd.Parameters.AddWithValue("@x", x);
                        cmd.Parameters.AddWithValue("@y", tmsY);
                        using (var reader = cmd.ExecuteReader(System.Data.CommandBehavior.SingleRow))
                        {
                            if (reader.Read())
                                return (byte[])reader[0];
                        }
                    }
                    // fallback: try XYZ row directly (some generators store this way)
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@z", z);
                        cmd.Parameters.AddWithValue("@x", x);
                        cmd.Parameters.AddWithValue("@y", y);
                        using (var reader = cmd.ExecuteReader(System.Data.CommandBehavior.SingleRow))
                        {
                            if (reader.Read())
                                return (byte[])reader[0];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO: log ex
            }
            return null;
        }
    }
} 