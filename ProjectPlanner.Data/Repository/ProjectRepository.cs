using Microsoft.EntityFrameworkCore;
using ProjectPlanner.Data.Contexts;
using ProjectPlanner.Data.Repository.IRepository;
using ProjectPlanner.Model;

namespace ProjectPlanner.Data.Repository
{
    public class ProjectRepository : Repository<Project>, IProjectRepository
    {
        private readonly ProjectContext _db;

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
                objFromDb.ProjectTypeId = entity.ProjectTypeId;
                objFromDb.Tasks = entity.Tasks;
            }
        }

        public override IEnumerable<Project>? GetAll()
        {
            return _db.Projects
                .Include(p => p.Tasks)
                .Include(p => p.Type)
                .ToList();
        }
    }
}