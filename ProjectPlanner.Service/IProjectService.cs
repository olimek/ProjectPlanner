using ProjectPlanner.Model;
using System.Collections.Generic;

namespace ProjectPlanner.Service
{
    public interface IProjectService
    {
        void AddProject(string name);

        void DeleteProject(Project project);

        void DeleteAllProjects();

        List<Project> GetAllProjects();

        void AddTaskToProject(Project project, string name, string? description = null);

        void DeleteTask(SubTask task);
    }
}