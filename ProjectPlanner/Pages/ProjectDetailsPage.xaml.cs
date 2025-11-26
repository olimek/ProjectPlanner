using ProjectPlanner.Model;
using ProjectPlanner.Service;

namespace ProjectPlanner.Pages
{
    public partial class ProjectPage : ContentPage
    {
        private readonly IProjectService _projectService;
        private Project _project;
        public List<SubTask> Tasks { get; set; } = new();

        public ProjectPage(Project project, IProjectService projectService)
        {
            InitializeComponent();
            _project = project;
            _projectService = projectService;
            ReloadAll();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ReloadAll();
        }

        private async void AddTaskBtn_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddOrEditTask(_project, _projectService));
        }

        private void ReloadAll()
        {
            var projectasdasd = _projectService.GetProjectByID(_project.Id);
            Tasks = _projectService.GetTasksForProject(_project.Id);
            TasksList.ItemsSource = Tasks;

            NameLabel.Text = projectasdasd.Name;
            DescriptionLabel.Text = projectasdasd.Description;
            TypeLabel.Text = projectasdasd.Type.ToString();

            if (projectasdasd.Tasks != null)
            {
                TasksList.ItemsSource = projectasdasd.Tasks;
            }
        }

        private async void EditBtn_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddOrEditProject(_project, _projectService));
        }

        private async void DelProjectBtn_Clicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Potwierdzenie",
                $"Czy na pewno chcesz usunąć projekt \"{_project.Name}\"?",
                "Usuń", "Anuluj");

            if (!confirm)
            {
                return;
            }

            _projectService.DeleteProject(_project);
            await Navigation.PopAsync();
        }

        private async void OnCollectionViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = e.CurrentSelection.FirstOrDefault() as SubTask;
            if (selected == null)
            {
                return;
            }
            var task = _projectService.GetTasksForProject(_project.Id).FirstOrDefault(p => p.Id == selected.Id) ?? selected;

            await Navigation.PushAsync(new SubtaskDetailsPage(task, _projectService));

            if (sender is CollectionView cv)
            {
                cv.SelectedItem = null;
            }
        }
    }
}