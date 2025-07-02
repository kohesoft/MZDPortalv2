using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using MZDNETWORK.Data;
using MZDNETWORK.Models;
using MZDNETWORK.Attributes;
using OfficeOpenXml;

namespace MZDNETWORK.Controllers
{
    [DynamicAuthorize(Permission = "HumanResources.LateArrivalReport")] // Yeni yetki kategorisi
    public class LateArrivalReportController : Controller
    {
        private readonly MZDNETWORKContext _db = new MZDNETWORKContext();

        // GET: LateArrivalReport
        public ActionResult Index()
        {
            var reports = _db.LateArrivalReports.OrderByDescending(r => r.LateDate).ToList();
            return View(reports);
        }

        // GET: LateArrivalReport/Create
        [DynamicAuthorize(Permission = "HumanResources.LateArrivalReport", Action = "Create")]
        public ActionResult Create()
        {
            var model = new LateArrivalReport
            {
                ShiftStartTime = new TimeSpan(7, 30, 0),
                ArrivalTime = DateTime.Now.TimeOfDay,
                LateDate = DateTime.Today
            };

            var departments = _db.Users
                .Where(u => !string.IsNullOrEmpty(u.Department))
                .Select(u => u.Department)
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            ViewBag.Departments = new SelectList(departments);
            return View(model);
        }

        // POST: LateArrivalReport/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DynamicAuthorize(Permission = "HumanResources.LateArrivalReport", Action = "Create")]
        [ValidateInput(false)]
        public ActionResult Create(LateArrivalReport model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.CreatedAt = DateTime.Now;
            _db.LateArrivalReports.Add(model);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: LateArrivalReport/Details/5
        public ActionResult Details(int id)
        {
            var report = _db.LateArrivalReports.FirstOrDefault(r => r.Id == id);
            if (report == null) return HttpNotFound();
            return View(report);
        }

        // GET + POST: Header (Admin only)
        [DynamicAuthorize(Permission = "HumanResources.LateArrivalReport", Action = "Manage")]
        public ActionResult Header()
        {
            var header = _db.LateArrivalReportHeaders.FirstOrDefault() ?? new LateArrivalReportHeader {
                FirstPublishDate = DateTime.Today,
                RevisionDate = DateTime.Today
            };
            return View(header);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        [DynamicAuthorize(Permission = "HumanResources.LateArrivalReport", Action = "Manage")]
        public ActionResult Header(LateArrivalReportHeader model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.Id == 0)
            {
                _db.LateArrivalReportHeaders.Add(model);
            }
            else
            {
                _db.Entry(model).State = EntityState.Modified;
            }
            _db.SaveChanges();
            return RedirectToAction("Header");
        }

        // Excel export
        [DynamicAuthorize(Permission = "HumanResources.LateArrivalReport", Action = "Export")]
        public ActionResult ExportExcel()
        {
            var rows = _db.LateArrivalReports.OrderBy(r => r.LateDate).ToList();
            using (var pck = new ExcelPackage())
            {
                var ws = pck.Workbook.Worksheets.Add("LateArrival");
                string[] headerTitles = { "Sıra No", "Tarih", "Ad Soyad", "Birim", "Ünvan", "Başlangıç Saati", "Fiili Başlangıç", "Savunma" };
                for (int i = 0; i < headerTitles.Length; i++) ws.Cells[1, i + 1].Value = headerTitles[i];
                int row = 2, index = 1;
                foreach (var r in rows)
                {
                    ws.Cells[row, 1].Value = index++;
                    ws.Cells[row, 2].Value = r.LateDate.ToString("dd.MM.yyyy");
                    ws.Cells[row, 3].Value = r.FullName;
                    ws.Cells[row, 4].Value = r.Department;
                    ws.Cells[row, 5].Value = r.Title;
                    ws.Cells[row, 6].Value = r.ShiftStartTime.ToString(@"hh\:mm");
                    ws.Cells[row, 7].Value = r.ArrivalTime.ToString(@"hh\:mm");
                    ws.Cells[row, 8].Value = r.DefenseText;
                    row++;
                }

                var bytes = pck.GetAsByteArray();
                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"LateArrival_{DateTime.Now:yyyyMMdd}.xlsx");
            }
        }

        // Print page
        [DynamicAuthorize(Permission = "HumanResources.LateArrivalReport", Action = "Export")]
        public ActionResult Print()
        {
            var reports = _db.LateArrivalReports.OrderBy(r => r.LateDate).ToList();
            var hdr = _db.LateArrivalReportHeaders.FirstOrDefault();
            ViewBag.Header = hdr;
            return View(reports);
        }

        // Print a single report
        [DynamicAuthorize(Permission = "HumanResources.LateArrivalReport", Action = "Export")]
        public ActionResult PrintSingle(int id)
        {
            var report = _db.LateArrivalReports.FirstOrDefault(r => r.Id == id);
            if (report == null) return HttpNotFound();
            var hdr = _db.LateArrivalReportHeaders.FirstOrDefault();
            ViewBag.Header = hdr;
            return View("PrintSingle", report);
        }

        // JSON: Users by department
        [HttpGet]
        public JsonResult GetUsersByDepartment(string department)
        {
            if (string.IsNullOrWhiteSpace(department))
            {
                return Json(new object[0], JsonRequestBehavior.AllowGet);
            }

            var data = _db.Users
                .Where(u => u.Department == department)
                .Select(u => new
                {
                    fullName = (u.Name + " " + u.Surname).Trim(),
                    position = u.Position ?? ""
                })
                .OrderBy(u => u.fullName)
                .ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
} 