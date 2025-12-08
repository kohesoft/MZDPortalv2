using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using MZDNETWORK.Models;
using MZDNETWORK.Data;
using OfficeOpenXml;
using MZDNETWORK.Attributes;
using System.Data.Entity;

namespace MZDNETWORK.Controllers
{
    [AllowAnonymous]
    public class ContactController : Controller
    {
        private MZDNETWORKContext db = new MZDNETWORKContext();

        public ContactController()
        {
            // Cache problemlerini önlemek için
            db.Configuration.AutoDetectChangesEnabled = true;
            db.Configuration.ValidateOnSaveEnabled = true;
            db.Configuration.LazyLoadingEnabled = false;
        }

        public ActionResult Index()
        {
            // Fresh context ile temiz veri al - cache problemlerini önlemek için
            using (var freshContext = new MZDNETWORKContext())
            {
                var users = freshContext.Users
                    .AsNoTracking() // Read-only için tracking'i devre dışı bırak
                    .OrderBy(u => u.Department)
                    .ThenBy(u => u.Name)
                    .ThenBy(u => u.Surname)
                    .ToList();
                    
                return View(users);
            }
        }

        [DynamicAuthorize(Permission = "Operasyon.Iletisim", Action = "Export")]
        [HttpPost]
        public ActionResult ExportToExcel(List<List<string>> data)
        {
            if (data == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Data cannot be null");
            }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Set the license context
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("KullaniciListesi");
                
                // Header'ları ekle
                string[] headers = { "Sicil", "Adı", "Soyadı", "Departman ve Pozisyon", "Dahili", "Cep Tel", "İç mail", "Dış mail" };
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = headers[i];
                    worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                }
                
                // Veri satırlarını ekle
                for (int i = 0; i < data.Count; i++)
                {
                    for (int j = 0; j < data[i].Count && j < headers.Length; j++)
                    {
                        worksheet.Cells[i + 2, j + 1].Value = data[i][j]; // i + 2 çünkü header satırı var
                    }
                }

                // Auto-fit columns
                worksheet.Cells.AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "KullaniciListesi.xlsx");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}


