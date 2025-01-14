using System.ComponentModel.DataAnnotations;

namespace MZDNETWORK.Models
{
    public class TodoItem
    {
        [Key]
        public int Id { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
        public int TaskId { get; set; }
        public Task Task { get; set; }
    }
}
