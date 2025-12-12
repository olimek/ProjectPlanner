using ProjectPlanner.Data.Contexts;
using ProjectPlanner.Data.Repository;
using ProjectPlanner.Data.Repository.IRepository;

namespace ProjectPlanner.Data.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ProjectContext _db;

        public UnitOfWork(ProjectContext db)
        {
            _db = db;
            Project = new ProjectRepository(_db);
            Task = new TaskRepository(_db);
            ProjectType = new ProjectTypeRepository(_db);
            TaskAttachment = new TaskAttachmentRepository(_db);
            TaskLink = new TaskLinkRepository(_db);
            TaskNote = new TaskNoteRepository(_db);
        }

        public IProjectRepository Project { get; private set; }
        public ITaskRepository Task { get; private set; }
        public IProjectTypeRepository ProjectType { get; private set; }
        public ITaskAttachmentRepository TaskAttachment { get; private set; }
        public ITaskLinkRepository TaskLink { get; private set; }
        public ITaskNoteRepository TaskNote { get; private set; }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}