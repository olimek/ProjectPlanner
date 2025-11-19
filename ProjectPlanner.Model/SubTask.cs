using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectPlanner.Model
{
    public class SubTask
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        // FK & navigation
        public int? ProjectId { get; set; }

        public Project? Project { get; set; }
    }
}