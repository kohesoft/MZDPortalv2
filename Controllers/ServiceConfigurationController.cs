using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using MZDNETWORK.Data;
using MZDNETWORK.Models;

namespace MZDNETWORK.Controllers
{
    public class ServiceConfigurationController : Controller
    {
        private readonly MZDNETWORKContext _context;

        public ServiceConfigurationController()
        {
            _context = new MZDNETWORKContext();
        }

        // GET: ServiceConfiguration
        public ActionResult Index()
        {
            return View();
        }

        // Servis listesi getir
        public ActionResult GetServices()
        {
            try
            {
                var services = _context.ServiceConfigurations
                    .Where(sc => sc.IsActive)
                    .Select(sc => new
                    {
                        Id = sc.Id,
                        ServiceName = sc.ServiceName,
                        RouteName = sc.RouteName,
                        Description = sc.Description,
                        VehiclePlate = sc.VehiclePlate,
                        DriverName = sc.DriverName,
                        DriverPhone = sc.DriverPhone,
                        DepartureTime = sc.DepartureTime,
                        ReturnTime = sc.ReturnTime,
                        Capacity = sc.Capacity,
                        CreatedDate = sc.CreatedDate,
                        PersonnelCount = sc.ServicePersonnels.Count(sp => sp.IsActive)
                    })
                    .OrderBy(sc => sc.ServiceName)
                    .ToList();

                return Json(new { success = true, data = services }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Yeni servis ekleme
        [HttpPost]
        public ActionResult AddService(string serviceName, string routeName, string description, 
            string vehiclePlate, string driverName, string driverPhone, 
            TimeSpan? departureTime, TimeSpan? returnTime, int capacity = 50)
        {
            try
            {
                // Gerekli alanların kontrolü
                if (string.IsNullOrEmpty(serviceName))
                {
                    return Json(new { success = false, message = "Servis adı gereklidir." });
                }

                if (string.IsNullOrEmpty(routeName))
                {
                    return Json(new { success = false, message = "Rota adı gereklidir." });
                }

                // Aynı isimde servis var mı kontrol et
                var existing = _context.ServiceConfigurations
                    .FirstOrDefault(sc => sc.ServiceName.ToLower() == serviceName.ToLower() && sc.IsActive);

                if (existing != null)
                {
                    return Json(new { success = false, message = "Bu isimde bir servis zaten mevcut." });
                }

                var serviceConfiguration = new ServiceConfiguration
                {
                    ServiceName = serviceName,
                    RouteName = routeName,
                    Description = description,
                    VehiclePlate = vehiclePlate,
                    DriverName = driverName,
                    DriverPhone = driverPhone,
                    DepartureTime = departureTime,
                    ReturnTime = returnTime,
                    Capacity = capacity,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                };

                _context.ServiceConfigurations.Add(serviceConfiguration);
                _context.SaveChanges();

                return Json(new { success = true, message = "Servis başarıyla eklendi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Servis güncelleme
        [HttpPost]
        public ActionResult UpdateService(int id, string serviceName, string routeName, string description, 
            string vehiclePlate, string driverName, string driverPhone, 
            TimeSpan? departureTime, TimeSpan? returnTime, int capacity = 50)
        {
            try
            {
                // Gerekli alanların kontrolü
                if (string.IsNullOrEmpty(serviceName))
                {
                    return Json(new { success = false, message = "Servis adı gereklidir." });
                }

                if (string.IsNullOrEmpty(routeName))
                {
                    return Json(new { success = false, message = "Rota adı gereklidir." });
                }

                var service = _context.ServiceConfigurations.Find(id);
                if (service == null)
                {
                    return Json(new { success = false, message = "Servis bulunamadı." });
                }

                // Aynı isimde başka servis var mı kontrol et
                var existing = _context.ServiceConfigurations
                    .FirstOrDefault(sc => sc.ServiceName.ToLower() == serviceName.ToLower() && 
                                         sc.IsActive && 
                                         sc.Id != id);

                if (existing != null)
                {
                    return Json(new { success = false, message = "Bu isimde başka bir servis zaten mevcut." });
                }

                service.ServiceName = serviceName;
                service.RouteName = routeName;
                service.Description = description;
                service.VehiclePlate = vehiclePlate;
                service.DriverName = driverName;
                service.DriverPhone = driverPhone;
                service.DepartureTime = departureTime;
                service.ReturnTime = returnTime;
                service.Capacity = capacity;
                service.UpdatedDate = DateTime.Now;

                _context.SaveChanges();

                return Json(new { success = true, message = "Servis başarıyla güncellendi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Servis silme (soft delete)
        [HttpPost]
        public ActionResult DeleteService(int id, string reason = "")
        {
            try
            {
                var service = _context.ServiceConfigurations.Find(id);
                if (service == null)
                {
                    return Json(new { success = false, message = "Servis bulunamadı." });
                }

                // Bu serviste aktif personel var mı kontrol et
                var hasActivePersonnel = _context.ServicePersonnels
                    .Any(sp => sp.ServiceConfigurationId == id && sp.IsActive);

                if (hasActivePersonnel)
                {
                    return Json(new { success = false, message = "Bu serviste aktif personel bulunduğu için silinemez. Önce personelleri çıkarın." });
                }

                // Soft delete kullan
                service.SoftDelete("System", reason);
                _context.SaveChanges();

                return Json(new { success = true, message = "Servis başarıyla silindi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Servis geri yükleme
        [HttpPost]
        public ActionResult RestoreService(int id)
        {
            try
            {
                var service = _context.ServiceConfigurations.Find(id);
                if (service == null)
                {
                    return Json(new { success = false, message = "Servis bulunamadı." });
                }

                // Restore işlemi
                service.Restore();
                _context.SaveChanges();

                return Json(new { success = true, message = "Servis başarıyla geri yüklendi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Silinen servisleri listeleme
        public ActionResult GetDeletedServices()
        {
            try
            {
                var deletedServices = _context.ServiceConfigurations
                    .Where(sc => !sc.IsActive && sc.DeletedDate.HasValue)
                    .Select(sc => new
                    {
                        Id = sc.Id,
                        ServiceName = sc.ServiceName,
                        RouteName = sc.RouteName,
                        Description = sc.Description,
                        VehiclePlate = sc.VehiclePlate,
                        DriverName = sc.DriverName,
                        DriverPhone = sc.DriverPhone,
                        DeletedDate = sc.DeletedDate,
                        DeletedBy = sc.DeletedBy,
                        DeletedReason = sc.DeletedReason
                    })
                    .OrderByDescending(sc => sc.DeletedDate)
                    .ToList();

                return Json(new { success = true, data = deletedServices }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Belirli bir servisi getir
        public ActionResult GetService(int id)
        {
            try
            {
                var service = _context.ServiceConfigurations
                    .Where(sc => sc.Id == id && sc.IsActive)
                    .Select(sc => new
                    {
                        Id = sc.Id,
                        ServiceName = sc.ServiceName,
                        RouteName = sc.RouteName,
                        Description = sc.Description,
                        VehiclePlate = sc.VehiclePlate,
                        DriverName = sc.DriverName,
                        DriverPhone = sc.DriverPhone,
                        DepartureTime = sc.DepartureTime,
                        ReturnTime = sc.ReturnTime,
                        Capacity = sc.Capacity
                    })
                    .FirstOrDefault();

                if (service == null)
                {
                    return Json(new { success = false, message = "Servis bulunamadı." }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = true, data = service }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
