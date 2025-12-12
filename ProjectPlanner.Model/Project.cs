using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectPlanner.Model
{
    public class Project
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        
        public int? ProjectTypeId { get; set; }
        
        [ForeignKey(nameof(ProjectTypeId))]
        public ProjectType? Type { get; set; }
        
        public List<SubTask> Tasks { get; set; } = new();

        [NotMapped]
        public double Progress
        {
            get
            {
                if (Tasks == null || !Tasks.Any()) return 0.0;
                var total = Tasks.Count;
                var done = Tasks.Count(t => t.IsDone);
                return total == 0 ? 0.0 : (double)done / total;
            }
        }
    }
}