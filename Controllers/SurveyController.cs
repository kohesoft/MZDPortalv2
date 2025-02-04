using MZDNETWORK.Models;
using System.Web.Mvc;
using System;
using System.Linq;
using System.Data.Entity;

public class SurveyController : Controller
{
    private MZDNETWORKContext db = new MZDNETWORKContext();

    // Anketleri listele
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

            // Sorular ve seçenekler varsa kaydediyoruz
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
            return RedirectToAction("Index");
        }
        return View(survey);
    }

    [Authorize(Roles = "IK, Yonetici, Sys")]
    public ActionResult SurveyResults(int surveyId)
    {
        var survey = db.Surveys
            .Include(s => s.Questions.Select(q => q.Answers)) // 🔹 Doğru kullanım
            .FirstOrDefault(s => s.ID == surveyId);

        if (survey == null)
            return HttpNotFound();

        return View(survey);
    }
    public JsonResult GetUsernames()
    {
        var usernames = db.Users.Select(u => new { u.Id, u.Username }).ToList();
        return Json(usernames, JsonRequestBehavior.AllowGet);
    }
}
