using ProjectPlanner.Model;
using ProjectPlanner.Data.UnitOfWork;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ProjectPlanner.Pages;

namespace ProjectPlanner.ViewModels
{
    public class ProjectDetailsViewModel : BindableObject
    {
        private readonly IUnitOfWork _unitOfWork;

        public Project Project { get; }
        public ObservableCollection<SubTask> SubTasks { get; }

        public ICommand AddSubtaskCommand { get; }
        public ICommand DeleteSubtaskCommand { get; }
        public ICommand DeleteProjectCommand { get; }

        public ProjectDetailsViewModel(Project project, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            Project = project;
            SubTasks = new ObservableCollection<SubTask>(project.tasks ?? new List<SubTask>());

            AddSubtaskCommand = new Command(OnAddSubtask);
            DeleteSubtaskCommand = new Command<SubTask>(OnDeleteSubtask);
            DeleteProjectCommand = new Command(OnDeleteProject);
        }

        private async void OnAddSubtask()
        {
            string title = await Application.Current.MainPage.DisplayPromptAsync("Nowy subtask", "Podaj nazwę:");
            if (string.IsNullOrWhiteSpace(title))
                return;

            // Utwórz nowy subtask
            var subtask = new SubTask
            {
                Name = title,
                Decription = "Nowy subtask"
            };

            // Upewnij się, że lista nie jest null
            Project.tasks ??= new List<SubTask>();

            // Połącz subtask z projektem
            Project.tasks.Add(subtask);

            // Zapisz do bazy
            _unitOfWork.Task.Add(subtask);
            _unitOfWork.Project.Update(Project);
            _unitOfWork.Save();

            // Odśwież listę widoku
            SubTasks.Add(subtask);
        }


        private async void OnDeleteSubtask(SubTask subtask)
        {
            bool confirm = await Application.Current.MainPage.DisplayAlert("Usuń zadanie", $"Usunąć '{subtask.Name}'?", "Tak", "Nie");
            if (!confirm)
                return;

            Project.tasks.Remove(subtask);
            _unitOfWork.Task.Remove(subtask);
            _unitOfWork.Save();
            SubTasks.Remove(subtask);
        }

        private async void OnDeleteProject()
        {
            bool confirm = await Application.Current.MainPage.DisplayAlert("Usuń projekt", $"Usunąć projekt '{Project.Name}'?", "Tak", "Nie");
            if (!confirm)
                return;

            _unitOfWork.Project.Remove(Project);
            _unitOfWork.Save();

            await Application.Current.MainPage.Navigation.PopAsync();

            // 🔹 Po powrocie do MainPage odśwież listę projektów
            if (Application.Current.MainPage is NavigationPage navPage &&
                navPage.CurrentPage is MainPage mainPage &&
                mainPage.BindingContext is MainPageViewModel vm)
            {
                vm.RefreshProjects();
            }
        }
    }
}
