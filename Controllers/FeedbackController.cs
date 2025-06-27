using System;
using System.Web.Mvc;
using MZDNETWORK.Models;
using MZDNETWORK.Data;
using System.Linq;
using MZDNETWORK.Attributes;

public class FeedbackController : Controller
{
    private MZDNETWORKContext db = new MZDNETWORKContext();

    [HttpPost]
    public ActionResult SubmitFeedback(bool liked, string suggestion)
    {
        string userIpAddress = Request.UserHostAddress;

        var feedback = new Feedback
        {
            Liked = liked,
            Suggestion = suggestion,
            CreatedAt = DateTime.Now,
            IpAddress = userIpAddress
        };

        db.Feedbacks.Add(feedback);
        db.SaveChanges();

        return Json(new { success = true });
    }

    [HttpGet]
    public ActionResult HasFeedback()
    {
        string userIpAddress = Request.UserHostAddress;
        bool hasFeedback = db.Feedbacks.Any(f => f.IpAddress == userIpAddress);
        return Json(new { hasFeedback = hasFeedback }, JsonRequestBehavior.AllowGet);
    }
}
