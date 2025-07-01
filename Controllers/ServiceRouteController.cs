using System.Collections.Generic;
using System.Web.Mvc;
using MZDNETWORK.Models;

namespace MZDNETWORK.Controllers
{
    public class ServiceRouteController : Controller
    {
        // GET: /ServiceRoute
        public ActionResult Index()
        {
            // Örnek veri – embed linklerini buraya ekleyin
            var routes = new List<ServiceRoute>
            {
                new ServiceRoute { Title = "MZD-1 ERYAMAN-BATIKENT", KmlPath = "~/Content/kml/eryaman-batikent.kml" },
                new ServiceRoute { Title = "MZD-2 DİKMEN", KmlPath = "~/Content/kml/dikmen.kml" },
                new ServiceRoute { Title = "MZD-3 KEÇİÖREN-UFUKTEPE", KmlPath = "~/Content/kml/kecioren-ufuktepe.kml" },
                new ServiceRoute { Title = "MZD-4 YENİMAHALLE", KmlPath = "~/Content/kml/yenimahalle.kml" },
                new ServiceRoute { Title = "MZD-5 KARAPÜRÇEK", KmlPath = "~/Content/kml/karapurcek.kml" },
                new ServiceRoute { Title = "MZD-6 SİNCAN-ETİMESGUT", KmlPath = "~/Content/kml/sincan_etimesgut.kml" },
                new ServiceRoute { Title = "MZD-7 PURSAKLAR", KmlPath = "~/Content/kml/pursaklar.kml" },
                new ServiceRoute { Title = "MZD-8 MAMAK-1 (BOĞAZİÇİ)", KmlPath = "~/Content/kml/mamak-1.kml" },
                new ServiceRoute { Title = "MZD-9 MAMAK-2 (BAHÇELERÜSTÜ)", KmlPath = "~/Content/kml/mamak-2.kml" },
                new ServiceRoute { Title = "MZD-10 MAMAK-3 (KIBRISKÖY)", KmlPath = "~/Content/kml/mamak-3.kml" },
            };

            return View(routes);
        }
    }
} 