using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using MZDNETWORK.Data;
using MZDNETWORK.Models;

namespace MZDNETWORK.Controllers
{
    public class OvertimeServiceManagerController : Controller
    {
        private readonly MZDNETWORKContext _context;

        public OvertimeServiceManagerController()
        {
            _context = new MZDNETWORKContext();
        }

        // GET: /OvertimeServiceManager - Basit mesai servisi yönetim sayfası
        public ActionResult Index()
        {
            return View();
        }

        // Servis kullanan tüm personelleri getir (mesai için uygun olanlar)
        public ActionResult GetAvailablePersonnel()
        {
            try
            {
                var today = DateTime.Today;
                
                // Normal serviste kayıtlı olan ve bugün mesai servisinde olmayan personeller
                var availablePersonnel = _context.ServicePersonnels
                    .Include(sp => sp.User)
                    .Include(sp => sp.ServiceConfiguration)
                    .Where(sp => sp.IsActive)
                    .Where(sp => !_context.OvertimeServicePersonnels
                        .Any(osp => osp.UserId == sp.UserId && 
                                   DbFunctions.TruncateTime(osp.ServiceDate) == today && 
                                   osp.IsActive))
                    .Select(sp => new
                    {
                        UserId = sp.UserId,
                        FirstName = sp.User.Name,
                        LastName = sp.User.Surname,
                        PersonelNo = sp.User.Id.ToString(), // Id'yi PersonelNo olarak kullan
                        DepartmentName = sp.User.Department,
                        PhoneNumber = sp.User.PhoneNumber,
                        ServiceName = sp.ServiceConfiguration.ServiceName,
                        RouteName = sp.ServiceConfiguration.RouteName,
                        ServiceId = sp.ServiceConfigurationId,
                        UserPhoto = "/Content/images/default-user.png"
                    })
                    .Distinct()
                    .OrderBy(p => p.FirstName)
                    .ToList();

                return Json(new { success = true, data = availablePersonnel }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Bugün mesai servisine eklenmiş personelleri getir
        public ActionResult GetTodayOvertimePersonnel()
        {
            try
            {
                var today = DateTime.Today;
                
                var overtimePersonnel = _context.OvertimeServicePersonnels
                    .Include(osp => osp.User)
                    .Include(osp => osp.ServiceConfiguration)
                    .Where(osp => osp.IsActive && 
                                  DbFunctions.TruncateTime(osp.ServiceDate) == today)
                    .Select(osp => new
                    {
                        Id = osp.Id,
                        UserId = osp.UserId,
                        UserFullName = osp.User.Name + " " + osp.User.Surname,
                        Department = osp.User.Department,
                        Position = osp.User.Position,
                        ServiceName = osp.ServiceConfiguration.ServiceName,
                        RouteName = osp.ServiceConfiguration.RouteName,
                        ServiceId = osp.ServiceConfigurationId,
                        UserPhoto = "/Content/images/default-user.png"
                    })
                    .OrderBy(osp => osp.ServiceName)
                    .ThenBy(osp => osp.UserFullName)
                    .ToList();

                return Json(new { success = true, data = overtimePersonnel }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Aktif servisleri getir
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
                        VehiclePlate = sc.VehiclePlate,
                        DriverName = sc.DriverName,
                        DriverPhone = sc.DriverPhone,
                        Capacity = sc.Capacity,
                        CurrentCount = sc.OvertimeServicePersonnels
                            .Count(osp => osp.IsActive && 
                                         DbFunctions.TruncateTime(osp.ServiceDate) == DateTime.Today)
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

        // Bugün mesai servisine eklenmiş personelleri getir
        public ActionResult GetOvertimePersonnel()
        {
            try
            {
                var today = DateTime.Today;
                
                var overtimePersonnel = _context.OvertimeServicePersonnels
                    .Include(osp => osp.User)
                    .Include(osp => osp.ServiceConfiguration)
                    .Where(osp => osp.IsActive && 
                                  DbFunctions.TruncateTime(osp.ServiceDate) == today)
                    .Select(osp => new
                    {
                        Id = osp.Id,
                        UserId = osp.UserId,
                        FirstName = osp.User.Name,
                        LastName = osp.User.Surname,
                        PersonelNo = osp.User.Id.ToString(), // Id'yi PersonelNo olarak kullan
                        DepartmentName = osp.User.Department,
                        PhoneNumber = osp.User.PhoneNumber,
                        ServiceName = osp.ServiceConfiguration.ServiceName,
                        RouteName = osp.ServiceConfiguration.RouteName,
                        ServiceId = osp.ServiceConfigurationId,
                        UserPhoto = "/Content/images/default-user.png"
                    })
                    .OrderBy(osp => osp.ServiceName)
                    .ThenBy(osp => osp.FirstName)
                    .ToList();

                return Json(new { success = true, data = overtimePersonnel }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Aktif servisleri getir
        public ActionResult GetActiveServices()
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
                        Capacity = sc.Capacity,
                        CurrentCount = sc.OvertimeServicePersonnels
                            .Count(osp => osp.IsActive && 
                                         DbFunctions.TruncateTime(osp.ServiceDate) == DateTime.Today)
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

        // JavaScript'ten gelen basit ekleme metodu
        [HttpPost]
        public ActionResult AddToOvertime(List<int> userIds, int? serviceId)
        {
            try
            {
                if (userIds == null || !userIds.Any())
                {
                    return Json(new { success = false, message = "Lütfen en az bir personel seçin." });
                }

                var today = DateTime.Today;
                var addedCount = 0;
                var errors = new List<string>();

                // Servis ID'si verilmişse onu kullan, yoksa ilk aktif servisi al
                ServiceConfiguration targetService;
                if (serviceId.HasValue)
                {
                    targetService = _context.ServiceConfigurations.FirstOrDefault(sc => sc.Id == serviceId.Value && sc.IsActive);
                    if (targetService == null)
                    {
                        return Json(new { success = false, message = "Seçilen servis bulunamadı veya aktif değil." });
                    }
                }
                else
                {
                    targetService = _context.ServiceConfigurations.FirstOrDefault(sc => sc.IsActive);
                    if (targetService == null)
                    {
                        return Json(new { success = false, message = "Aktif servis bulunamadı." });
                    }
                }

                foreach (var userId in userIds)
                {
                    try
                    {
                        // Bugün zaten mesai servisinde mi kontrol et
                        var alreadyExists = _context.OvertimeServicePersonnels
                            .Any(osp => osp.UserId == userId && 
                                       DbFunctions.TruncateTime(osp.ServiceDate) == today && 
                                       osp.IsActive);

                        if (alreadyExists)
                        {
                            continue;
                        }

                        // Mesai servisine ekle
                        var overtimeServicePersonnel = new OvertimeServicePersonnel
                        {
                            UserId = userId,
                            ServiceConfigurationId = targetService.Id,
                            ServiceDate = today,
                            IsActive = true,
                            CreatedDate = DateTime.Now
                        };

                        _context.OvertimeServicePersonnels.Add(overtimeServicePersonnel);
                        addedCount++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"User {userId}: {ex.Message}");
                    }
                }

                if (addedCount > 0)
                {
                    _context.SaveChanges();
                }

                return Json(new { 
                    success = true, 
                    message = $"{addedCount} personel {targetService.ServiceName} mesai servisine eklendi."
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // JavaScript'ten gelen basit çıkarma metodu
        [HttpPost]
        public ActionResult RemoveFromOvertime(List<int> userIds)
        {
            try
            {
                if (userIds == null || !userIds.Any())
                {
                    return Json(new { success = false, message = "Lütfen en az bir personel seçin." });
                }

                var today = DateTime.Today;
                var removedCount = 0;

                foreach (var userId in userIds)
                {
                    var overtimePersonnel = _context.OvertimeServicePersonnels
                        .FirstOrDefault(osp => osp.UserId == userId && 
                                              DbFunctions.TruncateTime(osp.ServiceDate) == today && 
                                              osp.IsActive);

                    if (overtimePersonnel != null)
                    {
                        overtimePersonnel.SoftDelete("System", "Manuel olarak çıkarıldı");
                        removedCount++;
                    }
                }

                if (removedCount > 0)
                {
                    _context.SaveChanges();
                }

                return Json(new { 
                    success = true, 
                    message = $"{removedCount} personel mesai servisinden çıkarıldı."
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Toplu mesai servisi ekleme
        [HttpPost]
        public ActionResult AddMultipleToOvertimeService(List<int> userIds, int serviceConfigurationId)
        {
            try
            {
                if (userIds == null || !userIds.Any())
                {
                    return Json(new { success = false, message = "Lütfen en az bir personel seçin." });
                }

                var today = DateTime.Today;
                var addedCount = 0;
                var skippedCount = 0;
                var errors = new List<string>();

                foreach (var userId in userIds)
                {
                    try
                    {
                        // Bu kişi normal serviste kayıtlı mı kontrol et
                        var hasNormalService = _context.ServicePersonnels
                            .Any(sp => sp.UserId == userId && sp.IsActive);

                        if (!hasNormalService)
                        {
                            var user = _context.Users.Find(userId);
                            errors.Add($"{user?.Name} {user?.Surname} normal serviste kayıtlı değil.");
                            skippedCount++;
                            continue;
                        }

                        // Bugün zaten mesai servisinde mi kontrol et
                        var alreadyExists = _context.OvertimeServicePersonnels
                            .Any(osp => osp.UserId == userId && 
                                       DbFunctions.TruncateTime(osp.ServiceDate) == today && 
                                       osp.IsActive);

                        if (alreadyExists)
                        {
                            skippedCount++;
                            continue;
                        }

                        // Mesai servisine ekle
                        var overtimeServicePersonnel = new OvertimeServicePersonnel
                        {
                            UserId = userId,
                            ServiceConfigurationId = serviceConfigurationId,
                            ServiceDate = today,
                            IsActive = true,
                            CreatedDate = DateTime.Now
                        };

                        _context.OvertimeServicePersonnels.Add(overtimeServicePersonnel);
                        addedCount++;
                    }
                    catch (Exception ex)
                    {
                        var user = _context.Users.Find(userId);
                        errors.Add($"{user?.Name} {user?.Surname}: {ex.Message}");
                        skippedCount++;
                    }
                }

                if (addedCount > 0)
                {
                    _context.SaveChanges();
                }

                var message = $"İşlem tamamlandı. {addedCount} personel eklendi";
                if (skippedCount > 0)
                {
                    message += $", {skippedCount} personel atlandı";
                }
                if (errors.Any())
                {
                    message += $". Hatalar: {string.Join(", ", errors)}";
                }

                return Json(new { 
                    success = true, 
                    message = message,
                    addedCount = addedCount,
                    skippedCount = skippedCount,
                    errors = errors
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Mesai servisinden çıkarma
        [HttpPost]
        public ActionResult RemoveFromOvertimeService(int overtimeServicePersonnelId)
        {
            try
            {
                var overtimePersonnel = _context.OvertimeServicePersonnels.Find(overtimeServicePersonnelId);
                if (overtimePersonnel == null)
                {
                    return Json(new { success = false, message = "Personel bulunamadı." });
                }

                overtimePersonnel.SoftDelete("System", "Manuel olarak çıkarıldı");
                _context.SaveChanges();

                return Json(new { success = true, message = "Personel mesai servisinden çıkarıldı." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
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
