using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ProjectPlanner.Data.Contexts;

namespace ProjectPlanner.Data
{
    public class ProjectContextFactory : IDesignTimeDbContextFactory<ProjectContext>
    {
        public ProjectContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ProjectContext>();
            optionsBuilder.UseSqlite("Data Source=projectsplanner.db3");

            return new ProjectContext(optionsBuilder.Options);
        }
    }
}
