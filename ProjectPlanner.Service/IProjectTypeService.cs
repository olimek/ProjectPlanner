using ProjectPlanner.Model;

namespace ProjectPlanner.Service
{
    public interface IProjectTypeService
    {
        List<ProjectType> GetAllProjectTypes();
        List<ProjectType> GetCustomProjectTypes();
        List<ProjectType> GetPredefinedProjectTypes();
        ProjectType? GetProjectTypeById(int id);
        ProjectType? GetProjectTypeByName(string name);
        ProjectType AddCustomProjectType(string name, string? description = null);
        void UpdateProjectType(int id, string name, string? description = null);
        void DeleteCustomProjectType(int id);
    }
}
