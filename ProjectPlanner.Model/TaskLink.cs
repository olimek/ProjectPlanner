using System.ComponentModel.DataAnnotations;

namespace ProjectPlanner.Model
{
    public class TaskLink
    {
        [Key]
        public int Id { get; set; }

        public string Url { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.Now;

        public int SubTaskId { get; set; }

        public SubTask? SubTask { get; set; }
    }
}