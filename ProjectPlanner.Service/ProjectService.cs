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
                ProjectTypeId = type?.Id ?? 5
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

        public SubTask AddTaskToProject(Project project, string name, string? description = null)
        {
            var dbProject = _uow.Project.GetFirstOrDefault(p => p.Id == project.Id);
            if (dbProject == null)
            {
                throw new InvalidOperationException($"Project with ID {project.Id} not found.");
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

            return task;
        }

        public void DeleteTask(SubTask task)
        {
            var dbTask = _uow.Task.GetFirstOrDefault(t => t.Id == task.Id);
            if (dbTask == null) return;

            // Remove related attachments, links, notes
            var attachments = _uow.TaskAttachment.GetAll(a => a.SubTaskId == task.Id)?.ToList();
            if (attachments != null && attachments.Any())
                _uow.TaskAttachment.RemoveList(attachments);

            var links = _uow.TaskLink.GetAll(l => l.SubTaskId == task.Id)?.ToList();
            if (links != null && links.Any())
                _uow.TaskLink.RemoveList(links);

            var notes = _uow.TaskNote.GetAll(n => n.SubTaskId == task.Id)?.ToList();
            if (notes != null && notes.Any())
                _uow.TaskNote.RemoveList(notes);

            _uow.Task.Remove(dbTask);
            _uow.Save();

            WeakReferenceMessenger.Default.Send(new ProjectsUpdatedMessage());
        }

        public Project ConvertSubTaskToProject(SubTask task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            var dbTask = _uow.Task.GetFirstOrDefault(t => t.Id == task.Id);
            if (dbTask == null)
                throw new InvalidOperationException($"Task with ID {task.Id} not found.");

            if (!dbTask.ProjectId.HasValue)
                throw new InvalidOperationException("Task is not assigned to any project.");

            var parentProject = _uow.Project.GetFirstOrDefault(p => p.Id == dbTask.ProjectId.Value);
            if (parentProject == null)
                throw new InvalidOperationException($"Project with ID {dbTask.ProjectId.Value} not found.");

            var newProject = new Project
            {
                Name = dbTask.Name,
                Description = dbTask.Description,
                ProjectTypeId = parentProject.ProjectTypeId ?? parentProject.Type?.Id
            };

            if (newProject.ProjectTypeId == null)
            {
                newProject.ProjectTypeId = 5;
            }

            _uow.Project.Add(newProject);
            _uow.Save();

            DeleteTask(dbTask);

            return newProject;
        }

        public void DeleteProject(Project project)
        {
            var dbProject = _uow.Project.GetFirstOrDefault(p => p.Id == project.Id);
            if (dbProject == null) return;

            var tasks = _uow.Task.GetAll()?.Where(t => t.ProjectId == dbProject.Id).ToList();
            if (tasks is not null && tasks.Any())
            {
                foreach (var task in tasks)
                {
                    var attachments = _uow.TaskAttachment.GetAll(a => a.SubTaskId == task.Id)?.ToList();
                    if (attachments != null && attachments.Any())
                        _uow.TaskAttachment.RemoveList(attachments);

                    var links = _uow.TaskLink.GetAll(l => l.SubTaskId == task.Id)?.ToList();
                    if (links != null && links.Any())
                        _uow.TaskLink.RemoveList(links);

                    var notes = _uow.TaskNote.GetAll(n => n.SubTaskId == task.Id)?.ToList();
                    if (notes != null && notes.Any())
                        _uow.TaskNote.RemoveList(notes);
                }

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

        public SubTask? GetTaskById(int taskId)
        {
            return _uow.Task.GetById(taskId);
        }

        public SubTask? GetTaskWithDetails(int taskId)
        {
            var task = _uow.Task.GetById(taskId);
            if (task == null) return null;

            task.Attachments = GetAttachmentsForTask(taskId);
            task.Links = GetLinksForTask(taskId);
            task.Notes = GetNotesForTask(taskId);

            return task;
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
            _uow.TaskAttachment.RemoveAll();
            _uow.TaskLink.RemoveAll();
            _uow.TaskNote.RemoveAll();
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

        // Attachments
        public void AddAttachment(TaskAttachment attachment)
        {
            if (attachment == null)
                throw new ArgumentNullException(nameof(attachment));

            _uow.TaskAttachment.Add(attachment);
            _uow.Save();
        }

        public void RemoveAttachment(TaskAttachment attachment)
        {
            if (attachment == null) return;

            var dbAttachment = _uow.TaskAttachment.GetById(attachment.Id);
            if (dbAttachment != null)
            {
                _uow.TaskAttachment.Remove(dbAttachment);
                _uow.Save();
            }
        }

        public List<TaskAttachment> GetAttachmentsForTask(int taskId)
        {
            var attachments = _uow.TaskAttachment.GetAll(a => a.SubTaskId == taskId)?.ToList();
            return attachments ?? new List<TaskAttachment>();
        }

        // Links
        public void AddLink(TaskLink link)
        {
            if (link == null)
                throw new ArgumentNullException(nameof(link));

            _uow.TaskLink.Add(link);
            _uow.Save();
        }

        public void RemoveLink(TaskLink link)
        {
            if (link == null) return;

            var dbLink = _uow.TaskLink.GetById(link.Id);
            if (dbLink != null)
            {
                _uow.TaskLink.Remove(dbLink);
                _uow.Save();
            }
        }

        public List<TaskLink> GetLinksForTask(int taskId)
        {
            var links = _uow.TaskLink.GetAll(l => l.SubTaskId == taskId)?.ToList();
            return links ?? new List<TaskLink>();
        }

        // Notes
        public void AddNote(TaskNote note)
        {
            if (note == null)
                throw new ArgumentNullException(nameof(note));

            _uow.TaskNote.Add(note);
            _uow.Save();
        }

        public void RemoveNote(TaskNote note)
        {
            if (note == null) return;

            var dbNote = _uow.TaskNote.GetById(note.Id);
            if (dbNote != null)
            {
                _uow.TaskNote.Remove(dbNote);
                _uow.Save();
            }
        }

        public List<TaskNote> GetNotesForTask(int taskId)
        {
            var notes = _uow.TaskNote.GetAll(n => n.SubTaskId == taskId)?.ToList();
            return notes ?? new List<TaskNote>();
        }
    }
}