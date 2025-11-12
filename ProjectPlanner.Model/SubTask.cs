using System.ComponentModel.DataAnnotations;

namespace ProjectPlanner.Model
{
    public class SubTask
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
        public Project? Project { get; set; }
    }
}