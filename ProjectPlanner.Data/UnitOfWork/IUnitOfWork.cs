using ProjectPlanner.Data.Repository.IRepository;

namespace ProjectPlanner.Data.UnitOfWork
{
    public interface IUnitOfWork
    {
        IProjectRepository Project { get; }

        void Save();
    }
}