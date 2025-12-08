using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MZDNETWORK.Models;
using MZDNETWORK.Data;
using MZDNETWORK.Attributes;

namespace MZDNETWORK.Controllers
{
    [DynamicAuthorize(Permission = "InsanKaynaklari.Oneri")]
    public class InsanKaynaklariController : Controller
    {
        private readonly MZDNETWORKContext db;

        public InsanKaynaklariController()
        {
            db = new MZDNETWORKContext();
        }

        [HttpGet]
        [DynamicAuthorize(Permission = "InsanKaynaklari.Oneri")]
        public ActionResult DilekIstek_IK()
        {
            var dilekIstekler = db.DilekOneriler.ToList();
            return View(dilekIstekler);
        }

        [DynamicAuthorize(Permission = "InsanKaynaklari.Oneri")]
        public ActionResult Index()
        {
            return View();
        }
    }
}
