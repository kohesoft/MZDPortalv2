using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MZDNETWORK.Attributes;
using MZDNETWORK.Data;
using MZDNETWORK.Helpers;
using MZDNETWORK.Models;
using Newtonsoft.Json;
using System.Data.Entity;

namespace MZDNETWORK.Controllers
{
    [Authorize]
    public class AccessChangeTicketController : Controller
    {
        private readonly MZDNETWORKContext _db = new MZDNETWORKContext();
        private readonly NotificationService _notificationService = new NotificationService();

        // Kullanıcıların kendi talepleri
        [HttpGet]
        public ActionResult Index()
        {
            var currentUserId = GetCurrentUserId();
            var tickets = _db.AccessChangeTickets
                .Include(t => t.Requester)
                .Include(t => t.TargetUser)
                .Where(t => t.RequesterUserId == currentUserId || t.TargetUserId == currentUserId)
                .OrderByDescending(t => t.CreatedAt)
                .ToList();
            BackfillSnapshots(tickets);
            PopulateLists();
            return View(tickets);
        }

        // Talep oluşturma sayfası
        [HttpGet]
        public ActionResult Create()
        {
            PopulateLists();
            return View(new AccessChangeTicketViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AccessChangeTicketViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                PopulateLists();
                return View(vm);
            }

            var requesterId = GetCurrentUserId();
            var targetUser = _db.Users.Find(vm.TargetUserId);
            if (targetUser == null)
            {
                ModelState.AddModelError("TargetUserId", "Hedef kullanıcı bulunamadı.");
                PopulateLists();
                return View(vm);
            }

            var currentRoles = GetUserRoleIds(targetUser.Id);
            var currentRoleNames = GetUserRoleNames(targetUser.Id);
            var currentPermissions = GetUserPermissionPaths(targetUser.Id);
            var currentUserInfo = BuildUserInfoDto(targetUser);

            var ticket = new AccessChangeTicket
            {
                RequesterUserId = requesterId,
                TargetUserId = targetUser.Id,
                RequestType = vm.RequestType,
                Description = vm.Description,
                CurrentRolesJson = JsonConvert.SerializeObject(currentRoleNames),
                RequestedRolesJson = JsonConvert.SerializeObject(vm.RequestedRoleIds ?? new List<int>()),
                CurrentPermissionsJson = JsonConvert.SerializeObject(currentPermissions),
                RequestedPermissionsJson = JsonConvert.SerializeObject(vm.RequestedPermissionPaths ?? new List<string>()),
                CurrentUserInfoJson = JsonConvert.SerializeObject(currentUserInfo),
                RequestedUserInfoJson = JsonConvert.SerializeObject(vm.RequestedUserInfo ?? new AccessChangeUserInfoDto()),
                Status = AccessChangeStatus.Pending,
                CreatedAt = DateTime.Now
            };

            _db.AccessChangeTickets.Add(ticket);
            _db.SaveChanges();

            _notificationService.CreateNotification(targetUser.Id.ToString(), "📝 Yeni erişim talebi oluşturuldu.");
            _notificationService.CreateNotification(requesterId.ToString(), "✅ Talebiniz kaydedildi ve yönetici onayına gönderildi.");

            return RedirectToAction("Index");
        }

        // Admin listesi
        [HttpGet]
        [DynamicAuthorize(Permission = "UserManagement.AccessChange", Action = "Manage")]
        public ActionResult AdminList()
        {
            var tickets = _db.AccessChangeTickets
                .Include(t => t.Requester)
                .Include(t => t.TargetUser)
                .OrderByDescending(t => t.CreatedAt)
                .ToList();
            BackfillSnapshots(tickets);
            PopulateLists();
            return View(tickets);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DynamicAuthorize(Permission = "UserManagement.AccessChange", Action = "Manage")]
        public ActionResult Approve(int id, string adminNote)
        {
            var ticket = _db.AccessChangeTickets.Find(id);
            if (ticket == null) return HttpNotFound();

            ApplyTicket(ticket, adminNote);
            return RedirectToAction("AdminList");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DynamicAuthorize(Permission = "UserManagement.AccessChange", Action = "Manage")]
        public ActionResult Reject(int id, string adminNote)
        {
            var ticket = _db.AccessChangeTickets.Find(id);
            if (ticket == null) return HttpNotFound();

            ticket.Status = AccessChangeStatus.Rejected;
            ticket.AdminNote = adminNote;
            ticket.ResolvedAt = DateTime.Now;
            _db.SaveChanges();

            NotifyResolution(ticket, false);
            return RedirectToAction("AdminList");
        }

        private void ApplyTicket(AccessChangeTicket ticket, string adminNote)
        {
            var targetUser = _db.Users.Find(ticket.TargetUserId);
            if (targetUser == null) return;

            switch (ticket.RequestType)
            {
                case AccessChangeRequestType.RoleChange:
                    var requestedRoleIds = Deserialize<List<int>>(ticket.RequestedRolesJson) ?? new List<int>();
                    ApplyRoleChange(targetUser.Id, requestedRoleIds);
                    break;
                case AccessChangeRequestType.PermissionChange:
                    var requestedPerms = Deserialize<List<string>>(ticket.RequestedPermissionsJson) ?? new List<string>();
                    ApplyPermissionChange(targetUser.Id, requestedPerms);
                    break;
                case AccessChangeRequestType.UserInfoChange:
                    var requestedInfo = Deserialize<AccessChangeUserInfoDto>(ticket.RequestedUserInfoJson) ?? new AccessChangeUserInfoDto();
                    ApplyUserInfoChange(targetUser, requestedInfo);
                    break;
            }

            ticket.Status = AccessChangeStatus.Applied;
            ticket.AdminNote = adminNote;
            ticket.ResolvedAt = DateTime.Now;
            _db.SaveChanges();

            NotifyResolution(ticket, true);
        }

        private void ApplyRoleChange(int userId, List<int> requestedRoleIds)
        {
            var existingRoles = _db.UserRoles.Where(ur => ur.UserId == userId).ToList();
            _db.UserRoles.RemoveRange(existingRoles);

            var adminId = GetCurrentUserId();
            foreach (var roleId in requestedRoleIds.Distinct())
            {
                _db.UserRoles.Add(new UserRole
                {
                    UserId = userId,
                    RoleId = roleId,
                    AssignedBy = adminId,
                    AssignedDate = DateTime.Now,
                    IsActive = true
                });
            }
            _db.SaveChanges();
        }

        private void ApplyPermissionChange(int userId, List<string> permissionPaths)
        {
            // Kullanıcıya özel bir rol yarat / kullan ve yetkileri bu role ekle
            var customRoleName = $"Custom-{userId}-Admin";
            var role = _db.Roles.FirstOrDefault(r => r.Name == customRoleName);
            if (role == null)
            {
                role = new Role
                {
                    Name = customRoleName,
                    Description = "Admin onayı ile oluşturulan kullanıcıya özel yetkiler",
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    CreatedBy = GetCurrentUserId()
                };
                _db.Roles.Add(role);
                _db.SaveChanges();
            }

            // RolePermission resetle
            var existingRps = _db.RolePermissions.Where(rp => rp.RoleId == role.Id).ToList();
            _db.RolePermissions.RemoveRange(existingRps);
            _db.SaveChanges();

            var nodes = _db.PermissionNodes.Where(p => permissionPaths.Contains(p.Path)).ToList();
            foreach (var node in nodes)
            {
                _db.RolePermissions.Add(new RolePermission
                {
                    RoleId = role.Id,
                    PermissionNodeId = node.Id,
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
                    CreatedBy = User?.Identity?.Name ?? "system"
                });
            }
            _db.SaveChanges();

            // Kullanıcıya rolü ata (varsa tekrar etme)
            var existingUserRole = _db.UserRoles.FirstOrDefault(ur => ur.UserId == userId && ur.RoleId == role.Id);
            if (existingUserRole == null)
            {
                _db.UserRoles.Add(new UserRole
                {
                    UserId = userId,
                    RoleId = role.Id,
                    AssignedBy = GetCurrentUserId(),
                    AssignedDate = DateTime.Now,
                    IsActive = true
                });
                _db.SaveChanges();
            }
        }

        private void ApplyUserInfoChange(User user, AccessChangeUserInfoDto newInfo)
        {
            user.Name = newInfo.Name;
            user.Surname = newInfo.Surname;
            user.Username = newInfo.Username ?? user.Username;
            user.InternalEmail = newInfo.InternalEmail;
            user.ExternalEmail = newInfo.ExternalEmail;
            user.Department = newInfo.Department;
            user.Position = newInfo.Title;
            user.PhoneNumber = newInfo.PhoneNumber;
            user.Intercom = newInfo.Intercom;
            _db.SaveChanges();
        }

        private void NotifyResolution(AccessChangeTicket ticket, bool approved)
        {
            var requesterId = ticket.RequesterUserId.ToString();
            var targetId = ticket.TargetUserId.ToString();
            if (approved)
            {
                _notificationService.CreateNotification(requesterId, "🎉 Talebiniz onaylandı ve uygulandı.");
                _notificationService.CreateNotification(targetId, "✅ Erişim değişikliğiniz uygulandı.");
            }
            else
            {
                _notificationService.CreateNotification(requesterId, "❌ Talebiniz reddedildi.");
                _notificationService.CreateNotification(targetId, "❌ Erişim değişikliği talebi reddedildi.");
            }
        }

        private List<int> GetUserRoleIds(int userId)
        {
            return _db.UserRoles.Where(ur => ur.UserId == userId).Select(ur => ur.RoleId).ToList();
        }

        private List<string> GetUserRoleNames(int userId)
        {
            return _db.UserRoles.Where(ur => ur.UserId == userId).Select(ur => ur.Role.Name).ToList();
        }

        private List<string> GetUserPermissionPaths(int userId)
        {
            var roleIds = GetUserRoleIds(userId);
            var paths = (from rp in _db.RolePermissions
                         join pn in _db.PermissionNodes on rp.PermissionNodeId equals pn.Id
                         where roleIds.Contains(rp.RoleId)
                         select pn.Path).Distinct().ToList();
            return paths;
        }

        private AccessChangeUserInfoDto BuildUserInfoDto(User user)
        {
            return new AccessChangeUserInfoDto
            {
                Name = user.Name,
                Surname = user.Surname,
                Username = user.Username,
                InternalEmail = user.InternalEmail,
                ExternalEmail = user.ExternalEmail,
                Department = user.Department,
                Title = user.Position,
                PhoneNumber = user.PhoneNumber,
                Intercom = user.Intercom
            };
        }

        private T Deserialize<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return default(T);
            try
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch
            {
                return default(T);
            }
        }

        private void BackfillSnapshots(IEnumerable<AccessChangeTicket> tickets)
        {
            foreach (var t in tickets ?? Enumerable.Empty<AccessChangeTicket>())
            {
                if (string.IsNullOrWhiteSpace(t.CurrentRolesJson))
                {
                    var names = GetUserRoleNames(t.TargetUserId) ?? new List<string>();
                    t.CurrentRolesJson = JsonConvert.SerializeObject(names);
                }

                if (string.IsNullOrWhiteSpace(t.CurrentPermissionsJson))
                {
                    var perms = GetUserPermissionPaths(t.TargetUserId) ?? new List<string>();
                    t.CurrentPermissionsJson = JsonConvert.SerializeObject(perms);
                }

                if (string.IsNullOrWhiteSpace(t.CurrentUserInfoJson) && t.TargetUser != null)
                {
                    t.CurrentUserInfoJson = JsonConvert.SerializeObject(BuildUserInfoDto(t.TargetUser));
                }
            }
        }

        private void PopulateLists()
        {
            ViewBag.Users = _db.Users
                .OrderBy(u => u.Username)
                .Select(u => new AccessChangeUserListItem
                {
                    Id = u.Id,
                    Username = u.Username,
                    Name = u.Name,
                    Surname = u.Surname,
                    InternalEmail = u.InternalEmail,
                    ExternalEmail = u.ExternalEmail,
                    Department = u.Department,
                    Title = u.Position,
                    PhoneNumber = u.PhoneNumber,
                    Intercom = u.Intercom,
                    Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList(),
                    Permissions = (
                        from rp in _db.RolePermissions
                        join pn in _db.PermissionNodes on rp.PermissionNodeId equals pn.Id
                        where u.UserRoles.Select(ur => ur.RoleId).Contains(rp.RoleId)
                        select pn.Path).Distinct().ToList()
                }).ToList();
            ViewBag.Roles = _db.Roles.Where(r => r.IsActive).OrderBy(r => r.Name).ToList();
            ViewBag.PermissionNodes = _db.PermissionNodes.Where(p => p.IsActive).OrderBy(p => p.Path).ToList();
            ViewBag.UserInfoMap = JsonConvert.SerializeObject(ViewBag.Users);
        }

        private int GetCurrentUserId()
        {
            var username = User?.Identity?.Name;
            if (string.IsNullOrWhiteSpace(username)) return 0;
            var user = _db.Users.FirstOrDefault(u => u.Username == username);
            return user?.Id ?? 0;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
