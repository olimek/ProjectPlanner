using System.Collections.ObjectModel;
using System.Windows.Input;
using ProjectPlanner.Data.UnitOfWork;
using ProjectPlanner.Model;

namespace ProjectPlanner.ViewModels
{
    public class MainPageViewModel : BindableObject
    {
        private readonly IUnitOfWork _unitOfWork;

        public ObservableCollection<Project> Projects { get; set; }
        public ICommand AddProjectCommand { get; }

        public MainPageViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            var projects = _unitOfWork.Project.GetAll();
            Projects = new ObservableCollection<Project>(projects);
            AddProjectCommand = new Command(OnAddProject);
        }

        private void OnAddProject()
        {
            var newProject = new Project
            {
                Name = $"Projekt {Projects.Count + 1}",
                Description = "Nowy projekt testowy"
            };

            _unitOfWork.Project.Add(newProject);
            _unitOfWork.Save();
            Projects.Add(newProject);
        }
    }
}
