using ProjectPlanner.Model;

namespace ProjectPlanner.Service
{
    public interface IProjectService
    {
        void AddProject(string name);

        void DeleteProject(Project project);

        void DeleteAllProjects();

        List<Project> GetAllProjects();
    }
}