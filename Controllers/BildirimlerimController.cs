using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MZDNETWORK.Models;


namespace MZDNETWORK.Controllers
{
  [Authorize(Roles = "IK, Yonetici, Sys, BilgiIslem, Merkez,Yerleske, IdariIsler")]
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
