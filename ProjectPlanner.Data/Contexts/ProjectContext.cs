using Microsoft.EntityFrameworkCore;
using ProjectPlanner.Model;

namespace ProjectPlanner.Data.Contexts
{
    public class ProjectContext : DbContext
    {
        public DbSet<Project> Projects { get; set; } = null!;
        public DbSet<SubTask> Tasks { get; set; } = null!;
        public DbSet<ProjectType> ProjectTypes { get; set; } = null!;
        public DbSet<TaskAttachment> TaskAttachments { get; set; } = null!;
        public DbSet<TaskLink> TaskLinks { get; set; } = null!;
        public DbSet<TaskNote> TaskNotes { get; set; } = null!;

        public string DbPath { get; }

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

            modelBuilder.Entity<ProjectType>().HasData(

                new ProjectType { Id = 1, Name = ProjectType.Predefined.Home, IsCustom = false }
                            );

            modelBuilder.Entity<Project>()
                .HasOne(p => p.Type)
                .WithMany()
                .HasForeignKey(p => p.ProjectTypeId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<SubTask>()
                .HasMany(t => t.Attachments)
                .WithOne(a => a.SubTask)
                .HasForeignKey(a => a.SubTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SubTask>()
                .HasMany(t => t.Links)
                .WithOne(l => l.SubTask)
                .HasForeignKey(l => l.SubTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SubTask>()
                .HasMany(t => t.Notes)
                .WithOne(n => n.SubTask)
                .HasForeignKey(n => n.SubTaskId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}