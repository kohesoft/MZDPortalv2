using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using MZDNETWORK.Models;

namespace MZDNETWORK.Controllers
{
    public class TaskController : Controller
    {
        private readonly MZDNETWORKContext _context;

        public TaskController()
        {
            _context = new MZDNETWORKContext();
        }

        public ActionResult Index()
        {
            var tasks = _context.Tasks.Include("User").Include("TodoItems").ToList();
            return View(tasks);
        }

        public ActionResult Create()
        {
            ViewData["Username"] = new SelectList(_context.Users, "Username", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async System.Threading.Tasks.Task<ActionResult> Create(MZDNETWORK.Models.Task task, string[] todoDescriptions)
        {
            if (ModelState.IsValid)
            {
                if (todoDescriptions != null)
                {
                    task.TodoItems = todoDescriptions.Select(desc => new TodoItem { Description = desc, IsCompleted = false }).ToList();
                }

                _context.Tasks.Add(task);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.Users = new SelectList(_context.Users, "Username", "Name", task.Username);
            return View(task);
        }

        public ActionResult Edit(int id)
        {
            var task = _context.Tasks.Include("TodoItems").FirstOrDefault(t => t.Id == id);
            if (task == null)
            {
                return HttpNotFound();
            }

            ViewBag.Username = new SelectList(_context.Users, "Username", "Name", task.Username);
            ViewBag.TodoItems = task.TodoItems; // Mevcut TodoItems öðelerini ViewBag'e ekle
            return View(task);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async System.Threading.Tasks.Task<ActionResult> Edit(MZDNETWORK.Models.Task task, string[] todoDescriptions)
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
                        // Mevcut TodoItems öðelerini sil
                        _context.TodoItems.RemoveRange(existingTask.TodoItems);

                        // Yeni TodoItems öðelerini ekle
                        foreach (var desc in todoDescriptions)
                        {
                            existingTask.TodoItems.Add(new TodoItem { Description = desc, IsCompleted = false });
                        }
                    }

                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("Index");
            }

            ViewBag.Users = new SelectList(_context.Users, "Username", "Name", task.Username);
            ViewBag.TodoItems = task.TodoItems; // Mevcut TodoItems öðelerini ViewBag'e ekle
            return View(task);
        }



        public ActionResult Details(int id)
        {
            var task = _context.Tasks.Include("User").Include("TodoItems").FirstOrDefault(t => t.Id == id);
            if (task == null)
            {
                return HttpNotFound();
            }

            return View(task);
        }

        public ActionResult Delete(int id)
        {
            var task = _context.Tasks.FirstOrDefault(t => t.Id == id);
            if (task == null)
            {
                return HttpNotFound();
            }

            return View(task);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async System.Threading.Tasks.Task<ActionResult> DeleteConfirmed(int id)
        {
            var task = _context.Tasks.FirstOrDefault(t => t.Id == id);
            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async System.Threading.Tasks.Task<ActionResult> UpdateProgress(int taskId, int todoItemId, bool isCompleted)
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

        // Kullanýcýlarýn görevlerini listelemek için yeni bir action
        public ActionResult UserTasks()
        {
            var username = HttpContext.User.Identity.Name;
            if (string.IsNullOrEmpty(username))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Username is required");
            }

            var tasks = _context.Tasks.Include("TodoItems").Where(t => t.Username == username).ToList();
            ViewBag.Username = username; // Kullanýcý adýný ViewBag'e ekle
            return View(tasks);
        }

    }
}
