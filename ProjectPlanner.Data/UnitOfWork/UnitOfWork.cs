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
        }

        public IProjectRepository Project { get; private set; }
        public ITaskRepository Task { get; private set; }
        public IProjectTypeRepository ProjectType { get; private set; }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}