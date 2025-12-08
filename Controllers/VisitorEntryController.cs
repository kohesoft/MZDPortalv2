using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using MZDNETWORK.Data;
using MZDNETWORK.Models;
using OfficeOpenXml;
using MZDNETWORK.Helpers;
using MZDNETWORK.Attributes;

namespace MZDNETWORK.Controllers
{
    [DynamicAuthorize(Permission = "InsanKaynaklari.ZiyaretciGiris")]
    public class VisitorEntryController : Controller
    {
        private readonly MZDNETWORKContext _db = new MZDNETWORKContext();

        // GET: VisitorEntry
        public ActionResult Index()
        {
            var entries = _db.VisitorEntries.OrderBy(e => e.Date).ThenBy(e => e.EntryTime).ToList();
            return View(entries);
        }

        // GET: VisitorEntry/Create
        public ActionResult Create()
        {
            var model = new VisitorEntry
            {
                Date = DateTime.Today,
                EntryTime = DateTime.Now.TimeOfDay
            };
            return View(model);
        }

        // POST: VisitorEntry/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        [DynamicAuthorize(Permission = "InsanKaynaklari.ZiyaretciGiris", Action = "Create")]
        public ActionResult Create(VisitorEntry model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Sunucu tarafÄ±nda tarih ve giriÅŸ saatini sabitle
            model.Date = DateTime.Today;
            model.EntryTime = DateTime.Now.TimeOfDay;

            _db.VisitorEntries.Add(model);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET + POST: Header (Admin only)
        [DynamicAuthorize(Permission = "InsanKaynaklari.ZiyaretciGiris", Action = "Manage")]
        public ActionResult Header()
        {
            var header = _db.VisitorEntryHeaders.FirstOrDefault() ?? new VisitorEntryHeader
            {
                FirstPublishDate = DateTime.Today,
                RevisionDate = DateTime.Today
            };
            return View(header);
        }

        [HttpPost]
        [DynamicAuthorize(Permission = "InsanKaynaklari.ZiyaretciGiris", Action = "Manage")]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Header(VisitorEntryHeader model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.Id == 0)
            {
                _db.VisitorEntryHeaders.Add(model);
            }
            else
            {
                _db.Entry(model).State = EntityState.Modified;
            }
            _db.SaveChanges();
            return RedirectToAction("Header");
        }

        // Excel export (Admin)
        [DynamicAuthorize(Permission = "InsanKaynaklari.ZiyaretciGiris", Action = "Export")]
        public ActionResult ExportExcel()
        {
            var rows = _db.VisitorEntries.OrderBy(e => e.Date).ToList();
            using (var pck = new ExcelPackage())
            {
                var ws = pck.Workbook.Worksheets.Add("VisitorLog");
                // Headers
                string[] headerTitles = { "SÄ±ra No", "Tarih", "Ad Soyad", "GeliÅŸ Sebebi", "Firma", "GÃ¶revi", "Kimlik No", "TC Kimlik", "GiriÅŸ", "Ã‡Ä±kÄ±ÅŸ" };
                for (int i = 0; i < headerTitles.Length; i++) ws.Cells[1, i + 1].Value = headerTitles[i];
                int row = 2;
                int index = 1;
                foreach (var e in rows)
                {
                    ws.Cells[row, 1].Value = index++;
                    ws.Cells[row, 2].Value = e.Date.ToString("dd.MM.yyyy");
                    ws.Cells[row, 3].Value = e.FullName;
                    ws.Cells[row, 4].Value = e.ArrivalReason;
                    ws.Cells[row, 5].Value = e.Organization;
                    ws.Cells[row, 6].Value = e.Duty;
                    ws.Cells[row, 7].Value = e.IdentityNo;
                    ws.Cells[row, 8].Value = e.TCKimlik;
                    ws.Cells[row, 9].Value = e.EntryTime.ToString(@"hh\:mm");
                    ws.Cells[row, 10].Value = e.ExitTime?.ToString(@"hh\:mm");
                    row++;
                }

                var bytes = pck.GetAsByteArray();
                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"VisitorLog_{DateTime.Now:yyyyMMdd}.xlsx");
            }
        }

        // Print page (Admin)
        [DynamicAuthorize(Permission = "InsanKaynaklari.ZiyaretciGiris", Action = "Export")]
        public ActionResult Print()
        {
            var entries = _db.VisitorEntries.OrderBy(e => e.Date).ThenBy(e => e.EntryTime).ToList();
            var hdr = _db.VisitorEntryHeaders.FirstOrDefault();
            ViewBag.Header = hdr;
            return View(entries);
        }

        // POST: VisitorEntry/SetExit
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DynamicAuthorize(Permission = "InsanKaynaklari.ZiyaretciGiris", Action = "Edit")]
        public ActionResult SetExit(int id)
        {
            var entry = _db.VisitorEntries.FirstOrDefault(e => e.Id == id);
            if (entry == null) return HttpNotFound();
            entry.ExitTime = DateTime.Now.TimeOfDay;
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
} 
