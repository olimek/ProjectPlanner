using ProjectPlanner.Data.Contexts;
using ProjectPlanner.Data.Repository;
using ProjectPlanner.Data.Repository.IRepository;

namespace ProjectPlanner.Data.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private ProjectContext _db;

        public UnitOfWork(ProjectContext db)
        {
            _db = db;
            Project = new ProjectRepository(_db);
        }

        public IProjectRepository Project { get; private set; }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}