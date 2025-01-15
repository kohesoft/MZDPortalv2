using System;
using System.ComponentModel.DataAnnotations;

namespace MZDNETWORK.Models
{
    public class TodoItem
    {
        [Key]
        public int Id { get; set; }
        public string Description { get; set; }
        public string AdditionalDescription { get; set; } // Ýkinci Description alaný
        public bool IsCompleted { get; set; }
        public int TaskId { get; set; }
        public Task Task { get; set; }
        public DateTime DueDate { get; set; } // DateTime alaný
    }
}
