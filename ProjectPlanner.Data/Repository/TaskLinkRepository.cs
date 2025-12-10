using ProjectPlanner.Data.Contexts;
using ProjectPlanner.Data.Repository.IRepository;
using ProjectPlanner.Model;

namespace ProjectPlanner.Data.Repository
{
    public class TaskLinkRepository : Repository<TaskLink>, ITaskLinkRepository
    {
        private readonly ProjectContext _db;

        public TaskLinkRepository(ProjectContext db) : base(db)
        {
            _db = db;
        }

        public override void Update(TaskLink entity)
        {
            var objFromDb = _db.TaskLinks.FirstOrDefault(x => x.Id == entity.Id);
            if (objFromDb != null)
            {
                objFromDb.Url = entity.Url;
                objFromDb.Title = entity.Title;
                objFromDb.Description = entity.Description;
            }
        }
    }
}
