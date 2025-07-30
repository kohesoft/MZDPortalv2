using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MZDNETWORK.Models;
using MZDNETWORK.Attributes;
using MZDNETWORK.Data; // ApplicationDbContext için ekleniyor

namespace MZDNETWORK.Controllers
{
    [DynamicAuthorize(Permission = "Operational.Chat", Action = "Manage")]
    public class ChatGroupController : Controller
    {
        private readonly MZDNETWORK.Data.MZDNETWORKContext _db = new MZDNETWORK.Data.MZDNETWORKContext();

        // Grup listesi
        public ActionResult Index()
        {
            var groups = _db.ChatGroups.Where(g => g.IsActive).ToList();
            return View(groups);
        }

        // Grup oluþturma
        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.Roles = _db.Roles.Where(r => r.IsActive).ToList();
            ViewBag.Users = _db.Users.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ChatGroup model, int[] allowedRoleIds, int[] managerIds, int[] memberIds)
        {
            if (ModelState.IsValid)
            {
                model.CreatedBy = 1; // Mevcut kullanýcý ID
                model.CreatedAt = System.DateTime.Now;
                model.IsActive = true;
                model.AllowedRoles = new List<Role>();
                model.Managers = new List<User>();
                model.Members = new List<User>();
                foreach (var role in _db.Roles.Where(r => allowedRoleIds.Contains(r.Id)))
                    model.AllowedRoles.Add(role);
                foreach (var user in _db.Users.Where(u => managerIds.Contains(u.Id)))
                    model.Managers.Add(user);
                foreach (var user in _db.Users.Where(u => memberIds.Contains(u.Id)))
                    model.Members.Add(user);
                _db.ChatGroups.Add(model);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Roles = _db.Roles.Where(r => r.IsActive).ToList();
            ViewBag.Users = _db.Users.ToList();
            return View(model);
        }

        // Grup düzenleme
        [HttpGet]
        public ActionResult Edit(int id)
        {
            var group = _db.ChatGroups.Find(id);
            if (group == null) return HttpNotFound();
            ViewBag.Roles = _db.Roles.Where(r => r.IsActive).ToList();
            ViewBag.Users = _db.Users.ToList();
            return View(group);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ChatGroup model, int[] allowedRoleIds, int[] managerIds, int[] memberIds)
        {
            var group = _db.ChatGroups.Find(model.Id);
            if (group == null) return HttpNotFound();
            if (ModelState.IsValid)
            {
                group.Name = model.Name;
                group.Description = model.Description;
                // Koleksiyonlarý önce temizle
                group.AllowedRoles.Clear();
                group.Managers.Clear();
                group.Members.Clear();
                // Sonra ekle
                foreach (var role in _db.Roles.Where(r => allowedRoleIds.Contains(r.Id)))
                    group.AllowedRoles.Add(role);
                foreach (var user in _db.Users.Where(u => managerIds.Contains(u.Id)))
                    group.Managers.Add(user);
                foreach (var user in _db.Users.Where(u => memberIds.Contains(u.Id)))
                    group.Members.Add(user);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Roles = _db.Roles.Where(r => r.IsActive).ToList();
            ViewBag.Users = _db.Users.ToList();
            return View(model);
        }

        // Grup silme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var group = _db.ChatGroups.Find(id);
            if (group == null) return HttpNotFound();
            group.IsActive = false;
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
