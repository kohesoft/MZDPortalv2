using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MZDNETWORK.Models;
using MZDNETWORK.Helpers;
using MZDNETWORK.Data;
using MZDNETWORK.Attributes;
using static MZDNETWORK.Helpers.DynamicPermissionHelper;

namespace MZDNETWORK.Controllers
{
    [DynamicAuthorize(Permission = "UserManagement.UserManagement")]
    public class Kullanici_IslemleriController : Controller
    {
        private MZDNETWORKContext db = new MZDNETWORKContext();

        public Kullanici_IslemleriController()
        {
            // Prodüksiyon ortamında daha güvenilir davranış için
            db.Configuration.AutoDetectChangesEnabled = true;
            db.Configuration.ValidateOnSaveEnabled = true;
            db.Configuration.LazyLoadingEnabled = false; // Lazy loading'i devre dışı bırak
        }

        // GET: IK_Kullanici
        [DynamicAuthorize(Permission = "UserManagement.UserManagement")]
        public ActionResult Index()
        {
            // Fresh context ile temiz veri al - cache problemlerini önlemek için
            using (var freshContext = new MZDNETWORKContext())
            {
                var users = freshContext.Users
                    .Include(u => u.UserInfo)
                    .Include(u => u.UserRoles.Select(ur => ur.Role))
                    .AsNoTracking() // Read-only için tracking'i devre dışı bırak
                    .ToList();
                    
                return View(users);
            }
        }

        // GET: IK_Kullanici/Details/5
        [DynamicAuthorize(Permission = "UserManagement.UserManagement")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Include(u => u.UserInfo).FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: IK_Kullanici/Create
        [DynamicAuthorize(Permission = "UserManagement.UserManagement", Action = "Create")]
        public ActionResult Create()
        {
            // Tüm rolleri ViewBag'e ekle (dinamik olarak)
            ViewBag.AvailableRoles = RoleHelper.GetAllRoles();
            return View();
        }

        // POST: IK_Kullanici/Create
        // Aşırı gönderim saldırılarından korunmak için, bağlamak istediğiniz belirli özellikleri etkinleştirin, 
        // daha fazla bilgi için bkz. https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DynamicAuthorize(Permission = "UserManagement.UserManagement", Action = "Create")]
        public ActionResult Create([Bind(Include = "Id,Username,Password,Role,Name,Surname,Department,Position,Intercom,PhoneNumber,InternalEmail,ExternalEmail,Sicil")] User user, [Bind(Include = "Email,RealPhoneNumber,Adres,Adres2,Sehir,Ulke,Postakodu,KanGrubu,DogumTarihi,Cinsiyet,MedeniDurum")] UserInfo userInfo, string selectedRole)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    user.UserInfo = new List<UserInfo> { userInfo };
                    db.Users.Add(user);
                    db.SaveChanges();

                    // Seçilen rolü kullanıcıya ata (yeni multiple roles sistemi)
                    if (!string.IsNullOrEmpty(selectedRole))
                    {
                        bool roleAssigned = RoleHelper.AssignRoleToUser(user.Id, selectedRole);
                        if (!roleAssigned)
                        {
                            TempData["ErrorMessage"] = $"Rol '{selectedRole}' atanamadı.";
                        }
                        else
                        {
                            TempData["SuccessMessage"] = "Kullanıcı başarıyla oluşturuldu ve rol atandı.";
                        }
                    }
                    else
                    {
                        TempData["SuccessMessage"] = "Kullanıcı başarıyla oluşturuldu.";
                    }

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Kullanıcı oluşturulurken bir hata oluştu: " + ex.Message);
                }
            }

            // Hata durumunda rolleri tekrar yükle
            ViewBag.AvailableRoles = RoleHelper.GetAllRoles();
            return View(user);
        }

        // GET: IK_Kullanici/Edit/5
        [DynamicAuthorize(Permission = "UserManagement.UserManagement", Action = "Edit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Include(u => u.UserInfo).FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: IK_Kullanici/Edit/5
        // Aşırı gönderim saldırılarından korunmak için, bağlamak istediğiniz belirli özellikleri etkinleştirin, 
        // daha fazla bilgi için bkz. https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DynamicAuthorize(Permission = "UserManagement.UserManagement", Action = "Edit")]
        public ActionResult Edit([Bind(Include = "Id,Username,Password,Name,Surname,Department,Position,Intercom,PhoneNumber,InternalEmail,ExternalEmail,Sicil")] User user, [Bind(Include = "Id,Email,RealPhoneNumber,Adres,Adres2,Sehir,Ulke,Postakodu,KanGrubu,DogumTarihi,Cinsiyet,MedeniDurum")] UserInfo userInfo)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Veritabanından mevcut kullanıcıyı al
                    var existingUser = db.Users.Include(u => u.UserInfo).FirstOrDefault(u => u.Id == user.Id);
                    if (existingUser == null)
                    {
                        return HttpNotFound();
                    }

                    // User bilgilerini güncelle
                    existingUser.Username = user.Username;
                    existingUser.Password = user.Password;
                    existingUser.Name = user.Name;
                    existingUser.Surname = user.Surname;
                    existingUser.Department = user.Department;
                    existingUser.Position = user.Position;
                    existingUser.Intercom = user.Intercom;
                    existingUser.PhoneNumber = user.PhoneNumber;
                    existingUser.InternalEmail = user.InternalEmail;
                    existingUser.ExternalEmail = user.ExternalEmail;
                    existingUser.Sicil = user.Sicil;

                    // UserInfo varsa güncelle, yoksa oluştur
                    var existingUserInfo = existingUser.UserInfo.FirstOrDefault();
                    if (existingUserInfo != null)
                    {
                        // Mevcut UserInfo'yu güncelle
                        existingUserInfo.Email = userInfo.Email;
                        existingUserInfo.RealPhoneNumber = userInfo.RealPhoneNumber;
                        existingUserInfo.Adres = userInfo.Adres;
                        existingUserInfo.Adres2 = userInfo.Adres2;
                        existingUserInfo.Sehir = userInfo.Sehir;
                        existingUserInfo.Ulke = userInfo.Ulke;
                        existingUserInfo.Postakodu = userInfo.Postakodu;
                        existingUserInfo.KanGrubu = userInfo.KanGrubu;
                        existingUserInfo.DogumTarihi = userInfo.DogumTarihi;
                        existingUserInfo.Cinsiyet = userInfo.Cinsiyet;
                        existingUserInfo.MedeniDurum = userInfo.MedeniDurum;
                    }
                    else
                    {
                        // Yeni UserInfo oluştur
                        userInfo.UserId = user.Id;
                        existingUser.UserInfo.Add(userInfo);
                    }

                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Kullanıcı bilgileri başarıyla güncellendi.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Kullanıcı güncellenirken bir hata oluştu: " + ex.Message;
                    return View(user);
                }
            }

            return View(user);
        }

        // GET: IK_Kullanici/Delete/5
        [DynamicAuthorize(Permission = "UserManagement.UserManagement", Action = "Delete")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Include(u => u.UserInfo).FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: IK_Kullanici/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [DynamicAuthorize(Permission = "UserManagement.UserManagement", Action = "Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // Veritabanından fresh bir instance al
                    User user = db.Users
                        .Include(u => u.UserInfo)
                        .Include(u => u.UserRoles)
                        .FirstOrDefault(u => u.Id == id);
                    
                    if (user != null)
                    {
                        // Eğer silinen kullanıcı aktif oturumu olan kullanıcıysa, oturumu sonlandır
                        if (User.Identity.Name == user.Username)
                        {
                            System.Web.Security.FormsAuthentication.SignOut();
                        }

                        // İlişkili diğer tabloları da kontrol et ve temizle
                        
                        // 1. PermissionCache kayıtlarını sil
                        var permissionCaches = db.PermissionCaches.Where(pc => pc.UserId == id).ToList();
                        foreach (var cache in permissionCaches)
                        {
                            db.PermissionCaches.Remove(cache);
                        }

                        // 2. Notification kayıtlarını sil
                        var notifications = db.Notifications.Where(n => n.UserId == id.ToString()).ToList();
                        foreach (var notification in notifications)
                        {
                            db.Notifications.Remove(notification);
                        }

                        // 3. ChatMessage kayıtlarını soft delete yap (silinmişe işaretle)
                        var chatMessages = db.ChatMessages.Where(cm => cm.UserId == id).ToList();
                        foreach (var message in chatMessages)
                        {
                            message.DeletedAt = DateTime.Now;
                            message.DeletedBy = User.Identity.Name == user.Username ? (int?)null : id;
                        }

                        // 4. ChatGroup üyeliklerini temizle
                        var chatGroupMemberships = db.ChatGroupMembers.Where(cgm => cgm.UserId == id).ToList();
                        foreach (var membership in chatGroupMemberships)
                        {
                            db.ChatGroupMembers.Remove(membership);
                        }

                        // 5. Task kayıtlarını sil (UserId nullable olmadığı için)
                        var userTasks = db.Tasks.Where(t => t.UserId == id).ToList();
                        foreach (var task in userTasks)
                        {
                            // İlişkili TodoItems'ları da sil
                            var todoItems = db.TodoItems.Where(ti => ti.TaskId == task.Id).ToList();
                            foreach (var todoItem in todoItems)
                            {
                                db.TodoItems.Remove(todoItem);
                            }
                            
                            // Task'ı sil
                            db.Tasks.Remove(task);
                        }

                        // 6. UserRole kayıtlarını sil
                        if (user.UserRoles != null && user.UserRoles.Any())
                        {
                            var userRolesToDelete = user.UserRoles.ToList();
                            foreach (var userRole in userRolesToDelete)
                            {
                                db.UserRoles.Remove(userRole);
                            }
                        }

                        // 7. ServicePersonnel kayıtlarını sil (if user is assigned to any service)
                        var servicePersonnelRecords = db.ServicePersonnels.Where(sp => sp.UserId == id).ToList();
                        foreach (var servicePersonnel in servicePersonnelRecords)
                        {
                            db.ServicePersonnels.Remove(servicePersonnel);
                        }

                        // 8. DilekOneri kayıtlarını sil (Username ile eşleşen)
                        var dilekOneriler = db.DilekOneriler.Where(d => d.Username == user.Username).ToList();
                        foreach (var dilekOneri in dilekOneriler)
                        {
                            db.DilekOneriler.Remove(dilekOneri);
                        }

                        // 9. Survey Answers kayıtlarını sil (UserID ile eşleşen)
                        var userAnswers = db.Answers.Where(a => a.UserID == id).ToList();
                        foreach (var answer in userAnswers)
                        {
                            // Answer'a bağlı AnswerOption'ları da sil
                            var answerOptions = db.Set<AnswerOption>().Where(ao => ao.AnswerId == answer.ID).ToList();
                            foreach (var option in answerOptions)
                            {
                                db.Set<AnswerOption>().Remove(option);
                            }
                            
                            db.Answers.Remove(answer);
                        }

                        // 10. Reservation kayıtlarını sil
                        var reservations = db.Reservations.Where(r => r.UserId == id).ToList();
                        foreach (var reservation in reservations)
                        {
                            db.Reservations.Remove(reservation);
                        }

                        // 11. DailyMood kayıtlarını sil
                        var dailyMoods = db.DailyMoods.Where(dm => dm.UserId == id).ToList();
                        foreach (var mood in dailyMoods)
                        {
                            db.DailyMoods.Remove(mood);
                        }

                        // 12. LeaveRequest kayıtlarını sil
                        var leaveRequests = db.LeaveRequests.Where(lr => lr.UserId == id).ToList();
                        foreach (var leaveRequest in leaveRequests)
                        {
                            db.LeaveRequests.Remove(leaveRequest);
                        }

                        // 13. MeetingRoomReservation kayıtlarını sil
                        var meetingRoomReservations = db.MeetingRoomReservations.Where(mrr => mrr.UserId == id).ToList();
                        foreach (var reservation in meetingRoomReservations)
                        {
                            db.MeetingRoomReservations.Remove(reservation);
                        }

                        // 14. BeyazTahtaEntry kayıtlarını sil
                        var beyazTahtaEntries = db.BeyazTahtaEntries.Where(bte => bte.UserId == id).ToList();
                        foreach (var entry in beyazTahtaEntries)
                        {
                            db.BeyazTahtaEntries.Remove(entry);
                        }

                        // 15. SuggestionComplaint kayıtlarını sil
                        var suggestionComplaints = db.SuggestionComplaints.Where(sc => sc.UserId == id).ToList();
                        foreach (var complaint in suggestionComplaints)
                        {
                            db.SuggestionComplaints.Remove(complaint);
                        }

                        // 16. VisitorEntry kayıtlarını sil (FullName ile eşleşen)
                        try
                        {
                            var visitorEntries = db.VisitorEntries.Where(ve => ve.FullName.Contains(user.Name) && ve.FullName.Contains(user.Surname)).ToList();
                            foreach (var entry in visitorEntries)
                            {
                                db.VisitorEntries.Remove(entry);
                            }
                        }
                        catch (Exception ex)
                        {
                            // VisitorEntry temizleme hatası
                            System.Diagnostics.Debug.WriteLine($"VisitorEntry temizleme hatası: {ex.Message}");
                        }

                        // 17. LateArrivalReport kayıtlarını sil (FullName ile eşleşen)
                        try
                        {
                            var lateArrivalReports = db.LateArrivalReports.Where(lar => lar.FullName.Contains(user.Name) && lar.FullName.Contains(user.Surname)).ToList();
                            foreach (var report in lateArrivalReports)
                            {
                                db.LateArrivalReports.Remove(report);
                            }
                        }
                        catch (Exception ex)
                        {
                            // LateArrivalReport temizleme hatası
                            System.Diagnostics.Debug.WriteLine($"LateArrivalReport temizleme hatası: {ex.Message}");
                        }

                        // 18. OvertimeServicePersonnel kayıtlarını sil
                        var overtimeServicePersonnels = db.OvertimeServicePersonnels.Where(osp => osp.UserId == id).ToList();
                        foreach (var personnel in overtimeServicePersonnels)
                        {
                            db.OvertimeServicePersonnels.Remove(personnel);
                        }

                        // 19. Chat kayıtlarını güncelle (CreatedBy field'ını temizle)
                        var createdChats = db.Chats.Where(c => c.CreatedBy == id).ToList();
                        foreach (var chat in createdChats)
                        {
                            // Chat'i sil veya sahipliği başka birine devret
                            db.Chats.Remove(chat);
                        }

                        // 20. ChatGroup kayıtlarını güncelle
                        var createdChatGroups = db.ChatGroups.Where(cg => cg.CreatedBy == id).ToList();
                        foreach (var chatGroup in createdChatGroups)
                        {
                            // ChatGroup'u sil veya sahipliği başka birine devret
                            db.ChatGroups.Remove(chatGroup);
                        }

                        // 21. PasswordResetRequest kayıtlarını sil
                        var passwordResetRequests = db.PasswordResetRequests.Where(prr => prr.UserId == id).ToList();
                        foreach (var request in passwordResetRequests)
                        {
                            db.PasswordResetRequests.Remove(request);
                        }

                        // 22. UserInfo kayıtlarını sil
                        if (user.UserInfo != null && user.UserInfo.Any())
                        {
                            var userInfosToDelete = user.UserInfo.ToList();
                            foreach (var userInfo in userInfosToDelete)
                            {
                                db.UserInfos.Remove(userInfo);
                            }
                        }

                        // 23. Ana kullanıcı kaydını sil
                        db.Users.Remove(user);

                        // Tüm değişiklikleri kaydet
                        db.SaveChanges();
                        
                        // Transaction'ı commit et
                        transaction.Commit();

                        // Cache'i temizle - Kullanıcı silindikten sonra tüm cache'i temizle
                        try
                        {
                            // Belirli kullanıcının cache'ini temizle
                            InvalidateUserCache(id);
                            
                            // Güvenlik için tüm cache'i de temizle (kullanıcı silme işlemi kritik)
                            InvalidateAllCache();
                        }
                        catch (Exception cacheEx)
                        {
                            // Cache temizleme hatası kritik değil, log et ama işlemi devam ettir
                            System.Diagnostics.Debug.WriteLine($"Cache temizleme hatası: {cacheEx.Message}");
                        }

                        if (Request.IsAjaxRequest())
                        {
                            return new HttpStatusCodeResult(HttpStatusCode.OK);
                        }

                        TempData["SuccessMessage"] = "Kullanıcı ve tüm ilişkili kayıtları başarıyla silindi.";
                    }
                    else
                    {
                        transaction.Rollback();
                        TempData["ErrorMessage"] = "Silinecek kullanıcı bulunamadı.";
                    }
                    
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    
                    if (Request.IsAjaxRequest())
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
                    }

                    TempData["ErrorMessage"] = "Kullanıcı silinirken bir hata oluştu: " + ex.Message;
                    return RedirectToAction("Index");
                }
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