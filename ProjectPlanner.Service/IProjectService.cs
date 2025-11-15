using ProjectPlanner.Model;

namespace ProjectPlanner.Service
{
    public interface IProjectService
    {
        void AddProject(string name);

        void DeleteProject(Project project);

        void AddTaskToProject(int projectId, string taskName, string? description = null);

        List<SubTask> GetTasksForProject(int projectId);

        void DeleteAllProjects();

        List<Project> GetAllProjects();
    }
}