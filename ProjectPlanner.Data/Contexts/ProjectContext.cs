using Microsoft.EntityFrameworkCore;
using ProjectPlanner.Model;

namespace ProjectPlanner.Data.Contexts
{
    public class ProjectContext : DbContext
    {
        public DbSet<Project> Projects { get; set; } = null!;
        public DbSet<SubTask> Tasks { get; set; } = null!;
        public DbSet<ProjectType> ProjectTypes { get; set; } = null!;

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed predefiniowanych typów projektów
            modelBuilder.Entity<ProjectType>().HasData(
                new ProjectType { Id = 1, Name = ProjectType.Predefined.Electronics, IsCustom = false },
                new ProjectType { Id = 2, Name = ProjectType.Predefined.Programming, IsCustom = false },
                new ProjectType { Id = 3, Name = ProjectType.Predefined.Mechanics, IsCustom = false },
                new ProjectType { Id = 4, Name = ProjectType.Predefined.Home, IsCustom = false },
                new ProjectType { Id = 5, Name = ProjectType.Predefined.Other, IsCustom = false }
            );

            // Konfiguracja relacji
            modelBuilder.Entity<Project>()
                .HasOne(p => p.Type)
                .WithMany()
                .HasForeignKey(p => p.ProjectTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}