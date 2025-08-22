using SQLite;

namespace ProjectPlanner.Database
{
    public enum ProjectType
    {
        Electronics,
        Programming,
        Mechanics,
        Home,
        Other
    }

    [Table("Project")]
    public class Project
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }
        public string? Description { get; set; }
        public ProjectType Type { get; set; }
        public List<Task> tasks { get; set; }
    }

    [Table("Task")]
    public class Task
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
    }
}