using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MZDNETWORK.Models
{
    public class Answer
    {
        public int ID { get; set; }
        public int QuestionID { get; set; }
        public int UserID { get; set; } // Eğer kullanıcı takibi yapacaksanız
        public string AnswerText { get; set; }

        public virtual Question Question { get; set; }
        
        // Navigation property for AnswerOptions
        public virtual ICollection<AnswerOption> Options { get; set; }
        
        public Answer()
        {
            Options = new HashSet<AnswerOption>();
        }
    }
}