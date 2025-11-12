using ProjectPlanner.Model;

namespace ProjectPlanner.Service
{
    public interface IProjectService
    {
        void AddProject(string name);

        void DeleteProject(Project project);

        List<Project> GetAllProjects();
    }
}