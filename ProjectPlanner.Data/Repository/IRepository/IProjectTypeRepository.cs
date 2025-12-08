using ProjectPlanner.Model;

namespace ProjectPlanner.Data.Repository.IRepository
{
    public interface IProjectTypeRepository : IRepository<ProjectType>
    {
        ProjectType? GetByName(string name);
        IEnumerable<ProjectType> GetCustomTypes();
        IEnumerable<ProjectType> GetPredefinedTypes();
    }
}
