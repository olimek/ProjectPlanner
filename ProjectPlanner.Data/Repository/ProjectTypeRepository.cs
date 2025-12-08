using ProjectPlanner.Data.Contexts;
using ProjectPlanner.Data.Repository.IRepository;
using ProjectPlanner.Model;

namespace ProjectPlanner.Data.Repository
{
    public class ProjectTypeRepository : Repository<ProjectType>, IProjectTypeRepository
    {
        private readonly ProjectContext _context;

        public ProjectTypeRepository(ProjectContext context) : base(context)
        {
            _context = context;
        }

        public ProjectType? GetByName(string name)
        {
            return _context.ProjectTypes.FirstOrDefault(pt => pt.Name == name);
        }

        public IEnumerable<ProjectType> GetCustomTypes()
        {
            return _context.ProjectTypes.Where(pt => pt.IsCustom).ToList();
        }

        public IEnumerable<ProjectType> GetPredefinedTypes()
        {
            return _context.ProjectTypes.Where(pt => !pt.IsCustom).ToList();
        }

        public override void Update(ProjectType entity)
        {
            var objFromDb = _context.ProjectTypes.FirstOrDefault(x => x.Id == entity.Id);
            if (objFromDb != null)
            {
                objFromDb.Name = entity.Name;
                objFromDb.Description = entity.Description;
                objFromDb.IsCustom = entity.IsCustom;
            }
        }
    }
}
