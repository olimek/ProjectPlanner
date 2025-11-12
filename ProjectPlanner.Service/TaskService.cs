using ProjectPlanner.Data.UnitOfWork;
using ProjectPlanner.Model;
using System.Collections.Generic;
using System.Linq;

namespace ProjectPlanner.Service
{
    public class TaskService : ITaskService
    {
        private readonly IUnitOfWork _uow;

        public TaskService(IUnitOfWork unitOfWork)
        {
            _uow = unitOfWork;
        }

        // ➕ Dodaj nowe zadanie do projektu (bezpośrednio do kolekcji tasks)
        public void AddTaskToProject(int projectId, string taskName, string description)
        {
            var project = _uow.Project.GetById(projectId);
            if (project == null)
                throw new InvalidOperationException($"Nie znaleziono projektu o ID {projectId}.");

            // utwórz zadanie
            var subTask = new SubTask
            {
                Name = taskName,
                Description = description,
            };

            // dodaj bezpośrednio do kolekcji
            project.tasks ??= new List<SubTask>();
            project.tasks.Add(subTask);

            // aktualizuj projekt (EF sam doda SubTasky)
            _uow.Project.Update(project);
            _uow.Save();
        }

        // ❌ Usuń jedno zadanie z projektu
        public void DeleteTask(int projectId, int taskId)
        {
            var project = _uow.Project.GetById(projectId);
            if (project?.tasks == null) return;

            var task = project.tasks.FirstOrDefault(t => t.Id == taskId);
            if (task == null) return;

            project.tasks.Remove(task);

            _uow.Project.Update(project);
            _uow.Save();
        }

        // ❌ Usuń wszystkie zadania projektu
        public void DeleteAllTasksInProject(int projectId)
        {
            var project = _uow.Project.GetById(projectId);
            if (project?.tasks == null) return;

            project.tasks.Clear();

            _uow.Project.Update(project);
            _uow.Save();
        }

        // 📋 Pobierz wszystkie zadania projektu
        public List<SubTask> GetTasksForProject(int projectId)
        {
            var project = _uow.Project.GetById(projectId);
            return project?.tasks?.ToList() ?? new List<SubTask>();
        }
    }
}