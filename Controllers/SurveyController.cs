using MZDNETWORK.Models;
using System.Web.Mvc;
using System;
using System.Linq;
using System.Data.Entity;

public class SurveyController : Controller
{
    private MZDNETWORKContext db = new MZDNETWORKContext();

    // Anketleri listele
    [Authorize(Roles = "IK, Yonetici, Sys, IdariIsler, BilgiIslem, Lider, Merkez, Yerleske, Dokumantasyon")]
    public ActionResult Index()
    {
        var surveys = db.Surveys.ToList();
        return View(surveys);
    }

    // Yeni anket oluşturma GET
    [Authorize(Roles = "IK, Yonetici, Sys")]
    public ActionResult Create()
    {
        return View();
    }

    // Yeni anket oluşturma POST
    [Authorize(Roles = "IK, Yonetici, Sys")]
    [HttpPost]
    public ActionResult Create(Survey survey)
    {
        if (ModelState.IsValid)
        {
            survey.CreatedDate = DateTime.Now;
            survey.EndDate = survey.CreatedDate.AddMinutes(survey.Duration); // Bitiş tarihini hesapla

            foreach (var question in survey.Questions)
            {
                db.Questions.Add(question);
                if (question.AnswerOptions != null)
                {
                    foreach (var option in question.AnswerOptions)
                    {
                        db.Entry(option).State = EntityState.Added;
                    }
                }
            }

            db.Surveys.Add(survey);
            db.SaveChanges();

            var users = db.Users.ToList();
            foreach (var user in users)
            {
                var notification = new Notification
                {
                    UserId = user.Id.ToString(),
                    Message = "Yeni bir anket oluşturuldu.",
                    IsRead = false,
                    CreatedDate = DateTime.Now
                };
                db.Notifications.Add(notification);
            }
            db.SaveChanges();

            return RedirectToAction("Index");
        }
        return View(survey);
    }


    [Authorize(Roles = "IK, Yonetici, Sys")]
    public ActionResult SurveyResults(int surveyId)
    {
        var survey = db.Surveys
            .Include(s => s.Questions.Select(q => q.Answers.Select(a => a.Question))) // <-- Bunu ekleyin
            .FirstOrDefault(s => s.ID == surveyId);

        if (survey == null)
            return HttpNotFound();

        return View(survey);
    }

    [Authorize(Roles = "Yonetici, Sys, IK")]
    public ActionResult Delete(int id)
    {
        var survey = db.Surveys.Find(id);
        if (survey == null)
        {
            return HttpNotFound();
        }

        db.Surveys.Remove(survey);
        db.SaveChanges();

        return RedirectToAction("Index");
    }

    public JsonResult GetUsernames()
    {
        var usernames = db.Users.Select(u => new { u.Id, u.Username }).ToList();
        return Json(usernames, JsonRequestBehavior.AllowGet);
    }
}
