using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using MZDNETWORK.Models;
using System.Data.Entity;

namespace MZDNETWORK.Controllers
{
    [Authorize(Roles = "IK, Yonetici, Sys, IdariIsler, BilgiIslem, Lider, Merkez, Yerleske, Dokumantasyon")]

    public class TaskController : Controller
    {
        private readonly MZDNETWORKContext _context;

        public TaskController()
        {
            _context = new MZDNETWORKContext();
        }

        [Authorize(Roles = "BilgiIslem, Yonetici, Sys,IK,Lider,IdariIsler")]
        public ActionResult Index()
        {
            var currentUsername = User.Identity.Name; // Oturum açmı kullanıcının kullanıcı adını al
            var tasks = _context.Tasks.Where(item => item.CreatedBy == currentUsername).ToList(); // Modeli filtrele ve listeye dönüştür
            return View(tasks);
        }

        [Authorize(Roles = "BilgiIslem, Yonetici, Sys,IK,Lider,IdariIsler")]
        public ActionResult Create()
        {
            ViewData["Username"] = new SelectList(_context.Users, "Username", "Username");
            return View();
        }

        [Authorize(Roles = "BilgiIslem, Yonetici, Sys,IK,Lider,IdariIsler")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(MZDNETWORK.Models.Task task, string[] todoDescriptions, string[] additionalDescriptions, DateTime[] dueDates)
        {
            if (ModelState.IsValid)
            {
                task.CreatedBy = User.Identity.Name;

                var user = _context.Users.FirstOrDefault(u => u.Username == task.Username);
                if (user != null)
                {
                    task.UserId = user.Id;
                }
                else
                {
                    ModelState.AddModelError("", "Geçersiz kullanıcı adı.");
                    ViewBag.Users = new SelectList(_context.Users, "Username", "Name", task.Username);
                    return View(task);
                }

                if (todoDescriptions != null)
                {
                    task.TodoItems = todoDescriptions.Select((desc, index) => new TodoItem
                    {
                        Description = desc,
                        AdditionalDescription = additionalDescriptions[index],
                        DueDate = dueDates[index],
                        IsCompleted = false
                    }).ToList();
                }

                _context.Tasks.Add(task);
                await _context.SaveChangesAsync();

                var notification = new Notification
                {
                    UserId = task.UserId.ToString(),
                    Message = "Yeni bir görev atandı.",
                    IsRead = false,
                    CreatedDate = DateTime.Now
                };
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            ViewBag.Users = new SelectList(_context.Users, "Username", "Name", task.Username);
            return View(task);
        }





        [Authorize(Roles = "BilgiIslem, Yonetici, Sys,IK,Lider,IdariIsler")]
        public ActionResult Edit(int id)
        {
            var task = _context.Tasks.Include("TodoItems").FirstOrDefault(t => t.Id == id);
            if (task == null)
            {
                return HttpNotFound();
            }

            ViewBag.Username = new SelectList(_context.Users, "Username", "Name", task.Username);
            ViewBag.TodoItems = task.TodoItems; // Mevcut TodoItems ��elerini ViewBag'e ekle
            return View(task);
        }
        [Authorize(Roles = "BilgiIslem, Yonetici, Sys,IK,Lider,IdariIsler")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(MZDNETWORK.Models.Task task, string[] todoDescriptions, string[] additionalDescriptions, DateTime[] dueDates)
        {
            if (ModelState.IsValid)
            {
                var existingTask = _context.Tasks.Include("TodoItems").FirstOrDefault(t => t.Id == task.Id);
                if (existingTask != null)
                {
                    existingTask.Title = task.Title;
                    existingTask.Description = task.Description;
                    existingTask.StartDate = task.StartDate;
                    existingTask.EndDate = task.EndDate;
                    existingTask.Username = task.Username;

                    if (todoDescriptions != null)
                    {
                        // Mevcut TodoItems ��elerini sil
                        _context.TodoItems.RemoveRange(existingTask.TodoItems);

                        // Yeni TodoItems ��elerini ekle
                        for (int i = 0; i < todoDescriptions.Length; i++)
                        {
                            existingTask.TodoItems.Add(new TodoItem
                            {
                                Description = todoDescriptions[i],
                                AdditionalDescription = additionalDescriptions[i],
                                DueDate = dueDates[i],
                                IsCompleted = false
                            });
                        }
                    }

                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("Index");
            }

            ViewBag.Users = new SelectList(_context.Users, "Username", "Name", task.Username);
            ViewBag.TodoItems = task.TodoItems; // Mevcut TodoItems ��elerini ViewBag'e ekle
            return View(task);
        }

        public ActionResult Details(int id)
        {
            var task = _context.Tasks.Include("User").Include("TodoItems").FirstOrDefault(t => t.Id == id);
            if (task == null)
            {
                return HttpNotFound();
            }

            ViewBag.Username = task.Username; // Username bilgisini ViewBag'e ekle
            return View(task);
        }
        [Authorize(Roles = "BilgiIslem, Yonetici, Sys,IK,Lider,IdariIsler")]
        public ActionResult Delete(int id)
        {
            var task = _context.Tasks.FirstOrDefault(t => t.Id == id);
            if (task == null)
            {
                return HttpNotFound();
            }

            ViewBag.Username = task.Username; // Username bilgisini ViewBag'e ekle
            return View(task);
        }
        [Authorize(Roles = "BilgiIslem, Yonetici, Sys,IK,Lider,IdariIsler")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var task = _context.Tasks
                .Include(t => t.TodoItems)
                .FirstOrDefault(t => t.Id == id);

            if (task != null)
            {
                // İlişkili TodoItems'ları sil
                if (task.TodoItems != null)
                {
                    _context.TodoItems.RemoveRange(task.TodoItems);
                }

                // İlişkili bildirimleri sil
                var notifications = _context.Notifications
                    .Where(n => n.Message.Contains("görev") && n.UserId == task.UserId.ToString())
                    .ToList();
                _context.Notifications.RemoveRange(notifications);

                // Görevi sil
                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<ActionResult> UpdateProgress(int taskId, int todoItemId, bool isCompleted)
        {
            var todoItem = _context.TodoItems.FirstOrDefault(t => t.Id == todoItemId);
            if (todoItem == null)
            {
                return HttpNotFound();
            }

            todoItem.IsCompleted = isCompleted;
            await _context.SaveChangesAsync();

            var task = _context.Tasks.Include("TodoItems").FirstOrDefault(t => t.Id == taskId);
            if (task != null)
            {
                task.Progress = (int)((double)task.TodoItems.Count(t => t.IsCompleted) / task.TodoItems.Count() * 100);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Details", new { id = taskId });
        }

        [HttpPost]
        public async Task<ActionResult> UpdateAdditionalDescription(int taskId, int todoItemId, string additionalDescription)
        {
            if (string.IsNullOrEmpty(additionalDescription))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Additional description cannot be null or empty.");
            }

            var todoItem = _context.TodoItems.FirstOrDefault(t => t.Id == todoItemId);
            if (todoItem == null)
            {
                return HttpNotFound();
            }

            todoItem.AdditionalDescription = additionalDescription;
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = taskId });
        }

        // Kullanıcıların görevlerini listelemek için yeni bir action
        public ActionResult UserTasks()
        {
            var username = HttpContext.User.Identity.Name;
            if (string.IsNullOrEmpty(username))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Username is required");
            }

            var tasks = _context.Tasks.Include("TodoItems").Where(t => t.Username == username).ToList();
            ViewBag.Username = username; // Kullanıcı adını ViewBag'e ekle
            return View(tasks);
        }
    }
}