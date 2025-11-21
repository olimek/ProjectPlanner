using ProjectPlanner.Data.UnitOfWork;
using ProjectPlanner.Model;
using System.Collections.Generic;
using System.Linq;

namespace ProjectPlanner.Service
{
    public class ProjectService : IProjectService
    {
        private readonly IUnitOfWork _uow;

        public ProjectService(IUnitOfWork unitOfWork)
        {
            _uow = unitOfWork;
        }

        public void AddTaskToProject(int projectId, string taskName, string? description = null)
        {
            var project = _uow.Project.GetById(projectId);

            if (project == null)
                throw new InvalidOperationException($"Nie znaleziono projektu o ID {projectId}.");

            var subTask = new SubTask
            {
                Name = taskName,
                Description = description
            };

            project.tasks ??= new List<SubTask>();
            project.tasks.Add(subTask);

            _uow.Project.Update(project);
            _uow.Save();
        }

        public void AddProject(string name)
        {
            var project = new Project
            {
                Name = name,
            };

            _uow.Project.Add(project);
            _uow.Save();
        }

        public void DeleteProject(Project project)
        {
            // Załaduj projekt z taskami
            var loadedProject = _uow.Project.GetById(project.Id);

            if (loadedProject == null)
                throw new InvalidOperationException($"Nie znaleziono projektu o ID {project.Id}.");

            // Usuń projekt
            _uow.Project.Remove(loadedProject);

            _uow.Save();
        }

        public List<SubTask> GetTasksForProject(int projectId)
        {
            var project = _uow.Project.GetById(projectId);

            if (project == null)
                throw new InvalidOperationException($"Nie znaleziono projektu o ID {projectId}.");

            // jeśli tasks jest null, zwróć pustą listę
            return project.tasks?.ToList() ?? new List<SubTask>();
        }

        public void DeleteAllProjects()
        {
            _uow.Project.RemoveAll();
            _uow.Save();
        }

        public List<Project> GetAllProjects()
        {
            var projects = _uow.Project.GetAll() ?? Enumerable.Empty<Project>();
            return projects.ToList();
        }
    }
}