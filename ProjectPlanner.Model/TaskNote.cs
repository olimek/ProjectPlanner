using System.ComponentModel.DataAnnotations;

namespace ProjectPlanner.Model
{
    public class TaskNote
    {
        [Key]
        public int Id { get; set; }

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? ModifiedAt { get; set; }

        public int SubTaskId { get; set; }

        public SubTask? SubTask { get; set; }
    }
}