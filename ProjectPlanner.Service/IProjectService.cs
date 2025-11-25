using ProjectPlanner.Model;
using System.Collections.Generic;

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

        void AddTaskToProject(Project project, string name, string? description = null);

        void DeleteTask(SubTask task);

        void UpdateProject(int projectId, string name, string? description = null, string? projectType = null);

        void UpdateProject(Project project);

        public Project GetProjectByID(int projectId);
    }
}