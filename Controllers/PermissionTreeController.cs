using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MZDNETWORK.Attributes;
using MZDNETWORK.Data;
using MZDNETWORK.Models;
using MZDNETWORK.Helpers;
using Newtonsoft.Json;

namespace MZDNETWORK.Controllers
{
    /// <summary>
    /// Permission Tree Management Controller
    /// Hiyerarşik yetki ağacı yönetimi için modern UI
    /// </summary>
    [DynamicAuthorize(Permission = "RoleManagement.PermissionTree")]
    public class PermissionTreeController : Controller
    {
        private MZDNETWORKContext db = new MZDNETWORKContext();

        /// <summary>
        /// Ana permission tree sayfası
        /// </summary>
        public ActionResult Index()
        {
            ViewBag.Title = "Yetki Ağacı Yönetimi";
            return View();
        }

        /// <summary>
        /// Permission tree JSON data (Ajax)
        /// </summary>
        public JsonResult GetPermissionTree()
        {
            try
            {
                var permissions = db.PermissionNodes
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.SortOrder)
                    .Select(p => new
                    {
                        id = p.Id,
                        text = p.Name,
                        parent = p.ParentId.HasValue ? p.ParentId.Value.ToString() : "#",
                        type = p.Type.ToLower(),
                        data = new
                        {
                            path = p.Path,
                            description = p.Description,
                            icon = p.Icon,
                            hasView = p.HasViewPermission,
                            hasCreate = p.HasCreatePermission,
                            hasEdit = p.HasEditPermission,
                            hasDelete = p.HasDeletePermission,
                            sortOrder = p.SortOrder,
                            isActive = p.IsActive
                        }
                    })
                    .ToList();

                return Json(permissions, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Permission detaylarını getirir
        /// </summary>
        public JsonResult GetPermissionDetails(int id)
        {
            try
            {
                var permission = db.PermissionNodes.Find(id);
                if (permission == null)
                {
                    return Json(new { success = false, message = "Permission bulunamadı" }, JsonRequestBehavior.AllowGet);
                }

                var result = new
                {
                    success = true,
                    data = new
                    {
                        id = permission.Id,
                        name = permission.Name,
                        path = permission.Path,
                        description = permission.Description,
                        parentId = permission.ParentId,
                        type = permission.Type,
                        icon = permission.Icon,
                        sortOrder = permission.SortOrder,
                        isActive = permission.IsActive,
                        hasViewPermission = permission.HasViewPermission,
                        hasCreatePermission = permission.HasCreatePermission,
                        hasEditPermission = permission.HasEditPermission,
                        hasDeletePermission = permission.HasDeletePermission,
                        parentName = permission.Parent?.Name,
                        childrenCount = permission.Children?.Count(c => c.IsActive) ?? 0,
                        roleCount = db.RolePermissions.Count(rp => rp.PermissionNodeId == permission.Id && rp.IsActive)
                    }
                };

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Yeni permission node oluşturur
        /// </summary>
        [HttpPost]
        [DynamicAuthorize(Permission = "RoleManagement.PermissionTree", Action = "Create")]
        public JsonResult CreatePermissionNode(PermissionNodeCreateModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                // Path benzersizlik kontrolü
                if (db.PermissionNodes.Any(p => p.Path == model.Path))
                {
                    return Json(new { success = false, message = "Bu path zaten kullanılıyor" });
                }

                var permission = new PermissionNode
                {
                    Name = model.Name,
                    Path = model.Path,
                    Description = model.Description,
                    ParentId = model.ParentId == 0 ? null : model.ParentId,
                    Type = model.Type,
                    Icon = model.Icon ?? "bx-key",
                    SortOrder = model.SortOrder,
                    IsActive = true,
                    HasViewPermission = model.HasViewPermission,
                    HasCreatePermission = model.HasCreatePermission,
                    HasEditPermission = model.HasEditPermission,
                    HasDeletePermission = model.HasDeletePermission
                };

                db.PermissionNodes.Add(permission);
                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    message = "Permission başarıyla oluşturuldu",
                    data = new { id = permission.Id, name = permission.Name, path = permission.Path }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Permission node günceller
        /// </summary>
        [HttpPost]
        [DynamicAuthorize(Permission = "RoleManagement.PermissionTree", Action = "Edit")]
        public JsonResult UpdatePermissionNode(PermissionNodeUpdateModel model)
        {
            try
            {
                var permission = db.PermissionNodes.Find(model.Id);
                if (permission == null)
                {
                    return Json(new { success = false, message = "Permission bulunamadı" });
                }

                // Path benzersizlik kontrolü (kendi ID'si hariç)
                if (db.PermissionNodes.Any(p => p.Path == model.Path && p.Id != model.Id))
                {
                    return Json(new { success = false, message = "Bu path zaten kullanılıyor" });
                }

                permission.Name = model.Name;
                permission.Path = model.Path;
                permission.Description = model.Description;
                permission.Icon = model.Icon;
                permission.SortOrder = model.SortOrder;
                permission.HasViewPermission = model.HasViewPermission;
                permission.HasCreatePermission = model.HasCreatePermission;
                permission.HasEditPermission = model.HasEditPermission;
                permission.HasDeletePermission = model.HasDeletePermission;

                db.SaveChanges();

                // Cache'i temizle
                DynamicPermissionHelper.ClearPermissionCache();

                return Json(new { success = true, message = "Permission başarıyla güncellendi" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Permission node siler (soft delete)
        /// </summary>
        [HttpPost]
        [DynamicAuthorize(Permission = "RoleManagement.PermissionTree", Action = "Delete")]
        public JsonResult DeletePermissionNode(int id)
        {
            try
            {
                var permission = db.PermissionNodes.Find(id);
                if (permission == null)
                {
                    return Json(new { success = false, message = "Permission bulunamadı" });
                }

                // Alt permission'ları kontrol et
                var hasChildren = db.PermissionNodes.Any(p => p.ParentId == id && p.IsActive);
                if (hasChildren)
                {
                    return Json(new { success = false, message = "Alt permission'ları olan bir permission silinemez" });
                }

                // Role assignment'larını kontrol et
                var hasRoleAssignments = db.RolePermissions.Any(rp => rp.PermissionNodeId == id && rp.IsActive);
                if (hasRoleAssignments)
                {
                    return Json(new { success = false, message = "Role atanmış permission'lar silinemez. Önce role atamalarını kaldırın." });
                }

                // Soft delete
                permission.IsActive = false;
                db.SaveChanges();

                // Cache'i temizle
                DynamicPermissionHelper.ClearPermissionCache();

                return Json(new { success = true, message = "Permission başarıyla silindi" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Permission tree istatistikleri
        /// </summary>
        public JsonResult GetPermissionStatistics()
        {
            try
            {
                var stats = new
                {
                    totalNodes = db.PermissionNodes.Count(p => p.IsActive),
                    modules = db.PermissionNodes.Count(p => p.Type == "Module" && p.IsActive),
                    subModules = db.PermissionNodes.Count(p => p.Type == "SubModule" && p.IsActive),
                    actions = db.PermissionNodes.Count(p => p.Type.Contains("Action") && p.IsActive),
                    totalRoles = db.Roles.Count(),
                    totalRolePermissions = db.RolePermissions.Count(rp => rp.IsActive),
                    mostUsedPermissions = db.RolePermissions
                        .Where(rp => rp.IsActive)
                        .GroupBy(rp => rp.PermissionNode.Name)
                        .Select(g => new { permission = g.Key, count = g.Count() })
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
        /// Permission tree'yi yeniden oluşturur (Re-seed)
        /// </summary>
        [HttpPost]
        [DynamicAuthorize(Permission = "RoleManagement.PermissionTree", Action = "Manage")]
        public JsonResult ReseedPermissionTree()
        {
            try
            {
                // Mevcut permission'ları pasif yap
                var existingPermissions = db.PermissionNodes.ToList();
                foreach (var perm in existingPermissions)
                {
                    perm.IsActive = false;
                }
                db.SaveChanges();

                // Yeniden seed et
                PermissionSeeder.SeedPermissions(db);

                // Cache'i temizle
                DynamicPermissionHelper.ClearPermissionCache();

                return Json(new { success = true, message = "Permission tree başarıyla yeniden oluşturuldu" });
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
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// Permission node oluşturma modeli
    /// </summary>
    public class PermissionNodeCreateModel
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Description { get; set; }
        public int? ParentId { get; set; }
        public string Type { get; set; }
        public string Icon { get; set; }
        public int SortOrder { get; set; }
        public bool HasViewPermission { get; set; }
        public bool HasCreatePermission { get; set; }
        public bool HasEditPermission { get; set; }
        public bool HasDeletePermission { get; set; }
    }

    /// <summary>
    /// Permission node güncelleme modeli
    /// </summary>
    public class PermissionNodeUpdateModel : PermissionNodeCreateModel
    {
        public int Id { get; set; }
    }
}