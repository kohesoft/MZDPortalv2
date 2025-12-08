// Controllers/BeyazTahtaController.cs
using System;
using System.Linq;
using System.Web.Mvc;
using MZDNETWORK;
using MZDNETWORK.Models;
using MZDNETWORK.Data;
using MZDNETWORK.Attributes;

namespace MZDNETWORK.Controllers
{
    
    public class BeyazTahtaController : Controller
    {
        private readonly MZDNETWORKContext db = new MZDNETWORKContext();

        
        public ActionResult Index()
            => View();

        
        public ActionResult TV()
        {
            // Varsayılan başlıklar
            ViewBag.Headers = new[] { "Tarih", "Öneri Veren", "Problem", "Öneri" };
            return View();
        }

        
        [HttpGet]
        public JsonResult GetHeader()
        {
            var h = db.TvHeaders.Find(1);
            return Json(new { title = h?.Title ?? "" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DynamicAuthorize(Permission = "Operasyon.BeyazTahta.BaslikGuncelle", Action = "Edit")]
        public JsonResult UpdateHeader(string title)
        {
            var h = db.TvHeaders.Find(1);
            if (h == null) return Json(new { success = false });
            h.Title = title;
            db.SaveChanges();
            return Json(new { success = true });
        }

        [HttpGet]
        public JsonResult GetEntries()
        {
            var list = db.BeyazTahtaEntries
                         .OrderByDescending(x => x.CreatedAt)
                         .Select(x => new {
                             x.Id,
                             x.CreatedAt,
                             x.OneriVeren,
                             x.Problem,
                             x.Oneri
                         })
                         .ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DynamicAuthorize(Permission = "Operasyon.BeyazTahta.Girdi", Action = "Create")]
        public JsonResult CreateEntry(BeyazTahtaEntry vm)
        {
            vm.CreatedAt = DateTime.Now;
            db.BeyazTahtaEntries.Add(vm);
            db.SaveChanges();
            return Json(new { success = true });
        }

        [HttpPost]
        [DynamicAuthorize(Permission = "Operasyon.BeyazTahta.Girdi", Action = "Edit")]
        public JsonResult EditEntry(BeyazTahtaEntry vm)
        {
            var ent = db.BeyazTahtaEntries.Find(vm.Id);
            if (ent == null) return Json(new { success = false });
            ent.OneriVeren = vm.OneriVeren;
            ent.Problem = vm.Problem;
            ent.Oneri = vm.Oneri;
            db.SaveChanges();
            return Json(new { success = true });
        }

        [HttpPost]
        [DynamicAuthorize(Permission = "Operasyon.BeyazTahta.Girdi", Action = "Delete")]
        public JsonResult DeleteEntry(int id)
        {
            var ent = db.BeyazTahtaEntries.Find(id);
            if (ent == null) return Json(new { success = false });
            db.BeyazTahtaEntries.Remove(ent);
            db.SaveChanges();
            return Json(new { success = true });
        }

        public JsonResult GetCurrentUserName()
        {
            var userName = User.Identity.IsAuthenticated ? User.Identity.Name : "Anonim";
            return Json(new { success = true, userName }, JsonRequestBehavior.AllowGet);
        }
    }
}

