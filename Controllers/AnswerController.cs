using Microsoft.AspNet.Identity;
using MZDNETWORK.Models;
using MZDNETWORK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MZDNETWORK.Attributes;

[DynamicAuthorize(Permission = "Operasyon.Anket")]
public class AnswerController : Controller
{
    // Sınıf genelinde Survey görüntüleme izni
    private MZDNETWORKContext db = new MZDNETWORKContext();

    // Kullanıcı anketi doldururken
    [DynamicAuthorize(Permission = "Operasyon.Anket")]
    public ActionResult TakeSurvey(int surveyId)
    {
        var survey = db.Surveys
            .Include("Questions.AnswerOptions") // Sorular ve seçenekler dahil ediliyor
            .FirstOrDefault(s => s.ID == surveyId);

        if (survey == null)
            return HttpNotFound();

        // Anketin süresinin dolup dolmadığını kontrol et
        if (survey.EndDate < DateTime.Now)
        {
            return RedirectToAction("Index", "Survey", new { error = "Bu anketin süresi dolmuştur." });
        }

        return View(survey);
    }

    [DynamicAuthorize(Permission = "Operasyon.Anket", Action = "Create")]
    [HttpPost]
    public ActionResult SubmitSurvey(FormCollection form)
    {
        int surveyId = int.Parse(form["surveyId"]);
        string username = User.Identity.Name;

        // Kullanıcı adını kullanarak kullanıcı ID'sini veritabanından al
        var user = db.Users.FirstOrDefault(u => u.Username == username);
        if (user == null)
        {
            // Kullanıcı bulunamazsa hata mesajı göster
            return RedirectToAction("TakeSurvey", new { surveyId, error = "Geçersiz kullanıcı." });
        }
        int userId = user.Id;

        var survey = db.Surveys.FirstOrDefault(s => s.ID == surveyId);
        if (survey == null)
        {
            return HttpNotFound();
        }

        // Anketin süresinin dolup dolmadığını kontrol et
        if (survey.EndDate < DateTime.Now)
        {
            return RedirectToAction("Index", "Survey", new { error = "Bu anketin süresi dolmuştur." });
        }

        var questionIds = form.AllKeys
            .Where(k => k.StartsWith("answers[") && k.EndsWith("].QuestionID"))
            .Select(k => int.Parse(form[k]))
            .ToList();
        var answers = form.AllKeys
            .Where(k => k.StartsWith("answers[") && k.EndsWith("].AnswerText"))
            .Select(k => form[k])
            .ToList();

        bool hasAnswered = db.Answers.Any(a => a.UserID == userId && a.Question.SurveyID == surveyId);
        if (hasAnswered)
        {
            ModelState.AddModelError("", "Bu anketi daha önce yanıtladınız.");
            return RedirectToAction("TakeSurvey", new { surveyId });
        }

        foreach (var i in Enumerable.Range(0, questionIds.Count))
        {
            var answer = new Answer
            {
                QuestionID = questionIds[i],
                AnswerText = answers[i],
                UserID = userId
            };

            db.Answers.Add(answer);
        }

        db.SaveChanges();

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public JsonResult HasAnsweredSurvey(int surveyId)
    {
        string userIdString = User.Identity.GetUserId();
        if (string.IsNullOrEmpty(userIdString))
        {
            // Eğer GetUserId() boş dönerse, User.Identity.Name kullanarak kullanıcı ID'sini al
            string username = User.Identity.Name;
            var user = db.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
            {
                throw new ArgumentException("User ID cannot be null or empty");
            }
            userIdString = user.Id.ToString();
        }

        int userId = int.Parse(userIdString);
        var hasAnswered = db.Answers.Any(a => a.Question.SurveyID == surveyId && a.UserID == userId);
        return Json(hasAnswered, JsonRequestBehavior.AllowGet);
    }
}

