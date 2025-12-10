using ProjectPlanner.Data.Contexts;
using ProjectPlanner.Data.Repository.IRepository;
using ProjectPlanner.Model;

namespace ProjectPlanner.Data.Repository
{
    public class TaskNoteRepository : Repository<TaskNote>, ITaskNoteRepository
    {
        private readonly ProjectContext _db;

        public TaskNoteRepository(ProjectContext db) : base(db)
        {
            _db = db;
        }

        public override void Update(TaskNote entity)
        {
            var objFromDb = _db.TaskNotes.FirstOrDefault(x => x.Id == entity.Id);
            if (objFromDb != null)
            {
                objFromDb.Content = entity.Content;
                objFromDb.ModifiedAt = DateTime.Now;
            }
        }
    }
}
