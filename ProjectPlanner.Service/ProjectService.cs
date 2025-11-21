using System;
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

        // Overload group: keep both overloads adjacent
        public void AddTaskToProject(int projectId, string taskName, string? description = null)
        {
            var project = _uow.Project.GetById(projectId);

            if (project == null)
                throw new InvalidOperationException($"Nie znaleziono projektu o ID {projectId}.");

            var subTask = new SubTask
            {
                Name = taskName,
                Description = description ?? string.Empty
            };

            project.Tasks ??= new List<SubTask>();
            project.Tasks.Add(subTask);

            _uow.Project.Update(project);
            _uow.Save();
        }

        public void AddTaskToProject(Project project, string name, string? description = null)
        {
            // Ensure we operate on the tracked DB project to avoid EF treating the passed Project as a new entity
            var dbProject = _uow.Project.GetFirstOrDefault(p => p.Id == project.Id);
            if (dbProject == null)
            {
                // Project not found in DB - nothing to attach the task to
                return;
            }

            var task = new SubTask
            {
                Name = name,
                Description = description ?? string.Empty,
                ProjectId = dbProject.Id
            };

            _uow.Task.Add(task);
            _uow.Save();

            // Ensure caller's in-memory Project.Tasks is updated without duplications
            project.Tasks ??= new List<SubTask>();

            if (!project.Tasks.Any(t => t.Id == task.Id))
            {
                project.Tasks.Add(task);
            }
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

        public void DeleteTask(SubTask task)
        {
            // Use tracked instance from DB to remove reliably
            var dbTask = _uow.Task.GetFirstOrDefault(t => t.Id == task.Id);
            if (dbTask == null) return;

            _uow.Task.Remove(dbTask);
            _uow.Save();
        }

        public void DeleteProject(Project project)
        {
            // Use tracked DB project to avoid attach/duplicate issues
            var dbProject = _uow.Project.GetFirstOrDefault(p => p.Id == project.Id);
            if (dbProject == null) return;

            // Delete subtasks by ProjectId (safer than relying on navigation collection)
            var tasks = _uow.Task.GetAll()?.Where(t => t.ProjectId == dbProject.Id).ToList();
            if (tasks is not null && tasks.Any())
            {
                _uow.Task.RemoveList(tasks);
            }

            _uow.Project.Remove(dbProject);
            _uow.Save();
        }

        public List<SubTask> GetTasksForProject(int projectId)
        {
            var project = _uow.Project.GetById(projectId);

            if (project == null)
                throw new InvalidOperationException($"Nie znaleziono projektu o ID {projectId}.");

            // jeśli tasks jest null, zwróć pustą listę
            return project.Tasks?.ToList() ?? new List<SubTask>();
        }

        public void DeleteAllProjects()
        {
            // Remove all tasks first to avoid orphaned records
            _uow.Task.RemoveAll();
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