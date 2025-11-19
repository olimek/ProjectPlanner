using System.Linq;
using ProjectPlanner.Data.Contexts;
using ProjectPlanner.Data.Repository.IRepository;
using ProjectPlanner.Model;

namespace ProjectPlanner.Data.Repository
{
    public class TaskRepository : Repository<SubTask>, ITaskRepository
    {
        private ProjectContext _db;

        public TaskRepository(ProjectContext db) : base(db)
        {
            _db = db;
        }

        public override void Update(SubTask entity)
        {
            var objFromDb = _db.Tasks.FirstOrDefault(x => x.Id == entity.Id);
            if (objFromDb != null)
            {
                objFromDb.Name = entity.Name;
                objFromDb.Description = entity.Description;
            }
        }
    }
}