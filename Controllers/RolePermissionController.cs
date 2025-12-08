using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MZDNETWORK.Attributes;
using MZDNETWORK.Data;
using MZDNETWORK.Models;
using MZDNETWORK.Helpers;
using Newtonsoft.Json;
using System.Web;

namespace MZDNETWORK.Controllers
{
    /// <summary>
    /// Role-Permission Matrix Management Controller
    /// Rol ve permission iliÅŸkilerinin yÃ¶netimi iÃ§in matrix UI
    /// </summary>
    [DynamicAuthorize(Permission = "RolYonetimi.RolYetkileri")]
    public class RolePermissionController : Controller
    {
        private MZDNETWORKContext db = new MZDNETWORKContext();

        /// <summary>
        /// Ana role-permission matrix sayfasÄ±
        /// </summary>
        public ActionResult Index()
        {
            // Cache'i devre dÄ±ÅŸÄ± bÄ±rak
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            
            ViewBag.Title = "Rol-Yetki Matrisi";
            
            // TablolarÄ± kontrol et ve yoksa oluÅŸtur
            EnsureRolePermissionsTableExists();
            EnsurePermissionsExist();
            
            return View();
        }
        
        /// <summary>
        /// Permission'larÄ±n mevcut olduÄŸundan emin ol
        /// </summary>
        private void EnsurePermissionsExist()
        {
            try
            {
                if (!db.PermissionNodes.Any())
                {
                    Data.PermissionSeeder.SeedPermissions(db);
                }
            }
            catch (Exception ex)
            {
                // Log error silently
                System.Diagnostics.Debug.WriteLine($"Permission seeding error: {ex.Message}");
            }
        }

        /// <summary>
        /// RolePermissions tablosunun mevcut olduÄŸundan emin ol
        /// </summary>
        private void EnsureRolePermissionsTableExists()
        {
            try
            {
                // Tabloyu test et
                var testQuery = db.RolePermissions.Take(1).ToList();
                System.Diagnostics.Debug.WriteLine("RolePermissions table exists and is accessible");
            }
            catch (Exception ex)
            {
                // Tablo yoksa oluÅŸtur
                System.Diagnostics.Debug.WriteLine($"RolePermissions table might not exist: {ex.Message}");
                try
                {
                    // Basit SQL ile tabloyu oluÅŸtur - daha uyumlu syntax
                    var createTableSql = @"
                        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='RolePermissions' AND xtype='U')
                        CREATE TABLE [RolePermissions](
                            [Id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
                            [RoleId] int NOT NULL,
                            [PermissionNodeId] int NOT NULL,
                            [CanView] bit NOT NULL DEFAULT 0,
                            [CanCreate] bit NOT NULL DEFAULT 0,
                            [CanEdit] bit NOT NULL DEFAULT 0,
                            [CanDelete] bit NOT NULL DEFAULT 0,
                            [CanManage] bit NOT NULL DEFAULT 0,
                            [CanApprove] bit NOT NULL DEFAULT 0,
                            [CanReject] bit NOT NULL DEFAULT 0,
                            [CanExport] bit NOT NULL DEFAULT 0,
                            [CanImport] bit NOT NULL DEFAULT 0,
                            [IsActive] bit NOT NULL DEFAULT 1,
                            [CreatedAt] datetime NOT NULL DEFAULT GETDATE(),
                            [CreatedBy] nvarchar(max),
                            [UpdatedAt] datetime,
                            [UpdatedBy] nvarchar(max),
                            [Notes] nvarchar(max)
                        )";
                    
                    db.Database.ExecuteSqlCommand(createTableSql);
                    System.Diagnostics.Debug.WriteLine("RolePermissions table created successfully");
                    
                    // Index'leri ekle
                    try
                    {
                        db.Database.ExecuteSqlCommand("CREATE INDEX IX_RolePermissions_RoleId ON RolePermissions(RoleId)");
                        db.Database.ExecuteSqlCommand("CREATE INDEX IX_RolePermissions_PermissionNodeId ON RolePermissions(PermissionNodeId)");
                        System.Diagnostics.Debug.WriteLine("Indexes created successfully");
                    }
                    catch (Exception idxEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Index creation warning: {idxEx.Message}");
                    }
                    
                    // Tekrar test et
                    var testQuery2 = db.RolePermissions.Take(1).ToList();
                    System.Diagnostics.Debug.WriteLine("RolePermissions table creation and test completed successfully");
                }
                catch (Exception createEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to create RolePermissions table: {createEx.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack trace: {createEx.StackTrace}");
                    
                    // Son Ã§are olarak basit bir tablo oluÅŸtur
                    try
                    {
                        var simpleSql = "CREATE TABLE RolePermissions (Id int IDENTITY(1,1) PRIMARY KEY, RoleId int, PermissionNodeId int, CanView bit DEFAULT 0, CanCreate bit DEFAULT 0, CanEdit bit DEFAULT 0, CanDelete bit DEFAULT 0, CanManage bit DEFAULT 0, CanApprove bit DEFAULT 0, CanReject bit DEFAULT 0, CanExport bit DEFAULT 0, CanImport bit DEFAULT 0, IsActive bit DEFAULT 1, CreatedAt datetime DEFAULT GETDATE(), CreatedBy nvarchar(max), UpdatedAt datetime, UpdatedBy nvarchar(max), Notes nvarchar(max))";
                        db.Database.ExecuteSqlCommand(simpleSql);
                        System.Diagnostics.Debug.WriteLine("Simple RolePermissions table created as fallback");
                    }
                    catch (Exception simpleEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Even simple table creation failed: {simpleEx.Message}");
                        throw new Exception($"All RolePermissions table creation attempts failed. Last error: {simpleEx.Message}", simpleEx);
                    }
                }
            }
        }

        /// <summary>
        /// Rol listesini getirir (Matrix iÃ§in)
        /// </summary>
        public JsonResult GetRoles()
        {
            try
            {
                var roles = db.Roles
                    .OrderBy(r => r.Name)
                    .Select(r => new
                    {
                        id = r.Id,
                        name = r.Name,
                        description = r.Description,
                        userCount = db.UserRoles.Count(ur => ur.RoleId == r.Id),
                        permissionCount = db.RolePermissions.Count(rp => rp.RoleId == r.Id && rp.IsActive)
                    })
                    .ToList();

                return Json(roles, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Permission listesini hiyerarÅŸik yapÄ±da getirir
        /// </summary>
        public JsonResult GetPermissions()
        {
            try
            {
                // Permission'larÄ± kontrol et
                var permissionCount = db.PermissionNodes.Count();
                if (permissionCount == 0)
                {
                    // Otomatik seeding yap
                    Data.PermissionSeeder.SeedPermissions(db);
                }

                var permissions = db.PermissionNodes
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.SortOrder)
                    .ThenBy(p => p.Name)
                    .Select(p => new
                    {
                        id = p.Id,
                        name = p.Name,
                        path = p.Path,
                        description = p.Description,
                        parentId = p.ParentId,
                        type = p.Type,
                        icon = p.Icon,
                        hasView = p.HasViewPermission,
                        hasCreate = p.HasCreatePermission,
                        hasEdit = p.HasEditPermission,
                        hasDelete = p.HasDeletePermission,
                        roleCount = db.RolePermissions.Count(rp => rp.PermissionNodeId == p.Id && rp.IsActive)
                    })
                    .ToList();

                // Basit liste dÃ¶ndÃ¼r (hiyerarÅŸi deÄŸil)
                return Json(permissions, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message, stackTrace = ex.StackTrace }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Test endpoint - Rol permissions'Ä± basit ÅŸekilde kontrol et
        /// </summary>
        public JsonResult TestRolePermissions(int roleId)
        {
            try
            {
                return Json(new { 
                    success = true,
                    roleId = roleId,
                    roleExists = db.Roles.Any(r => r.Id == roleId),
                    rolePermissionCount = db.RolePermissions.Count(rp => rp.RoleId == roleId),
                    activeRolePermissionCount = db.RolePermissions.Count(rp => rp.RoleId == roleId && rp.IsActive),
                    totalRolePermissions = db.RolePermissions.Count(),
                    message = "Test baÅŸarÄ±lÄ±"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    error = ex.Message, 
                    roleId = roleId 
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Debug endpoint - Tablo durumunu kontrol et
        /// </summary>
        public JsonResult CheckTableStatus()
        {
            try
            {
                EnsureRolePermissionsTableExists();
                
                var roleCount = db.Roles.Count();
                var permissionCount = db.PermissionNodes.Count();
                var rolePermissionCount = 0;
                
                try
                {
                    rolePermissionCount = db.RolePermissions.Count();
                }
                catch (Exception rpEx)
                {
                    return Json(new { 
                        success = false,
                        error = "RolePermissions table issue: " + rpEx.Message,
                        roleCount = roleCount,
                        permissionCount = permissionCount
                    }, JsonRequestBehavior.AllowGet);
                }
                
                return Json(new { 
                    success = true,
                    roleCount = roleCount,
                    permissionCount = permissionCount,
                    rolePermissionCount = rolePermissionCount,
                    message = "All tables accessible"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Belirli bir rol iÃ§in permission matrix'ini getirir
        /// </summary>
        public JsonResult GetRolePermissions(int? id, int? roleId)
        {
            try
            {
                // Gelen deÄŸer hangisi doluysa onu kullan
                int roleIdValue = roleId ?? id ?? 0;
                if (roleIdValue == 0)
                {
                    return Json(new { error = "Role ID is missing" }, JsonRequestBehavior.AllowGet);
                }
                
                // Response headers
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.Cache.SetNoStore();
                
                // Debug log
                System.Diagnostics.Debug.WriteLine($"GetRolePermissions called for roleId: {roleIdValue}");

                var result = new Dictionary<string, object>();
                
                try
                {
                    // Role ait permissions'larÄ± getir
                    var rolePermissions = db.RolePermissions
                        .Where(rp => rp.RoleId == roleIdValue && rp.IsActive)
                        .ToList();

                    System.Diagnostics.Debug.WriteLine($"Found {rolePermissions.Count} permissions for role {roleIdValue}");

                    // Dictionary formatÄ±na Ã§evir
                    result = rolePermissions.ToDictionary(
                        rp => rp.PermissionNodeId.ToString(),
                        rp => (object)new
                        {
                            canView = rp.CanView,
                            canCreate = rp.CanCreate,
                            canEdit = rp.CanEdit,
                            canDelete = rp.CanDelete,
                            canManage = rp.CanManage,
                            canApprove = rp.CanApprove,
                            canReject = rp.CanReject,
                            canExport = rp.CanExport,
                            canImport = rp.CanImport
                        }
                    );

                }
                catch (Exception queryEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Query Error: {queryEx.Message}");
                    // Tablo yoksa boÅŸ result dÃ¶ndÃ¼r
                    result = new Dictionary<string, object>();
                }

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // DetaylÄ± hata bilgisi dÃ¶ndÃ¼r
                System.Diagnostics.Debug.WriteLine($"GetRolePermissions Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                
                return Json(new { 
                    error = ex.Message, 
                    stackTrace = ex.StackTrace,
                    roleId = id ?? roleId 
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Tam permission matrix'ini getirir (TÃ¼m roller ve permission'lar)
        /// </summary>
        public JsonResult GetFullMatrix()
        {
            try
            {
                var roles = db.Roles.OrderBy(r => r.Name).ToList();
                var permissions = db.PermissionNodes.Where(p => p.IsActive).OrderBy(p => p.SortOrder).ToList();
                
                var matrix = new List<object>();

                foreach (var permission in permissions)
                {
                    var row = new
                    {
                        permissionId = permission.Id,
                        permissionName = permission.Name,
                        permissionPath = permission.Path,
                        permissionType = permission.Type,
                        permissionIcon = permission.Icon,
                        parentId = permission.ParentId,
                        roles = roles.Select(role =>
                        {
                            var rolePermission = db.RolePermissions
                                .FirstOrDefault(rp => rp.RoleId == role.Id && rp.PermissionNodeId == permission.Id && rp.IsActive);

                            return new
                            {
                                roleId = role.Id,
                                roleName = role.Name,
                                hasPermission = rolePermission != null,
                                canView = rolePermission?.CanView ?? false,
                                canCreate = rolePermission?.CanCreate ?? false,
                                canEdit = rolePermission?.CanEdit ?? false,
                                canDelete = rolePermission?.CanDelete ?? false,
                                canManage = rolePermission?.CanManage ?? false,
                                canApprove = rolePermission?.CanApprove ?? false,
                                canReject = rolePermission?.CanReject ?? false,
                                canExport = rolePermission?.CanExport ?? false,
                                canImport = rolePermission?.CanImport ?? false
                            };
                        }).ToList()
                    };
                    matrix.Add(row);
                }

                return Json(new { roles = roles, matrix = matrix }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Belirli bir role permission atar veya gÃ¼nceller
        /// </summary>
        [HttpPost]
        [DynamicAuthorize(Permission = "RolYonetimi.RolYetkileri", Action = "Edit")]
        public JsonResult AssignPermission(RolePermissionAssignModel model)
        {
            try
            {
                var existingRolePermission = db.RolePermissions
                    .FirstOrDefault(rp => rp.RoleId == model.RoleId && rp.PermissionNodeId == model.PermissionNodeId);

                if (existingRolePermission == null)
                {
                    // Yeni permission assignment
                    var rolePermission = new RolePermission
                    {
                        RoleId = model.RoleId,
                        PermissionNodeId = model.PermissionNodeId,
                        CanView = model.CanView,
                        CanCreate = model.CanCreate,
                        CanEdit = model.CanEdit,
                        CanDelete = model.CanDelete,
                        CanManage = model.CanManage,
                        CanApprove = model.CanApprove,
                        CanReject = model.CanReject,
                        CanExport = model.CanExport,
                        CanImport = model.CanImport,
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        CreatedBy = User.Identity.Name,
                        Notes = model.Notes
                    };

                    db.RolePermissions.Add(rolePermission);
                }
                else
                {
                    // Mevcut permission'Ä± gÃ¼ncelle
                    existingRolePermission.CanView = model.CanView;
                    existingRolePermission.CanCreate = model.CanCreate;
                    existingRolePermission.CanEdit = model.CanEdit;
                    existingRolePermission.CanDelete = model.CanDelete;
                    existingRolePermission.CanManage = model.CanManage;
                    existingRolePermission.CanApprove = model.CanApprove;
                    existingRolePermission.CanReject = model.CanReject;
                    existingRolePermission.CanExport = model.CanExport;
                    existingRolePermission.CanImport = model.CanImport;
                    existingRolePermission.IsActive = true;
                    existingRolePermission.UpdatedAt = DateTime.Now;
                    existingRolePermission.UpdatedBy = User.Identity.Name;
                    existingRolePermission.Notes = model.Notes;
                }

                db.SaveChanges();

                // Cache'i temizle
                DynamicPermissionHelper.ClearPermissionCache();

                return Json(new { success = true, message = "Permission baÅŸarÄ±yla atandÄ±" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Belirli bir rol-permission iliÅŸkisini kaldÄ±rÄ±r
        /// </summary>
        [HttpPost]
        [DynamicAuthorize(Permission = "RolYonetimi.RolYetkileri", Action = "Edit")]
        public JsonResult RemovePermission(int roleId, int permissionNodeId)
        {
            try
            {
                var rolePermission = db.RolePermissions
                    .FirstOrDefault(rp => rp.RoleId == roleId && rp.PermissionNodeId == permissionNodeId);

                if (rolePermission != null)
                {
                    rolePermission.IsActive = false;
                    rolePermission.UpdatedAt = DateTime.Now;
                    rolePermission.UpdatedBy = User.Identity.Name;
                    db.SaveChanges();

                    // Cache'i temizle
                    DynamicPermissionHelper.ClearPermissionCache();

                    return Json(new { success = true, message = "Permission kaldÄ±rÄ±ldÄ±" });
                }

                return Json(new { success = false, message = "Permission bulunamadÄ±" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Toplu permission atama (Bulk assign)
        /// </summary>
        [HttpPost]
        [DynamicAuthorize(Permission = "RolYonetimi.RolYetkileri", Action = "Manage")]
        public JsonResult BulkAssignPermissions(BulkPermissionAssignModel model)
        {
            try
            {
                foreach (var assignment in model.Assignments)
                {
                    var existingRolePermission = db.RolePermissions
                        .FirstOrDefault(rp => rp.RoleId == assignment.RoleId && rp.PermissionNodeId == assignment.PermissionNodeId);

                    if (existingRolePermission == null)
                    {
                        var rolePermission = new RolePermission
                        {
                            RoleId = assignment.RoleId,
                            PermissionNodeId = assignment.PermissionNodeId,
                            CanView = assignment.CanView,
                            CanCreate = assignment.CanCreate,
                            CanEdit = assignment.CanEdit,
                            CanDelete = assignment.CanDelete,
                            CanManage = assignment.CanManage,
                            CanApprove = assignment.CanApprove,
                            CanReject = assignment.CanReject,
                            CanExport = assignment.CanExport,
                            CanImport = assignment.CanImport,
                            IsActive = true,
                            CreatedAt = DateTime.Now,
                            CreatedBy = User.Identity.Name
                        };

                        db.RolePermissions.Add(rolePermission);
                    }
                    else
                    {
                        existingRolePermission.CanView = assignment.CanView;
                        existingRolePermission.CanCreate = assignment.CanCreate;
                        existingRolePermission.CanEdit = assignment.CanEdit;
                        existingRolePermission.CanDelete = assignment.CanDelete;
                        existingRolePermission.CanManage = assignment.CanManage;
                        existingRolePermission.CanApprove = assignment.CanApprove;
                        existingRolePermission.CanReject = assignment.CanReject;
                        existingRolePermission.CanExport = assignment.CanExport;
                        existingRolePermission.CanImport = assignment.CanImport;
                        existingRolePermission.IsActive = true;
                        existingRolePermission.UpdatedAt = DateTime.Now;
                        existingRolePermission.UpdatedBy = User.Identity.Name;
                    }
                }

                db.SaveChanges();

                // Cache'i temizle
                DynamicPermissionHelper.ClearPermissionCache();

                return Json(new { success = true, message = $"{model.Assignments.Count} permission atama baÅŸarÄ±yla tamamlandÄ±" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Role template uygular (HazÄ±r rol ÅŸablonu)
        /// </summary>
        [HttpPost]
        [DynamicAuthorize(Permission = "RolYonetimi.RolYetkileri", Action = "Manage")]
        public JsonResult ApplyRoleTemplate(int roleId, string templateName)
        {
            try
            {
                var templates = GetRoleTemplates();
                if (!templates.ContainsKey(templateName))
                {
                    return Json(new { success = false, message = "GeÃ§ersiz ÅŸablon" });
                }

                var template = templates[templateName];
                
                // Mevcut permission'larÄ± temizle
                var existingPermissions = db.RolePermissions
                    .Where(rp => rp.RoleId == roleId)
                    .ToList();

                foreach (var perm in existingPermissions)
                {
                    perm.IsActive = false;
                    perm.UpdatedAt = DateTime.Now;
                    perm.UpdatedBy = User.Identity.Name;
                }

                // Template permission'larÄ±nÄ± uygula
                foreach (var permissionPath in template.PermissionPaths)
                {
                    var permissionNode = db.PermissionNodes
                        .FirstOrDefault(p => p.Path == permissionPath && p.IsActive);

                    if (permissionNode != null)
                    {
                        var rolePermission = new RolePermission
                        {
                            RoleId = roleId,
                            PermissionNodeId = permissionNode.Id,
                            CanView = template.DefaultPermissions.CanView,
                            CanCreate = template.DefaultPermissions.CanCreate,
                            CanEdit = template.DefaultPermissions.CanEdit,
                            CanDelete = template.DefaultPermissions.CanDelete,
                            CanManage = template.DefaultPermissions.CanManage,
                            CanApprove = template.DefaultPermissions.CanApprove,
                            CanReject = template.DefaultPermissions.CanReject,
                            CanExport = template.DefaultPermissions.CanExport,
                            CanImport = template.DefaultPermissions.CanImport,
                            IsActive = true,
                            CreatedAt = DateTime.Now,
                            CreatedBy = User.Identity.Name,
                            Notes = $"Template '{templateName}' uygulandÄ±"
                        };

                        db.RolePermissions.Add(rolePermission);
                    }
                }

                db.SaveChanges();

                // Cache'i temizle
                DynamicPermissionHelper.ClearPermissionCache();

                return Json(new { success = true, message = $"{templateName} ÅŸablonu baÅŸarÄ±yla uygulandÄ±" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Rol kaydeder (Yeni oluÅŸtur veya gÃ¼ncelle)
        /// </summary>
        [HttpPost]
        [DynamicAuthorize(Permission = "RolYonetimi.RolYetkileri", Action = "Edit")]
        public JsonResult SaveRole(Role roleModel)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roleModel.Name))
                {
                    return Json(new { success = false, message = "Rol adÄ± boÅŸ olamaz" });
                }

                // AynÄ± isimde rol var mÄ± kontrol et
                var existingRole = db.Roles.FirstOrDefault(r => r.Name == roleModel.Name && r.Id != roleModel.Id);
                if (existingRole != null)
                {
                    return Json(new { success = false, message = "Bu isimde bir rol zaten mevcut" });
                }

                if (roleModel.Id == 0)
                {
                    // Yeni rol oluÅŸtur
                    var newRole = new Role
                    {
                        Name = roleModel.Name,
                        Description = roleModel.Description,
                        IsActive = true,
                        CreatedDate = DateTime.Now,
                        CreatedBy = 1 // Current user ID
                    };
                    db.Roles.Add(newRole);
                }
                else
                {
                    // Mevcut rolÃ¼ gÃ¼ncelle
                    var role = db.Roles.Find(roleModel.Id);
                    if (role == null)
                    {
                        return Json(new { success = false, message = "Rol bulunamadÄ±" });
                    }

                    role.Name = roleModel.Name;
                    role.Description = roleModel.Description;
                    role.ModifiedDate = DateTime.Now;
                    role.ModifiedBy = 1; // Current user ID
                }

                db.SaveChanges();

                // Cache'i temizle
                DynamicPermissionHelper.ClearPermissionCache();

                return Json(new { success = true, message = "Rol baÅŸarÄ±yla kaydedildi" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Rol siler
        /// </summary>
        [HttpPost]
        [DynamicAuthorize(Permission = "RolYonetimi.RolYetkileri", Action = "Delete")]
        public JsonResult DeleteRole(int id)
        {
            try
            {
                var role = db.Roles.Find(id);
                if (role == null)
                {
                    return Json(new { success = false, message = "Rol bulunamadÄ±" });
                }

                // RolÃ¼n kullanÄ±cÄ±larÄ± var mÄ± kontrol et
                var userCount = db.UserRoles.Count(ur => ur.RoleId == id);
                if (userCount > 0)
                {
                    return Json(new { success = false, message = $"Bu rol {userCount} kullanÄ±cÄ± tarafÄ±ndan kullanÄ±lÄ±yor. Ã–nce rol atamalarÄ±nÄ± kaldÄ±rÄ±n." });
                }

                // Rol permission'larÄ±nÄ± sil
                var rolePermissions = db.RolePermissions.Where(rp => rp.RoleId == id).ToList();
                db.RolePermissions.RemoveRange(rolePermissions);

                // RolÃ¼ sil
                db.Roles.Remove(role);
                db.SaveChanges();

                // Cache'i temizle
                DynamicPermissionHelper.ClearPermissionCache();

                return Json(new { success = true, message = "Rol baÅŸarÄ±yla silindi" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Matrix istatistiklerini getirir
        /// </summary>
        public JsonResult GetMatrixStatistics()
        {
            try
            {
                var stats = new
                {
                    totalRoles = db.Roles.Count(),
                    totalPermissions = db.PermissionNodes.Count(p => p.IsActive),
                    activeUsers = db.Users.Count(),
                    roleAssignments = db.UserRoles.Count(),
                    totalAssignments = db.RolePermissions.Count(rp => rp.IsActive),
                    rolesWithoutPermissions = db.Roles.Count(r => !db.RolePermissions.Any(rp => rp.RoleId == r.Id && rp.IsActive)),
                    permissionsWithoutRoles = db.PermissionNodes.Count(p => p.IsActive && !db.RolePermissions.Any(rp => rp.PermissionNodeId == p.Id && rp.IsActive)),
                    mostAssignedPermissions = db.RolePermissions
                        .Where(rp => rp.IsActive)
                        .GroupBy(rp => rp.PermissionNode.Name)
                        .Select(g => new { permission = g.Key, count = g.Count() })
                        .OrderByDescending(x => x.count)
                        .Take(5)
                        .ToList(),
                    rolesWithMostPermissions = db.RolePermissions
                        .Where(rp => rp.IsActive)
                        .GroupBy(rp => rp.Role.Name)
                        .Select(g => new { role = g.Key, count = g.Count() })
                        .OrderByDescending(x => x.count)
                        .Take(5)
                        .ToList()
                };

                return Json(stats, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Permission hiyerarÅŸisi oluÅŸturur
        /// </summary>
        private List<object> BuildPermissionHierarchy(dynamic permissions)
        {
            var permissionList = ((IEnumerable<dynamic>)permissions).ToList();
            var hierarchy = new List<object>();

            var rootPermissions = permissionList.Where(p => ((dynamic)p).parentId == null).ToList();

            foreach (var root in rootPermissions)
            {
                var item = new
                {
                    permission = root,
                    children = GetChildren(((dynamic)root).id, permissionList)
                };
                hierarchy.Add(item);
            }

            return hierarchy;
        }

        /// <summary>
        /// Alt permission'larÄ± getirir (Recursive)
        /// </summary>
        private List<object> GetChildren(int parentId, List<dynamic> allPermissions)
        {
            var children = new List<object>();
            var childPermissions = allPermissions.Where(p => ((dynamic)p).parentId == parentId).ToList();

            foreach (var child in childPermissions)
            {
                var item = new
                {
                    permission = child,
                    children = GetChildren(((dynamic)child).id, allPermissions)
                };
                children.Add(item);
            }

            return children;
        }

        /// <summary>
        /// HazÄ±r rol ÅŸablonlarÄ±nÄ± getirir
        /// </summary>
        private Dictionary<string, RoleTemplate> GetRoleTemplates()
        {
            return new Dictionary<string, RoleTemplate>
            {
                ["BasicUser"] = new RoleTemplate
                {
                    Name = "Temel KullanÄ±cÄ±",
                    Description = "Sadece gÃ¶rÃ¼ntÃ¼leme yetkisi",
                    PermissionPaths = new[] { "KullaniciYonetimi.View", "Operasyon.Sohbet.View", "Documentation.View" },
                    DefaultPermissions = new PermissionSet { CanView = true }
                },
                ["Manager"] = new RoleTemplate
                {
                    Name = "YÃ¶netici",
                    Description = "Temel yÃ¶netim yetkiler",
                    PermissionPaths = new[] { "KullaniciYonetimi.View", "KullaniciYonetimi.Edit", "InsanKaynaklari.LeaveRequests.Approve", "Operasyon.ToplantiOdasi.Manage" },
                    DefaultPermissions = new PermissionSet { CanView = true, CanEdit = true, CanApprove = true }
                },
                ["HRManager"] = new RoleTemplate
                {
                    Name = "Ä°K YÃ¶neticisi",
                    Description = "Ä°nsan kaynaklarÄ± yÃ¶netimi",
                    PermissionPaths = new[] { "InsanKaynaklari.LeaveRequests.Manage", "InsanKaynaklari.Performance", "InsanKaynaklari.Duyurular", "KullaniciYonetimi.View" },
                    DefaultPermissions = new PermissionSet { CanView = true, CanCreate = true, CanEdit = true, CanApprove = true }
                },
                ["ITManager"] = new RoleTemplate
                {
                    Name = "BT YÃ¶neticisi",
                    Description = "Bilgi iÅŸlem yÃ¶netimi",
                    PermissionPaths = new[] { "BilgiIslem.Settings", "BilgiIslem.Monitoring", "SistemYonetimi.Configuration", "KullaniciYonetimi.Manage" },
                    DefaultPermissions = new PermissionSet { CanView = true, CanCreate = true, CanEdit = true, CanManage = true }
                }
            };
        }

        /// <summary>
        /// Debug JSON endpoint - Role ve Permission sayÄ±larÄ±nÄ± kontrol et
        /// </summary>
        public JsonResult Debug()
        {
            try
            {
                var permissionCount = db.PermissionNodes.Count();
                var roleCount = db.Roles.Count();
                var activePermissionCount = db.PermissionNodes.Count(p => p.IsActive);
                var rolePermissionCount = db.RolePermissions.Count();

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        permissionCount,
                        activePermissionCount,
                        roleCount,
                        rolePermissionCount,
                        permissions = db.PermissionNodes.Take(5).Select(p => new { p.Id, p.Name, p.Path }).ToList(),
                        roles = db.Roles.Take(5).Select(r => new { r.Id, r.Name }).ToList()
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Test sayfasÄ± - Permission ve rol verilerini kontrol et
        /// </summary>
        public ActionResult Test()
        {
            try
            {
                ViewBag.Title = "Yetki Sistemi Test";
                
                // Permission sayÄ±sÄ±
                var permissionCount = db.PermissionNodes.Count();
                ViewBag.PermissionCount = permissionCount;
                
                // Rol sayÄ±sÄ±
                var roleCount = db.Roles.Count();
                ViewBag.RoleCount = roleCount;
                
                // Permission listesi
                var permissions = db.PermissionNodes.Take(10).ToList();
                ViewBag.SamplePermissions = permissions;
                
                // Rol listesi
                var roles = db.Roles.Take(10).ToList();
                ViewBag.SampleRoles = roles;
                
                ViewBag.Message = "âœ… Test baÅŸarÄ±lÄ±";
                
                if (permissionCount == 0)
                {
                    ViewBag.Message = "âš ï¸  Permission'lar bulunamadÄ±. Seeding yapÄ±lÄ±yor...";
                    Data.PermissionSeeder.SeedPermissions(db);
                    ViewBag.PermissionCount = db.PermissionNodes.Count();
                }
                
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"âŒ Test hatasÄ±: {ex.Message}";
                ViewBag.Error = ex.ToString();
                return View();
            }
        }

        /// <summary>
        /// Manuel tablo oluÅŸturma endpoint'i - debug iÃ§in
        /// </summary>
        public JsonResult CreateTableManually()
        {
            try
            {
                // Ã–nce tabloyu kontrol et
                try
                {
                    var test = db.RolePermissions.Take(1).ToList();
                    return Json(new { success = true, message = "Tablo zaten mevcut" }, JsonRequestBehavior.AllowGet);
                }
                catch
                {
                    // Tablo yok, oluÅŸtur
                }

                // En basit SQL ile tablo oluÅŸtur
                var sql = @"
                    CREATE TABLE RolePermissions (
                        Id int IDENTITY(1,1) NOT NULL,
                        RoleId int NOT NULL,
                        PermissionNodeId int NOT NULL,
                        CanView bit NOT NULL DEFAULT 0,
                        CanCreate bit NOT NULL DEFAULT 0,
                        CanEdit bit NOT NULL DEFAULT 0,
                        CanDelete bit NOT NULL DEFAULT 0,
                        CanManage bit NOT NULL DEFAULT 0,
                        CanApprove bit NOT NULL DEFAULT 0,
                        CanReject bit NOT NULL DEFAULT 0,
                        CanExport bit NOT NULL DEFAULT 0,
                        CanImport bit NOT NULL DEFAULT 0,
                        IsActive bit NOT NULL DEFAULT 1,
                        CreatedAt datetime NOT NULL DEFAULT GETDATE(),
                        CreatedBy nvarchar(255),
                        UpdatedAt datetime,
                        UpdatedBy nvarchar(255),
                        Notes nvarchar(max),
                        CONSTRAINT PK_RolePermissions PRIMARY KEY (Id)
                    )";

                db.Database.ExecuteSqlCommand(sql);

                // Test et
                var testQuery = db.RolePermissions.Take(1).ToList();
                
                return Json(new { 
                    success = true, 
                    message = "RolePermissions tablosu baÅŸarÄ±yla oluÅŸturuldu!" 
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                }, JsonRequestBehavior.AllowGet);
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

    /// <summary>
    /// Role permission atama modeli
    /// </summary>
    public class RolePermissionAssignModel
    {
        public int RoleId { get; set; }
        public int PermissionNodeId { get; set; }
        public bool CanView { get; set; }
        public bool CanCreate { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanManage { get; set; }
        public bool CanApprove { get; set; }
        public bool CanReject { get; set; }
        public bool CanExport { get; set; }
        public bool CanImport { get; set; }
        public string Notes { get; set; }
    }

    /// <summary>
    /// Toplu permission atama modeli
    /// </summary>
    public class BulkPermissionAssignModel
    {
        public List<RolePermissionAssignModel> Assignments { get; set; }
    }

    /// <summary>
    /// Rol ÅŸablonu
    /// </summary>
    public class RoleTemplate
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string[] PermissionPaths { get; set; }
        public PermissionSet DefaultPermissions { get; set; }
    }

    /// <summary>
    /// Permission set modeli
    /// </summary>
    public class PermissionSet
    {
        public bool CanView { get; set; }
        public bool CanCreate { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanManage { get; set; }
        public bool CanApprove { get; set; }
        public bool CanReject { get; set; }
        public bool CanExport { get; set; }
        public bool CanImport { get; set; }
    }
} 
