using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using MZDNETWORK.Models;
using MZDNETWORK.Data;
using System.Data.Entity;
using System.IO;
using MZDNETWORK.Attributes;

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
        [DynamicAuthorize(Permission = "Food.Merkez")]
        public ActionResult MerkezYemekListesi()
        {
            return View();
        }
        [DynamicAuthorize(Permission = "Food.Yerleske")]
        public ActionResult YerleskeYemekListesi()
        {
            return View();
        }
        public ActionResult Mola()
        {
            return View();
        }

        // Manual seeding test action  
        public ActionResult TestSeeding()
        {
            try
            {
                ViewBag.Message = "Seeding başlatılıyor...";

                using (var context = new MZDNETWORKContext())
                {
                    // Permission seeding  
                    PermissionSeeder.SeedPermissions(context);

                    // Admin role seeding  
                    PermissionSeeder.CreateDefaultAdminRole(context);

                    // Admin user seeding  
                    PermissionSeeder.CreateDefaultAdminUser(context);

                    ViewBag.Message = "✅ Seeding başarıyla tamamlandı!";
                    ViewBag.AdminUser = "admin";
                    ViewBag.AdminPassword = "admin123";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"❌ Seeding hatası: {ex.Message}";
                ViewBag.Error = ex.ToString();
            }

            return View();
        }
    }
}