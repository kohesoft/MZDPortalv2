using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using MZDNETWORK.Models;
using System.Data.Entity;


namespace MZDNETWORK.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private MZDNETWORKContext db = new MZDNETWORKContext(); // Veritabanı context'i
        

        public ActionResult Index()
        {
            List<Gonderi> model = GetGonderiler(); // Ensure this method returns a valid list                                            
            return View(model);
        }
        
        private List<Gonderi> GetGonderiler()
        {
            return db.Gonderiler.ToList(); // Veritabanından Gonderi listesini al
        }

        public ActionResult LoadMorePosts(int skip, int take)
        {
            var posts = GetGonderiler().Skip(skip).Take(take).ToList();
            return PartialView("_PostPartial", posts);
        }

        public ActionResult MerkezYemekListesi()
        {
            return View();
        }
        public ActionResult YerleskeYemekListesi()
        {
            return View();
        }
        public ActionResult Mola()
        {
            return View();
        }

    }
}