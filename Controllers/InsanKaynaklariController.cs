using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MZDNETWORK.Models;

namespace MZDNETWORK.Controllers
{
      [Authorize(Roles = "IK, Yonetici, Sys")]
    public class InsanKaynaklariController : Controller
    {

        private readonly MZDNETWORKContext db;

        public InsanKaynaklariController()
        {
            db = new MZDNETWORKContext();
        }

        [HttpGet]
        public ActionResult DilekIstek_IK()
        {
            var dilekIstekler = db.DilekOneriler.ToList();
            return View(dilekIstekler);
        }

        public ActionResult Index()
        {
            return View();
        }


        



    }
}