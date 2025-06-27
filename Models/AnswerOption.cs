using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MZDNETWORK.Models
{
    public class AnswerOption
    {
        public int ID { get; set; }
        public int QuestionID { get; set; }
        public string OptionText { get; set; }  // Örnek: "Evet", "Hayır"

        public virtual Question Question { get; set; }
        
        // Answer relationship
        public int AnswerId { get; set; }
        
        [ForeignKey("AnswerId")]
        public virtual Answer Answer { get; set; }
    }
}