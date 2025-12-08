using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.SignalR;
using MZDNETWORK.Hubs;
using MZDNETWORK.Attributes;
using MZDNETWORK.Models;
using MZDNETWORK.Data;

namespace MZDNETWORK.Controllers
{
    [DynamicAuthorize(Permission = "Operasyon.Sohbet")]
    public class ChatController : Controller
    {
        private readonly IHubContext _hubContext;
        private readonly MZDNETWORKContext _db = new MZDNETWORKContext();

        public ChatController()
        {
            _hubContext = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
        }

        [HttpPost]
        [DynamicAuthorize(Permission = "Operasyon.Sohbet", Action = "Create")]
        public ActionResult SendMessage(string user, string message, int groupId)
        {
            var dbUser = _db.Users.FirstOrDefault(u => u.Username == user);
            if (dbUser == null) return new HttpStatusCodeResult(403);
            var chatGroup = _db.ChatGroups.FirstOrDefault(g => g.Id == groupId);
            if (chatGroup == null) return new HttpStatusCodeResult(404);
            // Yetki kontrolü: ChatGroupMember tablosunda CanWrite kontrolü
            var groupMember = _db.ChatGroupMembers.FirstOrDefault(m => m.ChatGroupId == groupId && m.UserId == dbUser.Id);
            if (groupMember == null || !groupMember.CanWrite)
                return new HttpStatusCodeResult(403, "Bu grupta yazma yetkiniz yok.");
            var chatMessage = new ChatMessage
            {
                ChatGroupId = groupId,
                UserId = dbUser.Id,
                Content = message,
                SentAt = DateTime.Now,
                IsActive = true
            };
            _db.ChatMessages.Add(chatMessage);
            _db.SaveChanges();
            _hubContext.Clients.Group(groupId.ToString()).broadcastMessage(user, message);
            return new HttpStatusCodeResult(200);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadFile(int groupId, HttpPostedFileBase file)
        {
            var username = User.Identity.Name;
            var dbUser = _db.Users.FirstOrDefault(u => u.Username == username);
            if (dbUser == null) return new HttpStatusCodeResult(403);
            var chatGroup = _db.ChatGroups.FirstOrDefault(g => g.Id == groupId);
            if (chatGroup == null) return new HttpStatusCodeResult(404);
            var groupMember = _db.ChatGroupMembers.FirstOrDefault(m => m.ChatGroupId == groupId && m.UserId == dbUser.Id);
            if (groupMember == null || !groupMember.CanWrite)
                return new HttpStatusCodeResult(403, "Bu grupta dosya göndermek için yetkiniz yok.");
            if (file == null || file.ContentLength == 0)
                return new HttpStatusCodeResult(400, "Dosya seçilmedi.");
            var uploadsDir = "/Uploads/ChatFiles/" + groupId;
            var serverPath = Server.MapPath(uploadsDir);
            if (!System.IO.Directory.Exists(serverPath))
                System.IO.Directory.CreateDirectory(serverPath);
            var fileName = System.IO.Path.GetFileName(file.FileName);
            var filePath = System.IO.Path.Combine(serverPath, fileName);
            file.SaveAs(filePath);
            var chatMessage = new ChatMessage
            {
                ChatGroupId = groupId,
                UserId = dbUser.Id,
                Content = "[Dosya: " + fileName + "]",
                FilePath = uploadsDir + "/" + fileName,
                FileName = fileName,
                FileSize = file.ContentLength,
                MessageType = file.ContentType.StartsWith("image/") ? "Image" : "File",
                SentAt = DateTime.Now,
                IsActive = true
            };
            _db.ChatMessages.Add(chatMessage);
            _db.SaveChanges();
            _hubContext.Clients.Group(groupId.ToString()).broadcastMessage(dbUser.Username, chatMessage.Content);
            return Json(new { success = true, fileUrl = chatMessage.FilePath, fileName = fileName, messageType = chatMessage.MessageType });
        }

        public ActionResult ChatPage()
        {
            var username = User.Identity.Name;
            var user = _db.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
                return View(new List<ChatGroup>());
            var userRoleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();
            var groups = _db.ChatGroups.Where(g => g.IsActive && (
                g.Members.Any(m => m.Id == user.Id) ||
                g.AllowedRoles.Any(r => userRoleIds.Contains(r.Id))
            )).ToList();
            return View(groups);
        }

        // Kullanıcının yetkili olduğu grupları döner
        [HttpGet]
        public ActionResult GetUserGroups()
        {
            var username = User.Identity.Name;
            var user = _db.Users.FirstOrDefault(u => u.Username == username);
            if (user == null) return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            var userRoleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();
            var groups = _db.ChatGroups.Where(g => g.IsActive && (
                g.Members.Any(m => m.Id == user.Id) ||
                g.AllowedRoles.Any(r => userRoleIds.Contains(r.Id))
            )).Select(g => new { g.Id, g.Name, g.Description }).ToList();
            return Json(groups, JsonRequestBehavior.AllowGet);
        }

        // Seçili gruba ait mesaj geçmişi
        [HttpGet]
        public ActionResult GetGroupMessages(int groupId)
        {
            var messages = _db.ChatMessages
                .Where(m => m.ChatGroupId == groupId && m.IsActive)
                .OrderBy(m => m.SentAt)
                .ToList()
                .Select(m => new {
                    UserName = m.User.Username,
                    Content = m.Content,
                    SentAt = m.SentAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    MessageType = m.MessageType,
                    FilePath = m.FilePath,
                    FileName = m.FileName
                }).ToList();
            return Json(messages, JsonRequestBehavior.AllowGet);
        }
    }
}
