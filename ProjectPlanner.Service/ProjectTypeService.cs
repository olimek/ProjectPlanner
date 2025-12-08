using CommunityToolkit.Mvvm.Messaging;
using ProjectPlanner.Data.UnitOfWork;
using ProjectPlanner.Model;
using ProjectPlanner.Model.Messaging;

namespace ProjectPlanner.Service
{
    public class ProjectTypeService : IProjectTypeService
    {
        private readonly IUnitOfWork _uow;

        public ProjectTypeService(IUnitOfWork unitOfWork)
        {
            _uow = unitOfWork;
        }

        public List<ProjectType> GetAllProjectTypes()
        {
            var types = _uow.ProjectType.GetAll();
            return types?.ToList() ?? new List<ProjectType>();
        }

        public List<ProjectType> GetCustomProjectTypes()
        {
            return _uow.ProjectType.GetCustomTypes().ToList();
        }

        public List<ProjectType> GetPredefinedProjectTypes()
        {
            return _uow.ProjectType.GetPredefinedTypes().ToList();
        }

        public ProjectType? GetProjectTypeById(int id)
        {
            return _uow.ProjectType.GetById(id);
        }

        public ProjectType? GetProjectTypeByName(string name)
        {
            return _uow.ProjectType.GetByName(name);
        }

        public ProjectType AddCustomProjectType(string name, string? description = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty", nameof(name));

            var existing = _uow.ProjectType.GetByName(name);
            if (existing != null)
                throw new InvalidOperationException($"Project type with name '{name}' already exists.");

            var projectType = new ProjectType
            {
                Name = name,
                Description = description,
                IsCustom = true
            };

            _uow.ProjectType.Add(projectType);
            _uow.Save();

            WeakReferenceMessenger.Default.Send(new ProjectsUpdatedMessage());

            return projectType;
        }

        public void UpdateProjectType(int id, string name, string? description = null)
        {
            var projectType = _uow.ProjectType.GetById(id);
            if (projectType == null)
                throw new InvalidOperationException($"Project type with ID {id} not found.");

            if (!projectType.IsCustom)
                throw new InvalidOperationException("Cannot modify predefined project types.");

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty", nameof(name));

            var existingWithName = _uow.ProjectType.GetByName(name);
            if (existingWithName != null && existingWithName.Id != id)
                throw new InvalidOperationException($"Project type with name '{name}' already exists.");

            projectType.Name = name;
            projectType.Description = description;

            _uow.ProjectType.Update(projectType);
            _uow.Save();

            WeakReferenceMessenger.Default.Send(new ProjectsUpdatedMessage());
        }

        public void DeleteCustomProjectType(int id)
        {
            var projectType = _uow.ProjectType.GetById(id);
            if (projectType == null)
                return;

            if (!projectType.IsCustom)
                throw new InvalidOperationException("Cannot delete predefined project types.");

            // Sprawd?, czy typ nie jest u?ywany w ?adnym projekcie
            var projects = _uow.Project.GetAll();
            if (projects?.Any(p => p.ProjectTypeId == id) == true)
                throw new InvalidOperationException("Cannot delete project type that is used by existing projects.");

            _uow.ProjectType.Remove(projectType);
            _uow.Save();

            WeakReferenceMessenger.Default.Send(new ProjectsUpdatedMessage());
        }
    }
}
