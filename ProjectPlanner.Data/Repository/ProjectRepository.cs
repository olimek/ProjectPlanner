using System.Linq;
using Microsoft.EntityFrameworkCore;
using ProjectPlanner.Data.Contexts;
using ProjectPlanner.Data.Repository.IRepository;
using ProjectPlanner.Model;

namespace ProjectPlanner.Data.Repository
{
    public class ProjectRepository : Repository<Project>, IProjectRepository
    {
        private ProjectContext _db;

        public ProjectRepository(ProjectContext db) : base(db)
        {
            _db = db;
        }

        public override void Update(Project entity)
        {
            var objFromDb = _db.Projects.FirstOrDefault(x => x.Id == entity.Id);
            if (objFromDb != null)
            {
                objFromDb.Name = entity.Name;
                objFromDb.Description = entity.Description;
                objFromDb.Type = entity.Type;
                objFromDb.Tasks = entity.Tasks;
            }
        }

        // Ensure tasks are loaded with projects when fetching all projects
        public override IEnumerable<Project>? GetAll()
        {
            return _db.Projects.Include(p => p.Tasks).ToList();
        }
    }
}