using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using MZDNETWORK.Models;
using OfficeOpenXml;

namespace MZDNETWORK.Controllers
{
    [AllowAnonymous]
    public class ContactController : Controller
    {
        private MZDNETWORKContext db = new MZDNETWORKContext();

        public ActionResult Index()
        {
            var users = db.Users.ToList();
            return View(users);
        }


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
                for (int i = 0; i < data.Count; i++)
                {
                    for (int j = 0; j < data[i].Count; j++)
                    {
                        worksheet.Cells[i + 1, j + 1].Value = data[i][j];
                    }
                }

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "KullaniciListesi.xlsx");
            }
        }

    }
}

