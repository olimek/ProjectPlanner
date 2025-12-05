using Microsoft.EntityFrameworkCore;
using ProjectPlanner.Model;

namespace ProjectPlanner.Data.Contexts
{
    public class ProjectContext : DbContext
    {
        public DbSet<Project> Projects { get; set; } = null!;
        public DbSet<SubTask> Tasks { get; set; } = null!;

        public string DbPath { get; }

        // DI constructor
        public ProjectContext(DbContextOptions<ProjectContext> options) : base(options)
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            DbPath = System.IO.Path.Join(path, "projectsplanner.db3");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite($"Data Source={DbPath}");
            }
        }
    }
}