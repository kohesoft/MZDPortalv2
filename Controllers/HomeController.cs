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
    public class HomeController : Controller
    {
        private MZDNETWORKContext db = new MZDNETWORKContext(); // Veritabanı context'i  

        [AllowAnonymous]
        public ActionResult Index()
        {
            List<Gonderi> model = GetGonderiler(); // Ensure this method returns a valid list      
            return View(model);
        }

        private List<Gonderi> GetGonderiler()
        {
            try
            {
                return db.Gonderiler.Include("Ekler").OrderByDescending(g => g.CreatedAt).ToList();
            }
            catch (Exception ex)
            {
                // Log the error (you can add proper logging here)
                System.Diagnostics.Debug.WriteLine($"Error loading Gonderiler: {ex.Message}");
                return new List<Gonderi>();
            }
        }

        [AllowAnonymous]
        public ActionResult LoadMorePosts(int skip, int take)
        {
            var posts = GetGonderiler().Skip(skip).Take(take).ToList();
            return PartialView("_PostPartial", posts);
        }
        [DynamicAuthorize(Permission = "Yemek.Merkez")]
        public ActionResult MerkezYemekListesi()
        {
            return View();
        }
        [DynamicAuthorize(Permission = "Yemek.Yerleske")]
        public ActionResult YerleskeYemekListesi()
        {
            return View();
        }
        [AllowAnonymous]
        public ActionResult Mola()
        {
            return View();
        }

        // Permission debug sayfası
        [AllowAnonymous]
        public ActionResult PermissionDebug()
        {
            return View();
        }

        // Manual seeding test action  
        [AllowAnonymous]
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
