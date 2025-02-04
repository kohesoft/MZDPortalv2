using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MZDNETWORK.Models
{
    public class Question
    {
        public int ID { get; set; }
        public int SurveyID { get; set; }
        public string QuestionText { get; set; }
        public QuestionType Type { get; set; }

        public virtual Survey Survey { get; set; }
        public virtual ICollection<AnswerOption> AnswerOptions { get; set; }
        public virtual ICollection<Answer> Answers { get; set; } // Bu satırı güncelledik
    }

    // Enum ile soru türlerini tanımlıyoruz
    public enum QuestionType
    {
        Text,       // Kullanıcı serbest metin girebilir
        Radio,      // Tek seçenek seçebilir
        Checkbox    // Birden fazla seçenek seçebilir
    }



}