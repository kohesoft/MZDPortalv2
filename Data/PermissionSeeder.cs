using System;
using System.Collections.Generic;
using System.Linq;
using MZDNETWORK.Models;
using System.Reflection;
using System.Web.Mvc;

namespace MZDNETWORK.Data
{
    /// <summary>
    /// MZD Portal iÃ§in basit permission seeder'Ä±
    /// </summary>
    public static class PermissionSeeder
    {
        /// <summary>
        /// Ana permission aÄŸacÄ±nÄ± oluÅŸturur
        /// </summary>
        public static void SeedPermissions(MZDNETWORKContext context)
        {
            try
            {
                Console.WriteLine("ğŸŒ± Permission node'larÄ± DynamicAuthorize taramasÄ±yla senkronize ediliyor...");

                // Mevcut node'larÄ± aktif hÃ¢le getir ve temizle (opsiyonel)
                if (context.PermissionNodes.Any())
                {
                    UpdateExistingPermissions(context);
                }

                // Sadece Controller'lardaki [DynamicAuthorize] path'lerini tarayÄ±p eksik node'larÄ± ekle
                EnsureDynamicAuthorizePermissions(context);

                context.SaveChanges();

                // Ä°statistikler
                ShowStatistics(context);

                // Admin kullanÄ±cÄ± & rolÃ¼ (varsa gÃ¼ncelle)
                CreateAdminUserAndRole(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"âŒ Hata: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Mevcut permission'larÄ± gÃ¼nceller
        /// </summary>
        private static void UpdateExistingPermissions(MZDNETWORKContext context)
        {
            // Pasif node'larÄ± aktif hale getir
            var inactiveNodes = context.PermissionNodes.Where(p => !p.IsActive).ToList();
            foreach (var node in inactiveNodes)
            {
                node.IsActive = true;
                Console.WriteLine($"ğŸ”„ Permission aktif hale getirildi: {node.Path}");
            }

            context.SaveChanges();
            Console.WriteLine("âœ… Mevcut permission'lar gÃ¼ncellendi");
        }

        /// <summary>
        /// Ä°statistikleri gÃ¶sterir
        /// </summary>
        private static void ShowStatistics(MZDNETWORKContext context)
        {
            var totalNodes = context.PermissionNodes.Count();
            var modules = context.PermissionNodes.Count(p => p.Type == "Module");
            var actions = context.PermissionNodes.Count(p => p.Type == "Action");

            Console.WriteLine("\nğŸ“Š Ä°statistikler:");
            Console.WriteLine($"   ğŸ“ Toplam Node: {totalNodes}");
            Console.WriteLine($"   ğŸ¢ ModÃ¼l: {modules}");
            Console.WriteLine($"   ğŸ”‘ Action: {actions}");
            Console.WriteLine();
        }

        /// <summary>
        /// Admin kullanÄ±cÄ±sÄ± ve rolÃ¼ oluÅŸturur
        /// </summary>
        private static void CreateAdminUserAndRole(MZDNETWORKContext context)
        {
            try
            {
                // Admin rolÃ¼ oluÅŸtur
                var adminRole = context.Roles.FirstOrDefault(r => r.Name == "SuperAdmin");
                if (adminRole == null)
                {
                    adminRole = new Role
                    {
                        Name = "SuperAdmin",
                        Description = "SÃ¼per YÃ¶netici - TÃ¼m yetkiler",
                        IsActive = true,
                        CreatedDate = DateTime.Now,
                        CreatedBy = 0
                    };
                    context.Roles.Add(adminRole);
                    context.SaveChanges();
                    Console.WriteLine("ğŸ‘‘ SuperAdmin rolÃ¼ oluÅŸturuldu");
                }

                // TÃ¼m permission'lara tam yetki ver
                var allPermissions = context.PermissionNodes.Where(p => p.IsActive).ToList();
                var existingRolePermissions = context.RolePermissions
                    .Where(rp => rp.RoleId == adminRole.Id)
                    .Select(rp => rp.PermissionNodeId)
                    .ToList();

                foreach (var permission in allPermissions)
                {
                    if (!existingRolePermissions.Contains(permission.Id))
                    {
                        var rolePermission = new RolePermission
                        {
                            RoleId = adminRole.Id,
                            PermissionNodeId = permission.Id,
                            CanView = true,
                            CanCreate = true,
                            CanEdit = true,
                            CanDelete = true,
                            CanManage = true,
                            CanApprove = true,
                            CanReject = true,
                            CanExport = true,
                            CanImport = true,
                            IsActive = true,
                            CreatedAt = DateTime.Now,
                            CreatedBy = "System"
                        };

                        context.RolePermissions.Add(rolePermission);
                    }
                }

                // Admin kullanÄ±cÄ±sÄ± oluÅŸtur
                var adminUser = context.Users.FirstOrDefault(u => u.Username == "admin");
                if (adminUser == null)
                {
                    adminUser = new User
                    {
                        Username = "admin",
                        Password = "admin123",
                        Name = "Sistem",
                        Surname = "YÃ¶neticisi",
                        Department = "Bilgi Ä°ÅŸlem",
                        Position = "Sistem YÃ¶neticisi",
                        InternalEmail = "admin@mzd.com",
                        ExternalEmail = "admin@mzd.com",
                        PhoneNumber = "1000",
                        Intercom = "1000",
                        Sicil = "ADMIN001",
                        IsPasswordChanged = false
                    };

                    context.Users.Add(adminUser);
                    context.SaveChanges();
                    Console.WriteLine("ğŸ‘¤ Admin kullanÄ±cÄ±sÄ± oluÅŸturuldu");
                }

                // Admin rolÃ¼nÃ¼ kullanÄ±cÄ±ya ata
                var existingUserRole = context.UserRoles
                    .FirstOrDefault(ur => ur.UserId == adminUser.Id && ur.RoleId == adminRole.Id);

                if (existingUserRole == null)
                {
                    var userRole = new UserRole
                    {
                        UserId = adminUser.Id,
                        RoleId = adminRole.Id,
                        AssignedDate = DateTime.Now,
                        AssignedBy = adminUser.Id,
                        IsActive = true
                    };

                    context.UserRoles.Add(userRole);
                    context.SaveChanges();
                    Console.WriteLine("âœ… Admin rolÃ¼ atandÄ±");
                }

                context.SaveChanges();
                Console.WriteLine($"âœ… SuperAdmin rolÃ¼ne {allPermissions.Count} permission atandÄ±");
                Console.WriteLine("ğŸ¯ Admin kullanÄ±cÄ±sÄ± hazÄ±r! GiriÅŸ: admin / admin123");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"âŒ Admin oluÅŸturma hatasÄ±: {ex.Message}");
                throw;
            }
        }

        private static void EnsureDynamicAuthorizePermissions(MZDNETWORKContext context)
        {
            try
            {
                var discoveredPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                // TÃ¼m assembly'lerdeki Controller tiplerini tara
                var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic && a.FullName.StartsWith("MZDNETWORK"));

                foreach (var asm in assemblies)
                {
                    var controllerTypes = asm.GetTypes()
                        .Where(t => typeof(System.Web.Mvc.Controller).IsAssignableFrom(t) && !t.IsAbstract);

                    foreach (var ctrl in controllerTypes)
                    {
                        // SÄ±nÄ±f seviyesindeki attribute
                        var classAttr = ctrl.GetCustomAttributes(typeof(Attributes.DynamicAuthorizeAttribute), true)
                                            .FirstOrDefault() as Attributes.DynamicAuthorizeAttribute;
                        if (classAttr != null && !string.IsNullOrWhiteSpace(classAttr.Permission))
                        {
                            var fullPath = BuildFullPermissionPath(classAttr);
                            discoveredPaths.Add(fullPath);
                        }

                        // Metod seviyesindeki attribute'lar
                        var actionMethods = ctrl.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
                                                .Where(m => m.ReturnType.IsSubclassOf(typeof(ActionResult)) || m.ReturnType == typeof(ActionResult) || m.ReturnType.IsSubclassOf(typeof(JsonResult)) || m.ReturnType == typeof(JsonResult));

                        foreach (var method in actionMethods)
                        {
                            var methodAttr = method.GetCustomAttributes(typeof(Attributes.DynamicAuthorizeAttribute), true)
                                                    .FirstOrDefault() as Attributes.DynamicAuthorizeAttribute;
                            if (methodAttr != null && !string.IsNullOrWhiteSpace(methodAttr.Permission))
                            {
                                var fullPath = BuildFullPermissionPath(methodAttr);
                                discoveredPaths.Add(fullPath);
                            }
                        }
                    }
                }

                if (!discoveredPaths.Any())
                    return; // HiÃ§bir permission keÅŸfedilmedi

                Console.WriteLine($"ğŸ” DynamicAuthorize taramasÄ±: {discoveredPaths.Count} benzersiz permission bulundu");

                foreach (var path in discoveredPaths)
                {
                    EnsurePathNodes(context, path);
                }

                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"âš ï¸  EnsureDynamicAuthorizePermissions error: {ex.Message}");
            }
        }

        /// <summary>
        /// DynamicAuthorize attribute'Ä±ndaki Permission + Action deÄŸerini tam path'e dÃ¶nÃ¼ÅŸtÃ¼rÃ¼r
        /// </summary>
        private static string BuildFullPermissionPath(Attributes.DynamicAuthorizeAttribute attr)
        {
            if (string.IsNullOrEmpty(attr.Permission))
                return string.Empty;

            if (string.IsNullOrEmpty(attr.Action) || attr.Action.Equals("View", StringComparison.OrdinalIgnoreCase))
                return attr.Permission;

            // Attr.Action zaten path iÃ§erisinde varsa tekrar ekleme
            if (attr.Permission.EndsWith($".{attr.Action}", StringComparison.OrdinalIgnoreCase))
                return attr.Permission;

            return $"{attr.Permission}.{attr.Action}";
        }

        /// <summary>
        /// Verilen tam permission path'i iÃ§in (Ã¶r. "KullaniciYonetimi.Edit") hiyerarÅŸideki tÃ¼m node'larÄ±n DB'de olduÄŸundan emin olur
        /// </summary>
        private static void EnsurePathNodes(MZDNETWORKContext context, string fullPath)
        {
            if (string.IsNullOrWhiteSpace(fullPath)) return;

            var segments = fullPath.Split('.');
            string accumulatedPath = string.Empty;
            int? parentId = null;

            for (int i = 0; i < segments.Length; i++)
            {
                var segment = segments[i];
                accumulatedPath = i == 0 ? segment : $"{accumulatedPath}.{segment}";

                var existing = context.PermissionNodes.FirstOrDefault(p => p.Path == accumulatedPath);
                if (existing != null)
                {
                    parentId = existing.Id;
                    continue;
                }

                // Yeni node ekle
                string type;
                if (i == 0) type = "Module"; // kÃ¶k
                else if (i == segments.Length - 1) type = "Action"; // son seviye
                else type = "Controller"; // ara seviye

                var sortOrder = (context.PermissionNodes.Where(p => p.ParentId == parentId).Max(p => (int?)p.SortOrder) ?? 0) + 1;

                var newNode = new PermissionNode
                {
                    Name = segment,
                    Path = accumulatedPath,
                    Description = segment,
                    ParentId = parentId,
                    Type = type,
                    Icon = "bx-key",
                    SortOrder = sortOrder,
                    IsActive = true,
                    HasViewPermission = true
                };

                context.PermissionNodes.Add(newNode);
                context.SaveChanges(); // ID almak iÃ§in

                parentId = newNode.Id;
                Console.WriteLine($"â• Otomatik node eklendi: {accumulatedPath} ({type})");
            }
        }

        /// <summary>
        /// DÄ±ÅŸ Ã§aÄŸrÄ±lar iÃ§in geriye dÃ¶nÃ¼k uyumluluk: Admin rolÃ¼nÃ¼ oluÅŸturur
        /// </summary>
        public static void CreateDefaultAdminRole(MZDNETWORKContext context)
        {
            CreateAdminUserAndRole(context);
        }

        /// <summary>
        /// DÄ±ÅŸ Ã§aÄŸrÄ±lar iÃ§in geriye dÃ¶nÃ¼k uyumluluk: Admin kullanÄ±cÄ±sÄ±nÄ± oluÅŸturur
        /// </summary>
        public static void CreateDefaultAdminUser(MZDNETWORKContext context)
        {
            CreateAdminUserAndRole(context);
        }
    }
} 
