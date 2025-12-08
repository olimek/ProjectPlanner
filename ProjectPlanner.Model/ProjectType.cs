using System.ComponentModel.DataAnnotations;

namespace ProjectPlanner.Model
{
    public class ProjectType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsCustom { get; set; }

        public static class Predefined
        {
            public const string Electronics = "Electronics";
            public const string Programming = "Programming";
            public const string Mechanics = "Mechanics";
            public const string Home = "Home";
            public const string Other = "Other";
        }

        public override string ToString() => Name;

        public override bool Equals(object? obj) =>
            obj is ProjectType other && Id == other.Id;

        public override int GetHashCode() => Id.GetHashCode();
    }
}