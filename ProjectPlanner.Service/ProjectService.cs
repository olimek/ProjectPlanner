using ProjectPlanner.Data;
using ProjectPlanner.Data.Contexts;
using ProjectPlanner.Data.UnitOfWork;
using ProjectPlanner.Model;
using CommunityToolkit.Mvvm.Messaging;
using ProjectPlanner.Model.Messaging;

namespace ProjectPlanner.Service
{
    public class ProjectService : IProjectService
    {
        private readonly IUnitOfWork _uow;

        public ProjectService(IUnitOfWork unitOfWork)
        {
            _uow = unitOfWork;
        }

        public void AddProject(string name)
        {
            AddProject(name, null, null);
        }

        public Project AddProject(string name, string? description, ProjectType? type)
        {
            var project = new Project
            {
                Name = name ?? string.Empty,
                Description = description,
                ProjectTypeId = type?.Id ?? 5 // Default to "Other" (Id = 5)
            };

            _uow.Project.Add(project);
            _uow.Save();

            WeakReferenceMessenger.Default.Send(new ProjectsUpdatedMessage());

            return project;
        }

        public void AddTaskToProject(int projectId, string taskName, string? description = null)
        {
            var project = _uow.Project.GetById(projectId);

            if (project == null)
                throw new InvalidOperationException($"Project with ID {projectId} not found.");

            var subTask = new SubTask
            {
                Name = taskName,
                Description = description ?? string.Empty,
                IsDone = false,
                ProjectId = projectId
            };
            _uow.Task.Add(subTask);
            _uow.Save();

            WeakReferenceMessenger.Default.Send(new ProjectsUpdatedMessage());
        }

        public void AddTaskToProject(Project project, string name, string? description = null)
        {
            var dbProject = _uow.Project.GetFirstOrDefault(p => p.Id == project.Id);
            if (dbProject == null)
            {
                return;
            }

            var task = new SubTask
            {
                Name = name,
                Description = description ?? string.Empty,
                ProjectId = dbProject.Id,
                IsDone = false
            };

            _uow.Task.Add(task);
            _uow.Save();

            WeakReferenceMessenger.Default.Send(new ProjectsUpdatedMessage());
        }

        public void DeleteTask(SubTask task)
        {
            var dbTask = _uow.Task.GetFirstOrDefault(t => t.Id == task.Id);
            if (dbTask == null) return;

            _uow.Task.Remove(dbTask);
            _uow.Save();

            WeakReferenceMessenger.Default.Send(new ProjectsUpdatedMessage());
        }

        public void DeleteProject(Project project)
        {
            var dbProject = _uow.Project.GetFirstOrDefault(p => p.Id == project.Id);
            if (dbProject == null) return;

            var tasks = _uow.Task.GetAll()?.Where(t => t.ProjectId == dbProject.Id).ToList();
            if (tasks is not null && tasks.Any())
            {
                _uow.Task.RemoveList(tasks);
            }

            _uow.Project.Remove(dbProject);
            _uow.Save();

            WeakReferenceMessenger.Default.Send(new ProjectsUpdatedMessage());
        }

        public List<SubTask> GetTasksForProject(int projectId)
        {
            var tasks = _uow.Task.GetAll()?.Where(t => t.ProjectId == projectId).ToList();
            return tasks ?? new List<SubTask>();
        }

        public Project GetProjectByID(int projectId)
        {
            var project = _uow.Project.GetById(projectId);

            if (project == null)
                throw new InvalidOperationException($"Project with ID {projectId} not found.");

            return project;
        }

        public void DeleteAllProjects()
        {
            _uow.Task.RemoveAll();
            _uow.Project.RemoveAll();
            _uow.Save();

            WeakReferenceMessenger.Default.Send(new ProjectsUpdatedMessage());
        }

        public List<Project> GetAllProjects()
        {
            var projects = _uow.Project.GetAll() ?? Enumerable.Empty<Project>();
            var list = projects.ToList();

            foreach (var proj in list)
            {
                proj.Tasks = GetTasksForProject(proj.Id);
            }

            return list;
        }

        public void UpdateProject(Project project)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));

            var dbProject = _uow.Project.GetFirstOrDefault(p => p.Id == project.Id);
            if (dbProject == null)
                throw new InvalidOperationException($"Project with ID {project.Id} not found.");

            dbProject.Name = project.Name;
            dbProject.Description = project.Description;
            dbProject.ProjectTypeId = project.ProjectTypeId;

            _uow.Project.Update(dbProject);
            _uow.Save();
        }

        public void UpdateProject(int projectId, string name, string? description = null, string? projectType = null)
        {
            var dbProject = _uow.Project.GetById(projectId);
            if (dbProject == null)
                throw new InvalidOperationException($"Project with ID {projectId} not found.");

            dbProject.Name = name;

            if (description != null)
                dbProject.Description = description;

            if (projectType != null)
            {
                var type = _uow.ProjectType.GetByName(projectType);
                if (type != null)
                    dbProject.ProjectTypeId = type.Id;
            }

            _uow.Project.Update(dbProject);
            _uow.Save();
        }

        public void UpdateTask(SubTask task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            var dbTask = _uow.Task.GetFirstOrDefault(t => t.Id == task.Id);
            if (dbTask == null)
                throw new InvalidOperationException($"Task with ID {task.Id} not found.");

            dbTask.Name = task.Name;
            dbTask.Description = task.Description;
            dbTask.ProjectId = task.ProjectId;
            dbTask.IsDone = task.IsDone;

            _uow.Task.Update(dbTask);
            _uow.Save();

            WeakReferenceMessenger.Default.Send(new ProjectsUpdatedMessage());
        }

        public void UpdateTask(int taskId, string name, string? description = null, int? projectId = null, bool? isDone = null)
        {
            var dbTask = _uow.Task.GetById(taskId);
            if (dbTask == null)
                throw new InvalidOperationException($"Task with ID {taskId} not found.");

            dbTask.Name = name;

            if (description != null)
                dbTask.Description = description;

            if (projectId != null)
                dbTask.ProjectId = projectId.Value;

            if (isDone.HasValue)
                dbTask.IsDone = isDone.Value;

            _uow.Task.Update(dbTask);
            _uow.Save();

            WeakReferenceMessenger.Default.Send(new ProjectsUpdatedMessage());
        }
    }
}