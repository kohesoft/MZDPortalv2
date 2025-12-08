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
    [DynamicAuthorize(Permission = "Operasyon.Oneri")]
    public class BildirimlerimController : Controller
    {
        public ActionResult Bildirimlerim()
        {
            try
            {
                using (var context = new MZDNETWORKContext())
                {
                    var dilekOneriler = context.DilekOneriler.ToList();

                    return View(dilekOneriler);
                }
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, "Internal server error");
            }
        }
    }
}

