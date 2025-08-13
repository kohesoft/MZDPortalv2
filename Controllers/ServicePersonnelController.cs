using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using MZDNETWORK.Attributes;
using MZDNETWORK.Data;
using MZDNETWORK.Models;
using Newtonsoft.Json;

namespace MZDNETWORK.Controllers
{
    [DynamicAuthorize(Permission = "ServiceManagement.Personnel")]
    public class ServicePersonnelController : Controller
    {
        private MZDNETWORKContext db = new MZDNETWORKContext();

        // GET: ServicePersonnel
        [DynamicAuthorize(Permission = "ServiceManagement.Personnel", Action = "View")]
        public ActionResult Index()
        {
            return View();
        }

        // GET: ServicePersonnel/GetAll
        [DynamicAuthorize(Permission = "ServiceManagement.Personnel", Action = "View")]
        public JsonResult GetAll()
        {
            try
            {
                var personnel = db.ServicePersonnels
                    .Include(sp => sp.User)
                    .OrderBy(p => p.User.Name)
                    .ToList();

                // Manually load ServiceConfiguration for each personnel
                var serviceCodes = personnel.Select(p => p.ServiceCode).Distinct().ToList();
                var serviceConfigs = db.ServiceConfigurations
                    .Where(sc => serviceCodes.Contains(sc.ServiceCode))
                    .ToList();

                var result = personnel.Select(p => {
                    var serviceConfig = serviceConfigs.FirstOrDefault(sc => sc.ServiceCode == p.ServiceCode);
                    return new
                    {
                        p.Id,
                        p.UserId,
                        FirstName = p.User.Name,
                        LastName = p.User.Surname,
                        Username = p.User.Username,
                        Service = p.ServiceCode,
                        ServiceName = serviceConfig?.ServiceName ?? p.ServiceCode,
                        p.ShiftType,
                        p.Status,
                        p.CreatedDate
                    };
                });

                return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: ServicePersonnel/GetById/5
        [DynamicAuthorize(Permission = "ServiceManagement.Personnel", Action = "View")]
        public JsonResult GetById(int id)
        {
            try
            {
                var personnel = db.ServicePersonnels
                    .Include(sp => sp.User)
                    .FirstOrDefault(sp => sp.Id == id);

                if (personnel == null)
                {
                    return Json(new { success = false, message = "Kayýt bulunamadý." }, JsonRequestBehavior.AllowGet);
                }

                // Manually load ServiceConfiguration
                var serviceConfig = db.ServiceConfigurations.FirstOrDefault(sc => sc.ServiceCode == personnel.ServiceCode);

                var result = new
                {
                    personnel.Id,
                    personnel.UserId,
                    FirstName = personnel.User.Name,
                    LastName = personnel.User.Surname,
                    Username = personnel.User.Username,
                    Service = personnel.ServiceCode,
                    ServiceName = serviceConfig?.ServiceName ?? personnel.ServiceCode,
                    personnel.ShiftType,
                    personnel.Status
                };

                return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: ServicePersonnel/GetAvailableUsers
        [DynamicAuthorize(Permission = "ServiceManagement.Personnel", Action = "View")]
        public JsonResult GetAvailableUsers()
        {
            try
            {
                // Tüm kullanýcýlarý getir (bir kiþi hem mesai içi hem mesai sonrasý olabilir)
                var users = db.Users
                    .OrderBy(u => u.Name)
                    .Select(u => new
                    {
                        u.Id,
                        FullName = u.Name + " " + u.Surname,
                        u.Username,
                        u.Department
                    })
                    .ToList();

                return Json(new { success = true, data = users }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: ServicePersonnel/GetAvailableServices
        [DynamicAuthorize(Permission = "ServiceManagement.Personnel", Action = "View")]
        public JsonResult GetAvailableServices()
        {
            try
            {
                var services = db.ServiceConfigurations
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.SortOrder)
                    .ThenBy(s => s.ServiceName)
                    .Select(s => new
                    {
                        s.ServiceCode,
                        s.ServiceName,
                        s.Description,
                        s.ColorCode,
                        s.IconClass
                    })
                    .ToList();

                return Json(new { success = true, data = services }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: ServicePersonnel/Create
        [HttpPost]
        [DynamicAuthorize(Permission = "ServiceManagement.Personnel", Action = "Create")]
        public JsonResult Create(ServicePersonnel model)
        {
            try
            {
                // Check if user is already assigned
                var existingAssignment = db.ServicePersonnels.Any(sp => sp.UserId == model.UserId);
                if (existingAssignment)
                {
                    return Json(new { success = false, message = "Bu kullanýcý zaten servis personeli olarak atanmýþ." });
                }

                // Validate user exists
                var user = db.Users.Find(model.UserId);
                if (user == null)
                {
                    return Json(new { success = false, message = "Geçerli bir kullanýcý seçiniz." });
                }

                // Validate service exists
                var service = db.ServiceConfigurations.FirstOrDefault(s => s.ServiceCode == model.ServiceCode);
                if (service == null)
                {
                    return Json(new { success = false, message = "Geçerli bir servis seçiniz." });
                }

                if (ModelState.IsValid)
                {
                    model.CreatedDate = DateTime.Now;
                    db.ServicePersonnels.Add(model);
                    db.SaveChanges();

                    var result = new
                    {
                        model.Id,
                        model.UserId,
                        FirstName = user.Name,
                        LastName = user.Surname,
                        Username = user.Username,
                        Service = model.ServiceCode,
                        ServiceName = service.ServiceName,
                        model.ShiftType,
                        model.Status,
                        model.CreatedDate
                    };

                    return Json(new { success = true, message = "Kayýt baþarýyla oluþturuldu.", data = result });
                }

                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = "Doðrulama hatasý.", errors = errors });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: ServicePersonnel/Edit
        [HttpPost]
        [DynamicAuthorize(Permission = "ServiceManagement.Personnel", Action = "Edit")]
        public JsonResult Edit(ServicePersonnel model)
        {
            try
            {
                var existing = db.ServicePersonnels.Include(sp => sp.User).FirstOrDefault(sp => sp.Id == model.Id);
                if (existing == null)
                {
                    return Json(new { success = false, message = "Kayýt bulunamadý." });
                }

                // Check if user is already assigned to another service personnel record
                var existingAssignment = db.ServicePersonnels.Any(sp => sp.UserId == model.UserId && sp.Id != model.Id);
                if (existingAssignment)
                {
                    return Json(new { success = false, message = "Bu kullanýcý baþka bir servis personeli kaydýnda zaten atanmýþ." });
                }

                // Validate user exists
                var user = db.Users.Find(model.UserId);
                if (user == null)
                {
                    return Json(new { success = false, message = "Geçerli bir kullanýcý seçiniz." });
                }

                // Validate service exists
                var service = db.ServiceConfigurations.FirstOrDefault(s => s.ServiceCode == model.ServiceCode);
                if (service == null)
                {
                    return Json(new { success = false, message = "Geçerli bir servis seçiniz." });
                }

                if (ModelState.IsValid)
                {
                    existing.UserId = model.UserId;
                    existing.ServiceCode = model.ServiceCode;
                    existing.ShiftType = model.ShiftType;
                    existing.Status = model.Status;
                    existing.UpdatedDate = DateTime.Now;

                    db.Entry(existing).State = EntityState.Modified;
                    db.SaveChanges();

                    var result = new
                    {
                        existing.Id,
                        existing.UserId,
                        FirstName = user.Name,
                        LastName = user.Surname,
                        Username = user.Username,
                        Service = existing.ServiceCode,
                        ServiceName = service.ServiceName,
                        existing.ShiftType,
                        existing.Status,
                        existing.UpdatedDate
                    };

                    return Json(new { success = true, message = "Kayýt baþarýyla güncellendi.", data = result });
                }

                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = "Doðrulama hatasý.", errors = errors });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: ServicePersonnel/Delete/5
        [HttpPost]
        [DynamicAuthorize(Permission = "ServiceManagement.Personnel", Action = "Delete")]
        public JsonResult Delete(int id)
        {
            try
            {
                var personnel = db.ServicePersonnels.Find(id);
                if (personnel == null)
                {
                    return Json(new { success = false, message = "Kayýt bulunamadý." });
                }

                db.ServicePersonnels.Remove(personnel);
                db.SaveChanges();

                return Json(new { success = true, message = "Kayýt baþarýyla silindi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: ServicePersonnel/GetStats
        [DynamicAuthorize(Permission = "ServiceManagement.Personnel", Action = "View")]
        public JsonResult GetStats()
        {
            try
            {
                var allPersonnel = db.ServicePersonnels.Include(sp => sp.User).ToList();
                
                var stats = new
                {
                    totalPersonnel = allPersonnel.Count,
                    activeServices = allPersonnel.Select(p => p.ServiceCode).Distinct().Count(),
                    pendingCount = allPersonnel.Count(p => p.Status == "Beklemede"),
                    monthlyAdded = allPersonnel.Count(p => p.CreatedDate.Month == DateTime.Now.Month && p.CreatedDate.Year == DateTime.Now.Year)
                };

                return Json(new { success = true, data = stats }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: ServicePersonnel/Search
        [DynamicAuthorize(Permission = "ServiceManagement.Personnel", Action = "View")]
        public JsonResult Search(string term = "", string service = "", string status = "", string shiftType = "")
        {
            try
            {
                var query = db.ServicePersonnels.Include(sp => sp.User).AsQueryable();

                if (!string.IsNullOrEmpty(term))
                {
                    term = term.ToLower();
                    query = query.Where(p => p.User.Name.ToLower().Contains(term) || 
                                           p.User.Surname.ToLower().Contains(term) ||
                                           p.User.Username.ToLower().Contains(term) ||
                                           p.ServiceCode.ToLower().Contains(term));
                }

                if (!string.IsNullOrEmpty(service))
                {
                    query = query.Where(p => p.ServiceCode == service);
                }

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(p => p.Status == status);
                }

                if (!string.IsNullOrEmpty(shiftType))
                {
                    query = query.Where(p => p.ShiftType == shiftType);
                }

                var personnel = query.OrderBy(p => p.User.Name).ToList();

                // Manually load ServiceConfiguration for search results
                var serviceCodes = personnel.Select(p => p.ServiceCode).Distinct().ToList();
                var serviceConfigs = db.ServiceConfigurations
                    .Where(sc => serviceCodes.Contains(sc.ServiceCode))
                    .ToList();

                var results = personnel.Select(p => {
                    var serviceConfig = serviceConfigs.FirstOrDefault(sc => sc.ServiceCode == p.ServiceCode);
                    return new
                    {
                        p.Id,
                        p.UserId,
                        FirstName = p.User.Name,
                        LastName = p.User.Surname,
                        Username = p.User.Username,
                        Service = p.ServiceCode,
                        ServiceName = serviceConfig?.ServiceName ?? p.ServiceCode,
                        p.ShiftType,
                        p.Status,
                        p.CreatedDate
                    };
                });

                return Json(new { success = true, data = results }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: ServicePersonnel/CreateSampleData - For testing purposes (make it public)
        public ActionResult CreateSampleData()
        {
            try
            {
                // Only create sample data if no service personnel exist
                if (db.ServicePersonnels.Any())
                {
                    return Json(new { success = false, message = "Zaten servis personeli kayýtlarý mevcut." }, JsonRequestBehavior.AllowGet);
                }

                // Ensure we have service configurations first
                if (!db.ServiceConfigurations.Any())
                {
                    return Json(new { success = false, message = "Önce servis konfigürasyonlarý oluþturun." }, JsonRequestBehavior.AllowGet);
                }

                // Get some users from the database
                var users = db.Users.Take(6).ToList();
                if (users.Count < 6)
                {
                    return Json(new { success = false, message = "En az 6 kullanýcý gerekli. Lütfen önce kullanýcý kayýtlarý oluþturun." }, JsonRequestBehavior.AllowGet);
                }

                // Get available service codes
                var serviceCodes = db.ServiceConfigurations.Where(s => s.IsActive).Select(s => s.ServiceCode).ToArray();
                if (serviceCodes.Length == 0)
                {
                    return Json(new { success = false, message = "Aktif servis konfigürasyonu bulunamadý." }, JsonRequestBehavior.AllowGet);
                }

                var samplePersonnel = new List<ServicePersonnel>();
                for (int i = 0; i < Math.Min(users.Count, serviceCodes.Length); i++)
                {
                    samplePersonnel.Add(new ServicePersonnel
                    {
                        UserId = users[i].Id,
                        ServiceCode = serviceCodes[i % serviceCodes.Length],
                        ShiftType = i % 2 == 0 ? "Mesai Ýçi" : "Mesai Sonrasý",
                        Status = i == 2 ? "Beklemede" : "Aktif",
                        CreatedDate = DateTime.Now
                    });
                }

                db.ServicePersonnels.AddRange(samplePersonnel);
                db.SaveChanges();

                return Json(new { success = true, message = "Örnek veriler baþarýyla oluþturuldu." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: ServicePersonnel/RecreateSampleData - Recreate sample data
        public JsonResult RecreateSampleData()
        {
            try
            {
                // Delete existing sample data
                var existingPersonnel = db.ServicePersonnels.ToList();
                db.ServicePersonnels.RemoveRange(existingPersonnel);
                db.SaveChanges();

                // Ensure we have service configurations first
                if (!db.ServiceConfigurations.Any())
                {
                    return Json(new { success = false, message = "Önce servis konfigürasyonlarý oluþturun." }, JsonRequestBehavior.AllowGet);
                }

                // Get some users from the database
                var users = db.Users.Take(6).ToList();
                if (users.Count < 3)
                {
                    return Json(new { success = false, message = "En az 3 kullanýcý gerekli. Lütfen önce kullanýcý kayýtlarý oluþturun." }, JsonRequestBehavior.AllowGet);
                }

                // Get available service codes
                var serviceCodes = db.ServiceConfigurations.Where(s => s.IsActive).Select(s => s.ServiceCode).ToArray();
                if (serviceCodes.Length == 0)
                {
                    return Json(new { success = false, message = "Aktif servis konfigürasyonu bulunamadý." }, JsonRequestBehavior.AllowGet);
                }

                var samplePersonnel = new List<ServicePersonnel>();
                for (int i = 0; i < Math.Min(users.Count, 8); i++)
                {
                    samplePersonnel.Add(new ServicePersonnel
                    {
                        UserId = users[i % users.Count].Id,
                        ServiceCode = serviceCodes[i % serviceCodes.Length],
                        ShiftType = i % 2 == 0 ? "Mesai Ýçi" : "Mesai Sonrasý",
                        Status = i == 2 ? "Beklemede" : "Aktif",
                        CreatedDate = DateTime.Now
                    });
                }

                db.ServicePersonnels.AddRange(samplePersonnel);
                db.SaveChanges();

                return Json(new { success = true, message = $"Örnek veriler yeniden oluþturuldu. {samplePersonnel.Count} kayýt eklendi." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: ServicePersonnel/GetPublicData - Public read-only access for display purposes
        public JsonResult GetPublicData()
        {
            try
            {
                var personnel = db.ServicePersonnels
                    .Include(sp => sp.User)
                    .Where(sp => sp.Status == "Aktif") // Only show active personnel for public view
                    .OrderBy(p => p.User.Name)
                    .ToList();

                // Manually load ServiceConfiguration for each personnel
                var serviceCodes = personnel.Select(p => p.ServiceCode).Distinct().ToList();
                var serviceConfigs = db.ServiceConfigurations
                    .Where(sc => serviceCodes.Contains(sc.ServiceCode) && sc.IsActive)
                    .ToList();

                var result = personnel.Select(p => {
                    var serviceConfig = serviceConfigs.FirstOrDefault(sc => sc.ServiceCode == p.ServiceCode);
                    return new
                    {
                        p.Id,
                        FirstName = p.User.Name,
                        LastName = p.User.Surname,
                        Username = p.User.Username,
                        Service = p.ServiceCode,
                        ServiceName = serviceConfig?.ServiceName ?? p.ServiceCode,
                        p.ShiftType,
                        p.Status,
                        Department = p.User.Department
                    };
                });

                return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Veri yüklenemedi." }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: ServicePersonnel/GetPublicServices - Public service list
        public JsonResult GetPublicServices()
        {
            try
            {
                var services = db.ServiceConfigurations
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.SortOrder)
                    .ThenBy(s => s.ServiceName)
                    .Select(s => new
                    {
                        s.ServiceCode,
                        s.ServiceName,
                        s.Description,
                        s.ColorCode,
                        s.IconClass
                    })
                    .ToList();

                return Json(new { success = true, data = services }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Servis bilgileri yüklenemedi." }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: ServicePersonnel/GetPublicStats - Public statistics
        public JsonResult GetPublicStats()
        {
            try
            {
                var activePersonnel = db.ServicePersonnels
                    .Include(sp => sp.User)
                    .Where(sp => sp.Status == "Aktif")
                    .ToList();
                
                var stats = new
                {
                    totalPersonnel = activePersonnel.Count,
                    activeServices = activePersonnel.Select(p => p.ServiceCode).Distinct().Count(),
                    workHoursCount = activePersonnel.Count(p => p.ShiftType == "Mesai Ýçi"),
                    afterHoursCount = activePersonnel.Count(p => p.ShiftType == "Mesai Sonrasý")
                };

                return Json(new { success = true, data = stats }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Ýstatistik bilgileri yüklenemedi." }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: ServicePersonnel/SearchPublic - Public search functionality
        public JsonResult SearchPublic(string term = "", string service = "", string shiftType = "")
        {
            try
            {
                var query = db.ServicePersonnels
                    .Include(sp => sp.User)
                    .Where(sp => sp.Status == "Aktif") // Only active personnel for public view
                    .AsQueryable();

                if (!string.IsNullOrEmpty(term))
                {
                    term = term.ToLower();
                    query = query.Where(p => p.User.Name.ToLower().Contains(term) || 
                                           p.User.Surname.ToLower().Contains(term) ||
                                           p.User.Username.ToLower().Contains(term) ||
                                           p.ServiceCode.ToLower().Contains(term));
                }

                if (!string.IsNullOrEmpty(service))
                {
                    query = query.Where(p => p.ServiceCode == service);
                }

                if (!string.IsNullOrEmpty(shiftType))
                {
                    query = query.Where(p => p.ShiftType == shiftType);
                }

                var personnel = query.OrderBy(p => p.User.Name).ToList();

                // Manually load ServiceConfiguration for search results
                var serviceCodes = personnel.Select(p => p.ServiceCode).Distinct().ToList();
                var serviceConfigs = db.ServiceConfigurations
                    .Where(sc => serviceCodes.Contains(sc.ServiceCode) && sc.IsActive)
                    .ToList();

                var results = personnel.Select(p => {
                    var serviceConfig = serviceConfigs.FirstOrDefault(sc => sc.ServiceCode == p.ServiceCode);
                    return new
                    {
                        p.Id,
                        FirstName = p.User.Name,
                        LastName = p.User.Surname,
                        Username = p.User.Username,
                        Service = p.ServiceCode,
                        ServiceName = serviceConfig?.ServiceName ?? p.ServiceCode,
                        p.ShiftType,
                        p.Status,
                        Department = p.User.Department
                    };
                });

                return Json(new { success = true, data = results }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Arama yapýlamadý." }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: ServicePersonnel/DebugInfo - Debug information endpoint
        public JsonResult DebugInfo()
        {
            try
            {
                var totalUsers = db.Users.Count();
                var totalServiceConfigs = db.ServiceConfigurations.Count();
                var totalServicePersonnel = db.ServicePersonnels.Count();
                var activeServicePersonnel = db.ServicePersonnels.Count(sp => sp.Status == "Aktif");
                
                var sampleUsers = db.Users.Take(3).Select(u => new { u.Id, u.Username, u.Name, u.Surname }).ToList();
                var sampleServices = db.ServiceConfigurations.Take(3).Select(s => new { s.ServiceCode, s.ServiceName, s.IsActive }).ToList();
                var samplePersonnel = db.ServicePersonnels.Include(sp => sp.User).Take(5).Select(sp => new { 
                    sp.Id, 
                    sp.ServiceCode, 
                    sp.ShiftType, 
                    sp.Status,
                    UserName = sp.User.Username,
                    FirstName = sp.User.Name,
                    LastName = sp.User.Surname
                }).ToList();

                // Shift type istatistikleri
                var shiftStats = new
                {
                    MesaiIci = db.ServicePersonnels.Count(sp => sp.ShiftType == "Mesai Ýçi"),
                    MesaiSonrasi = db.ServicePersonnels.Count(sp => sp.ShiftType == "Mesai Sonrasý"),
                    AllShiftTypes = db.ServicePersonnels.Select(sp => sp.ShiftType).Distinct().ToList()
                };

                var debug = new
                {
                    TotalUsers = totalUsers,
                    TotalServiceConfigs = totalServiceConfigs,
                    TotalServicePersonnel = totalServicePersonnel,
                    ActiveServicePersonnel = activeServicePersonnel,
                    SampleUsers = sampleUsers,
                    SampleServices = sampleServices,
                    SamplePersonnel = samplePersonnel,
                    ShiftStatistics = shiftStats,
                    DatabaseConnection = db.Database.Connection.State.ToString()
                };

                return Json(new { success = true, data = debug }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, stackTrace = ex.StackTrace }, JsonRequestBehavior.AllowGet);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}