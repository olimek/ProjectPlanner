using System.ComponentModel.DataAnnotations;

namespace ProjectPlanner.Model
{
    public class Project
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
        public string? Description { get; set; }
        public ProjectType Type { get; set; }
        public List<SubTask>? tasks { get; set; }
    }
}