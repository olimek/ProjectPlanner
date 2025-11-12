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
            _uow.Project.Remove(project);
            _uow.Save();
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