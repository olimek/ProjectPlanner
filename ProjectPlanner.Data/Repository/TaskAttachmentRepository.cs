using ProjectPlanner.Data.Contexts;
using ProjectPlanner.Data.Repository.IRepository;
using ProjectPlanner.Model;

namespace ProjectPlanner.Data.Repository
{
    public class TaskAttachmentRepository : Repository<TaskAttachment>, ITaskAttachmentRepository
    {
        private readonly ProjectContext _db;

        public TaskAttachmentRepository(ProjectContext db) : base(db)
        {
            _db = db;
        }

        public override void Update(TaskAttachment entity)
        {
            var objFromDb = _db.TaskAttachments.FirstOrDefault(x => x.Id == entity.Id);
            if (objFromDb != null)
            {
                objFromDb.FileName = entity.FileName;
                objFromDb.FilePath = entity.FilePath;
                objFromDb.FileType = entity.FileType;
                objFromDb.FileSize = entity.FileSize;
                objFromDb.ThumbnailPath = entity.ThumbnailPath;
            }
        }
    }
}
