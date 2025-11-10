using System.Collections.ObjectModel;
using System.Windows.Input;
using ProjectPlanner.Model;
using ProjectPlanner.Data.UnitOfWork;

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

            // Wczytanie projektów z bazy
            var projects = _unitOfWork.Project.GetAll();
            Projects = new ObservableCollection<Project>(projects);

            // Komenda "Dodaj projekt"
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
