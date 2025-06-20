using System;
using System.Linq;
using System.Web.Mvc;
using MZDNETWORK.Models;

namespace MZDNETWORK.Controllers
{
    [Authorize]
    public class DailyMoodController : Controller
    {
        private readonly MZDNETWORKContext _db = new MZDNETWORKContext();

        [HttpPost]
        public ActionResult Submit(int mood, string comment)
        {
            var username = User.Identity.Name;
            var user = _db.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
            {
                return new HttpStatusCodeResult(401);
            }

            var userId = user.Id;

            var today = DateTime.Today;

            // Check if user already submitted today
            var existing = _db.DailyMoods.FirstOrDefault(m => m.UserId == userId && m.Date == today);
            if (existing != null)
            {
                existing.Mood = mood;
                existing.Comment = comment;
            }
            else
            {
                var dailyMood = new DailyMood
                {
                    UserId = userId,
                    Date = today,
                    Mood = mood,
                    Comment = comment
                };
                _db.DailyMoods.Add(dailyMood);
            }
            _db.SaveChanges();
            return Json(new { success = true });
        }
    }
} 