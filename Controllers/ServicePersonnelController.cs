using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using MZDNETWORK.Data;
using MZDNETWORK.Models;

namespace MZDNETWORK.Controllers
{
    public class ServicePersonnelController : Controller
    {
        private readonly MZDNETWORKContext _context;

        public ServicePersonnelController()
        {
            _context = new MZDNETWORKContext();
        }

        // GET: ServicePersonnel
        public ActionResult Index()
        {
            return View();
        }

        // Normal servis personeli listesi
        public ActionResult GetPublicData()
        {
            try
            {
                var servicePersonnels = _context.ServicePersonnels
                    .Include(sp => sp.User)
                    .Include(sp => sp.ServiceConfiguration)
                    .Where(sp => sp.IsActive && sp.ServiceConfiguration.IsActive)
                    .Select(sp => new
                    {
                        Id = sp.Id,
                        UserFullName = sp.User.Name + " " + sp.User.Surname,
                        Department = sp.User.Department,
                        Position = sp.User.Position,
                        ServiceName = sp.ServiceConfiguration.ServiceName,
                        RouteName = sp.ServiceConfiguration.RouteName,
                        VehiclePlate = sp.ServiceConfiguration.VehiclePlate,
                        DriverName = sp.ServiceConfiguration.DriverName,
                        DriverPhone = sp.ServiceConfiguration.DriverPhone,
                        DepartureTime = sp.ServiceConfiguration.DepartureTime,
                        ReturnTime = sp.ServiceConfiguration.ReturnTime
                    })
                    .OrderBy(sp => sp.ServiceName)
                    .ThenBy(sp => sp.UserFullName)
                    .ToList();

                return Json(new { success = true, data = servicePersonnels }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Mesai servisi personeli listesi
        public ActionResult GetOvertimeData()
        {
            try
            {
                var today = DateTime.Today;
                var overtimePersonnels = _context.OvertimeServicePersonnels
                    .Include(osp => osp.User)
                    .Include(osp => osp.ServiceConfiguration)
                    .Where(osp => osp.IsActive && 
                                  osp.ServiceConfiguration.IsActive && 
                                  DbFunctions.TruncateTime(osp.ServiceDate) == today)
                    .Select(osp => new
                    {
                        Id = osp.Id,
                        UserFullName = osp.User.Name + " " + osp.User.Surname,
                        Department = osp.User.Department,
                        Position = osp.User.Position,
                        ServiceName = osp.ServiceConfiguration.ServiceName,
                        RouteName = osp.ServiceConfiguration.RouteName,
                        VehiclePlate = osp.ServiceConfiguration.VehiclePlate,
                        DriverName = osp.ServiceConfiguration.DriverName,
                        DriverPhone = osp.ServiceConfiguration.DriverPhone,
                        DepartureTime = osp.ServiceConfiguration.DepartureTime,
                        ReturnTime = osp.ServiceConfiguration.ReturnTime,
                        ServiceDate = osp.ServiceDate
                    })
                    .OrderBy(osp => osp.ServiceName)
                    .ThenBy(osp => osp.UserFullName)
                    .ToList();

                return Json(new { success = true, data = overtimePersonnels }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Servis listesi
        public ActionResult GetPublicServices()
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
                        VehiclePlate = sc.VehiclePlate,
                        DriverName = sc.DriverName,
                        DriverPhone = sc.DriverPhone,
                        DepartureTime = sc.DepartureTime,
                        ReturnTime = sc.ReturnTime,
                        Capacity = sc.Capacity,
                        CurrentPersonnelCount = sc.ServicePersonnels.Count(sp => sp.IsActive)
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

        // Admin sayfası - Personel yönetimi
        public ActionResult Admin()
        {
            return View();
        }

        // Servis kullanan tüm personelleri getir (Mesai için ekleme yaparken kullanılacak)
        public ActionResult GetServiceUsers()
        {
            try
            {
                var serviceUsers = _context.ServicePersonnels
                    .Include(sp => sp.User)
                    .Where(sp => sp.IsActive)
                    .Select(sp => new
                    {
                        Id = sp.User.Id,
                        FullName = sp.User.Name + " " + sp.User.Surname,
                        Department = sp.User.Department,
                        Position = sp.User.Position,
                        Username = sp.User.Username
                    })
                    .Distinct()
                    .OrderBy(u => u.FullName)
                    .ToList();

                return Json(new { success = true, data = serviceUsers }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Tüm personelleri getir (Normal servis ekleme için)
        public ActionResult GetAllUsers()
        {
            try
            {
                var users = _context.Users
                    .Select(u => new
                    {
                        Id = u.Id,
                        FullName = u.Name + " " + u.Surname,
                        Department = u.Department,
                        Position = u.Position,
                        Username = u.Username
                    })
                    .OrderBy(u => u.FullName)
                    .ToList();

                return Json(new { success = true, data = users }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Normal servise personel ekleme
        [HttpPost]
        public ActionResult AddServicePersonnel(int userId, int serviceConfigurationId)
        {
            try
            {
                // Aynı kişi aynı serviste zaten var mı kontrol et
                var existing = _context.ServicePersonnels
                    .FirstOrDefault(sp => sp.UserId == userId && 
                                         sp.ServiceConfigurationId == serviceConfigurationId && 
                                         sp.IsActive);

                if (existing != null)
                {
                    return Json(new { success = false, message = "Bu personel zaten bu serviste kayıtlı." });
                }

                var servicePersonnel = new ServicePersonnel
                {
                    UserId = userId,
                    ServiceConfigurationId = serviceConfigurationId,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                };

                _context.ServicePersonnels.Add(servicePersonnel);
                _context.SaveChanges();

                return Json(new { success = true, message = "Personel servise başarıyla eklendi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Mesai servisine personel ekleme (sadece servis kullanan personeller)
        [HttpPost]
        public ActionResult AddOvertimeServicePersonnel(int userId, int serviceConfigurationId)
        {
            try
            {
                var today = DateTime.Today;

                // Kişinin normal serviste kayıtlı olup olmadığını kontrol et
                var hasNormalService = _context.ServicePersonnels
                    .Any(sp => sp.UserId == userId && sp.IsActive);

                if (!hasNormalService)
                {
                    return Json(new { success = false, message = "Bu personel normal serviste kayıtlı değil. Mesai servisine eklenemez." });
                }

                // Bugün için zaten ekli mi kontrol et
                var existing = _context.OvertimeServicePersonnels
                    .FirstOrDefault(osp => osp.UserId == userId && 
                                          osp.ServiceConfigurationId == serviceConfigurationId && 
                                          DbFunctions.TruncateTime(osp.ServiceDate) == today && 
                                          osp.IsActive);

                if (existing != null)
                {
                    return Json(new { success = false, message = "Bu personel bugün zaten bu mesai servisinde kayıtlı." });
                }

                var overtimeServicePersonnel = new OvertimeServicePersonnel
                {
                    UserId = userId,
                    ServiceConfigurationId = serviceConfigurationId,
                    ServiceDate = today,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                };

                _context.OvertimeServicePersonnels.Add(overtimeServicePersonnel);
                _context.SaveChanges();

                return Json(new { success = true, message = "Personel mesai servisine başarıyla eklendi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Normal servisten personel çıkarma (soft delete)
        [HttpPost]
        public ActionResult RemoveServicePersonnel(int id, string reason = "")
        {
            try
            {
                var servicePersonnel = _context.ServicePersonnels.Find(id);
                if (servicePersonnel == null)
                {
                    return Json(new { success = false, message = "Personel bulunamadı." });
                }

                // Soft delete kullan
                servicePersonnel.SoftDelete("System", reason);
                _context.SaveChanges();

                return Json(new { success = true, message = "Personel servisten başarıyla çıkarıldı." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Mesai servisinden personel çıkarma (soft delete)
        [HttpPost]
        public ActionResult RemoveOvertimeServicePersonnel(int id, string reason = "")
        {
            try
            {
                var overtimeServicePersonnel = _context.OvertimeServicePersonnels.Find(id);
                if (overtimeServicePersonnel == null)
                {
                    return Json(new { success = false, message = "Personel bulunamadı." });
                }

                // Soft delete kullan
                overtimeServicePersonnel.SoftDelete("System", reason);
                _context.SaveChanges();

                return Json(new { success = true, message = "Personel mesai servisinden başarıyla çıkarıldı." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Normal servisten personel geri yükleme
        [HttpPost]
        public ActionResult RestoreServicePersonnel(int id)
        {
            try
            {
                var servicePersonnel = _context.ServicePersonnels.Find(id);
                if (servicePersonnel == null)
                {
                    return Json(new { success = false, message = "Personel bulunamadı." });
                }

                // Restore işlemi
                servicePersonnel.Restore();
                _context.SaveChanges();

                return Json(new { success = true, message = "Personel servise başarıyla geri yüklendi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Mesai servisinden personel geri yükleme
        [HttpPost]
        public ActionResult RestoreOvertimeServicePersonnel(int id)
        {
            try
            {
                var overtimeServicePersonnel = _context.OvertimeServicePersonnels.Find(id);
                if (overtimeServicePersonnel == null)
                {
                    return Json(new { success = false, message = "Personel bulunamadı." });
                }

                // Restore işlemi
                overtimeServicePersonnel.Restore();
                _context.SaveChanges();

                return Json(new { success = true, message = "Personel mesai servisine başarıyla geri yüklendi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Silinen personelleri listeleme
        public ActionResult GetDeletedServicePersonnels()
        {
            try
            {
                var deletedPersonnels = _context.ServicePersonnels
                    .Include(sp => sp.User)
                    .Include(sp => sp.ServiceConfiguration)
                    .Where(sp => !sp.IsActive && sp.DeletedDate.HasValue)
                    .Select(sp => new
                    {
                        Id = sp.Id,
                        UserFullName = sp.User.Name + " " + sp.User.Surname,
                        Department = sp.User.Department,
                        Position = sp.User.Position,
                        ServiceName = sp.ServiceConfiguration.ServiceName,
                        RouteName = sp.ServiceConfiguration.RouteName,
                        DeletedDate = sp.DeletedDate,
                        DeletedBy = sp.DeletedBy,
                        DeletedReason = sp.DeletedReason
                    })
                    .OrderByDescending(sp => sp.DeletedDate)
                    .ToList();

                return Json(new { success = true, data = deletedPersonnels }, JsonRequestBehavior.AllowGet);
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
