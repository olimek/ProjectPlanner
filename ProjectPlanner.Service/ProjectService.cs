using ProjectPlanner.Data.UnitOfWork;
using ProjectPlanner.Model;

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
                Type = type ?? default
            };

            _uow.Project.Add(project);
            _uow.Save();

            return project;
        }

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
            var dbProject = _uow.Project.GetFirstOrDefault(p => p.Id == project.Id);
            if (dbProject == null)
            {
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

            project.Tasks ??= new List<SubTask>();

            if (!project.Tasks.Any(t => t.Id == task.Id))
            {
                project.Tasks.Add(task);
            }
        }

        public void DeleteTask(SubTask task)
        {
            var dbTask = _uow.Task.GetFirstOrDefault(t => t.Id == task.Id);
            if (dbTask == null) return;

            _uow.Task.Remove(dbTask);
            _uow.Save();
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
        }

        public List<SubTask> GetTasksForProject(int projectId)
        {
            var project = _uow.Project.GetById(projectId);

            if (project == null)
                throw new InvalidOperationException($"Nie znaleziono projektu o ID {projectId}.");

            return project.Tasks?.ToList() ?? new List<SubTask>();
        }

        public Project GetProjectByID(int projectId)
        {
            var project = _uow.Project.GetById(projectId);

            if (project == null)
                throw new InvalidOperationException($"Nie znaleziono projektu o ID {projectId}.");

            return project;
        }

        public void DeleteAllProjects()
        {
            _uow.Task.RemoveAll();
            _uow.Project.RemoveAll();
            _uow.Save();
        }

        public List<Project> GetAllProjects()
        {
            var projects = _uow.Project.GetAll() ?? Enumerable.Empty<Project>();
            return projects.ToList();
        }

        public void UpdateProject(Project project)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));

            var dbProject = _uow.Project.GetFirstOrDefault(p => p.Id == project.Id);
            if (dbProject == null)
                throw new InvalidOperationException($"Nie znaleziono projektu o ID {project.Id}.");

            dbProject.Name = project.Name;

            var projectType = typeof(Project);
            var descProp = projectType.GetProperty("Description");
            if (descProp != null)
            {
                var incomingDesc = descProp.GetValue(project) as string;
                if (incomingDesc != null)
                {
                    descProp.SetValue(dbProject, incomingDesc);
                }
            }

            var typeProp = projectType.GetProperty("ProjectType");
            if (typeProp != null)
            {
                var incomingType = typeProp.GetValue(project) as string;
                if (incomingType != null)
                {
                    typeProp.SetValue(dbProject, incomingType);
                }
            }

            _uow.Project.Update(dbProject);
            _uow.Save();
        }

        public void UpdateProject(int projectId, string name, string? description = null, string? projectType = null)
        {
            var dbProject = _uow.Project.GetById(projectId);
            if (dbProject == null)
                throw new InvalidOperationException($"Nie znaleziono projektu o ID {projectId}.");

            dbProject.Name = name;

            var projectTypeRef = typeof(Project);
            var descProp = projectTypeRef.GetProperty("Description");
            if (descProp != null && description is not null)
            {
                descProp.SetValue(dbProject, description);
            }

            var typeProp = projectTypeRef.GetProperty("ProjectType");
            if (typeProp != null && projectType is not null)
            {
                typeProp.SetValue(dbProject, projectType);
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
                throw new InvalidOperationException($"Nie znaleziono zadania o ID {task.Id}.");

            dbTask.Name = task.Name;

            if (task.Description is not null)
            {
                dbTask.Description = task.Description;
            }

            if (task.ProjectId != default && task.ProjectId != dbTask.ProjectId)
            {
                dbTask.ProjectId = task.ProjectId;
            }

            _uow.Task.Update(dbTask);
            _uow.Save();
        }

        public void UpdateTask(int taskId, string name, string? description = null, int? projectId = null)
        {
            var dbTask = _uow.Task.GetById(taskId);
            if (dbTask == null)
                throw new InvalidOperationException($"Nie znaleziono zadania o ID {taskId}.");

            dbTask.Name = name;

            if (description is not null)
            {
                dbTask.Description = description;
            }

            if (projectId is not null)
            {
                dbTask.ProjectId = projectId.Value;
            }

            _uow.Task.Update(dbTask);
            _uow.Save();
        }
    }
}