using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ProjectPlanner.Model
{
    public class Project
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ProjectType Type { get; set; }
        public List<SubTask> Tasks { get; set; } = new();
    }
}