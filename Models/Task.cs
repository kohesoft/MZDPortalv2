using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MZDNETWORK.Models
{
    public class Task
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Username { get; set; } // UserId yerine Username kullanýlýyor
        public User User { get; set; }
        public ICollection<TodoItem> TodoItems { get; set; }
        public int Progress { get; set; }
    }
}