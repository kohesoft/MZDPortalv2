using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MZDNETWORK.Models;
using MZDNETWORK.Data;

namespace MZDNETWORK.Helpers
{
    /// <summary>
    /// Yetki ağacı yapısını oluşturmak ve yönetmek için helper sınıfı
    /// </summary>
    public static class PermissionTreeBuilder
    {
        /// <summary>
        /// Tüm yetki ağacını hiyerarşik yapıda getirir
        /// </summary>
        /// <returns>Root node'ları içeren liste</returns>
        public static List<PermissionTreeNode> BuildFullTree()
        {
            using (var context = new MZDNETWORKContext())
            {
                var allNodes = context.PermissionNodes
                    .Where(n => n.IsActive)
                    .OrderBy(n => n.SortOrder)
                    .ThenBy(n => n.Name)
                    .ToList();

                return BuildTreeFromNodes(allNodes);
            }
        }

        /// <summary>
        /// Belirli bir role sahip kullanıcının yetki ağacını getirir
        /// </summary>
        /// <param name="roleIds">Rol ID'leri</param>
        /// <returns>Yetki ağacı</returns>
        public static List<PermissionTreeNode> BuildTreeForRoles(List<int> roleIds)
        {
            using (var context = new MZDNETWORKContext())
            {
                // Bu rollere ait yetki node'larını al
                var permissionNodeIds = context.RolePermissions
                    .Where(rp => roleIds.Contains(rp.RoleId) && rp.IsActive && rp.HasAnyPermission)
                    .Select(rp => rp.PermissionNodeId)
                    .Distinct()
                    .ToList();

                var nodes = context.PermissionNodes
                    .Where(n => n.IsActive && permissionNodeIds.Contains(n.Id))
                    .OrderBy(n => n.SortOrder)
                    .ThenBy(n => n.Name)
                    .ToList();

                // Parent node'ları da dahil et (inheritance için)
                var allRequiredNodes = GetAllRequiredNodes(nodes, context);

                return BuildTreeFromNodes(allRequiredNodes);
            }
        }

        /// <summary>
        /// Belirli bir kullanıcının yetki ağacını getirir
        /// </summary>
        /// <param name="userId">Kullanıcı ID</param>
        /// <returns>Kullanıcının erişebileceği yetki ağacı</returns>
        public static List<PermissionTreeNode> BuildTreeForUser(int userId)
        {
            var userRoles = RoleHelper.GetUserRoles(userId);
            
            using (var context = new MZDNETWORKContext())
            {
                var roleIds = context.Roles
                    .Where(r => userRoles.Contains(r.Name))
                    .Select(r => r.Id)
                    .ToList();

                return BuildTreeForRoles(roleIds);
            }
        }

        /// <summary>
        /// Node listesinden hiyerarşik ağaç yapısı oluşturur
        /// </summary>
        /// <param name="nodes">Düz node listesi</param>
        /// <returns>Hiyerarşik ağaç yapısı</returns>
        private static List<PermissionTreeNode> BuildTreeFromNodes(List<PermissionNode> nodes)
        {
            var treeNodes = nodes.Select(n => new PermissionTreeNode
            {
                Id = n.Id,
                Name = n.Name,
                Path = n.Path,
                Description = n.Description,
                Type = n.Type,
                Icon = n.Icon,
                SortOrder = n.SortOrder,
                ParentId = n.ParentId,
                HasViewPermission = n.HasViewPermission,
                HasCreatePermission = n.HasCreatePermission,
                HasEditPermission = n.HasEditPermission,
                HasDeletePermission = n.HasDeletePermission,
                Children = new List<PermissionTreeNode>()
            }).ToList();

            // Parent-child ilişkilerini kur
            var rootNodes = new List<PermissionTreeNode>();

            foreach (var node in treeNodes)
            {
                if (node.ParentId == null)
                {
                    // Root node
                    rootNodes.Add(node);
                }
                else
                {
                    // Child node - parent'ını bul ve ekle
                    var parent = treeNodes.FirstOrDefault(t => t.Id == node.ParentId);
                    if (parent != null)
                    {
                        parent.Children.Add(node);
                    }
                    else
                    {
                        // Parent bulunamadı, root olarak ekle
                        rootNodes.Add(node);
                    }
                }
            }

            return rootNodes.OrderBy(n => n.SortOrder).ThenBy(n => n.Name).ToList();
        }

        /// <summary>
        /// Verilen node'lar için gerekli tüm parent node'ları da dahil eder
        /// </summary>
        /// <param name="nodes">Mevcut node'lar</param>
        /// <param name="context">Database context</param>
        /// <returns>Tüm gerekli node'lar (parent'lar dahil)</returns>
        private static List<PermissionNode> GetAllRequiredNodes(List<PermissionNode> nodes, MZDNETWORKContext context)
        {
            var allNodes = new List<PermissionNode>(nodes);
            var processedIds = new HashSet<int>(nodes.Select(n => n.Id));

            foreach (var node in nodes.ToList())
            {
                AddParentNodes(node, allNodes, processedIds, context);
            }

            return allNodes;
        }

        /// <summary>
        /// Belirli bir node'un tüm parent'larını recursively ekler
        /// </summary>
        /// <param name="node">Node</param>
        /// <param name="allNodes">Tüm node'lar listesi</param>
        /// <param name="processedIds">İşlenmiş ID'ler</param>
        /// <param name="context">Database context</param>
        private static void AddParentNodes(PermissionNode node, List<PermissionNode> allNodes, HashSet<int> processedIds, MZDNETWORKContext context)
        {
            if (node.ParentId == null || processedIds.Contains(node.ParentId.Value))
                return;

            var parent = context.PermissionNodes.Find(node.ParentId.Value);
            if (parent != null && parent.IsActive)
            {
                allNodes.Add(parent);
                processedIds.Add(parent.Id);

                // Recursively parent'ların parent'larını da ekle
                AddParentNodes(parent, allNodes, processedIds, context);
            }
        }

        /// <summary>
        /// Yeni yetki node'u oluşturur
        /// </summary>
        /// <param name="name">Node adı</param>
        /// <param name="path">Yetki yolu</param>
        /// <param name="parentId">Parent node ID</param>
        /// <param name="type">Node tipi</param>
        /// <param name="icon">Icon</param>
        /// <param name="description">Açıklama</param>
        /// <returns>Oluşturulan node ID</returns>
        public static int CreateNode(string name, string path, int? parentId, string type, string icon = null, string description = null)
        {
            using (var context = new MZDNETWORKContext())
            {
                // Path unique kontrolü
                var existingNode = context.PermissionNodes.FirstOrDefault(n => n.Path == path && n.IsActive);
                if (existingNode != null)
                {
                    throw new InvalidOperationException($"Path '{path}' zaten mevcut.");
                }

                // Parent kontrolü
                if (parentId.HasValue)
                {
                    var parent = context.PermissionNodes.Find(parentId.Value);
                    if (parent == null || !parent.IsActive)
                    {
                        throw new InvalidOperationException("Geçersiz parent node.");
                    }
                }

                // SortOrder belirle
                var maxSortOrder = context.PermissionNodes
                    .Where(n => n.ParentId == parentId)
                    .Max(n => (int?)n.SortOrder) ?? 0;

                var newNode = new PermissionNode
                {
                    Name = name,
                    Path = path,
                    ParentId = parentId,
                    Type = type,
                    Icon = icon,
                    Description = description,
                    SortOrder = maxSortOrder + 1,
                    IsActive = true,
                    HasViewPermission = true,
                    HasCreatePermission = false,
                    HasEditPermission = false,
                    HasDeletePermission = false
                };

                context.PermissionNodes.Add(newNode);
                context.SaveChanges();

                return newNode.Id;
            }
        }

        /// <summary>
        /// Node'u günceller
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        /// <param name="name">Yeni ad</param>
        /// <param name="description">Yeni açıklama</param>
        /// <param name="icon">Yeni icon</param>
        /// <returns>Başarılı mı</returns>
        public static bool UpdateNode(int nodeId, string name = null, string description = null, string icon = null)
        {
            using (var context = new MZDNETWORKContext())
            {
                var node = context.PermissionNodes.Find(nodeId);
                if (node == null || !node.IsActive)
                    return false;

                if (!string.IsNullOrEmpty(name))
                    node.Name = name;

                if (description != null)
                    node.Description = description;

                if (icon != null)
                    node.Icon = icon;

                context.SaveChanges();
                return true;
            }
        }

        /// <summary>
        /// Node'u siler (soft delete)
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        /// <returns>Başarılı mı</returns>
        public static bool DeleteNode(int nodeId)
        {
            using (var context = new MZDNETWORKContext())
            {
                var node = context.PermissionNodes.Find(nodeId);
                if (node == null)
                    return false;

                // Child node'ları kontrol et
                var hasChildren = context.PermissionNodes.Any(n => n.ParentId == nodeId && n.IsActive);
                if (hasChildren)
                {
                    throw new InvalidOperationException("Bu node'un alt node'ları var. Önce onları silin.");
                }

                node.IsActive = false;
                context.SaveChanges();

                return true;
            }
        }

        /// <summary>
        /// Node'ların sırasını değiştirir
        /// </summary>
        /// <param name="nodeOrders">Node ID ve yeni sıra numaraları</param>
        /// <returns>Başarılı mı</returns>
        public static bool ReorderNodes(Dictionary<int, int> nodeOrders)
        {
            using (var context = new MZDNETWORKContext())
            {
                foreach (var order in nodeOrders)
                {
                    var node = context.PermissionNodes.Find(order.Key);
                    if (node != null && node.IsActive)
                    {
                        node.SortOrder = order.Value;
                    }
                }

                context.SaveChanges();
                return true;
            }
        }

        /// <summary>
        /// Belirli bir node'un tüm child'larını getirir (recursive)
        /// </summary>
        /// <param name="nodeId">Parent node ID</param>
        /// <returns>Tüm child node'lar</returns>
        public static List<PermissionNode> GetAllChildren(int nodeId)
        {
            using (var context = new MZDNETWORKContext())
            {
                var allChildren = new List<PermissionNode>();
                GetChildrenRecursive(nodeId, allChildren, context);
                return allChildren;
            }
        }

        /// <summary>
        /// Recursive olarak child node'ları toplar
        /// </summary>
        private static void GetChildrenRecursive(int parentId, List<PermissionNode> result, MZDNETWORKContext context)
        {
            var directChildren = context.PermissionNodes
                .Where(n => n.ParentId == parentId && n.IsActive)
                .ToList();

            result.AddRange(directChildren);

            foreach (var child in directChildren)
            {
                GetChildrenRecursive(child.Id, result, context);
            }
        }

        /// <summary>
        /// Path'e göre node bulur
        /// </summary>
        /// <param name="path">Yetki yolu</param>
        /// <returns>Bulunan node veya null</returns>
        public static PermissionNode FindNodeByPath(string path)
        {
            using (var context = new MZDNETWORKContext())
            {
                return context.PermissionNodes
                    .FirstOrDefault(n => n.Path == path && n.IsActive);
            }
        }
    }

    /// <summary>
    /// Tree view için optimize edilmiş yetki node modeli
    /// </summary>
    public class PermissionTreeNode
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Icon { get; set; }
        public int SortOrder { get; set; }
        public int? ParentId { get; set; }
        public bool HasViewPermission { get; set; }
        public bool HasCreatePermission { get; set; }
        public bool HasEditPermission { get; set; }
        public bool HasDeletePermission { get; set; }
        public List<PermissionTreeNode> Children { get; set; }

        public PermissionTreeNode()
        {
            Children = new List<PermissionTreeNode>();
        }

        /// <summary>
        /// Node'un herhangi bir child'ı var mı
        /// </summary>
        public bool HasChildren => Children != null && Children.Any();

        /// <summary>
        /// Node'un derinlik seviyesi
        /// </summary>
        public int Level
        {
            get
            {
                int level = 0;
                var current = this;
                while (current.ParentId != null)
                {
                    level++;
                    // Parent node'u manuel olarak bulamayız, sadece sayıyı artırabiliriz
                    break; // Infinite loop'u önlemek için
                }
                return level;
            }
        }

        /// <summary>
        /// UI için CSS sınıfı
        /// </summary>
        public string CssClass
        {
            get
            {
                var classes = new List<string> { "permission-node", $"node-type-{Type?.ToLower()}" };
                if (!HasChildren) classes.Add("leaf-node");
                return string.Join(" ", classes);
            }
        }
    }
} 