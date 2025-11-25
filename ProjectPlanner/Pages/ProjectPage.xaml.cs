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
            _projectService.AddTaskToProject(_project, "test_" + _project.Name, "sdfsdthhsfgh");
            var updatedProject = _projectService.GetAllProjects().FirstOrDefault(p => p.Id == _project.Id);
            if (updatedProject != null)
            {
                TasksList.ItemsSource = null;
                TasksList.ItemsSource = updatedProject.Tasks;

                ReloadAll();
            }
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

        private async void addTaskBtn_Clicked(object sender, EventArgs e)
        {
            var taskName = await DisplayPromptAsync("Task name", "");
            if (string.IsNullOrWhiteSpace(taskName)) return;

            var description = await DisplayPromptAsync("Task description", "");
            if (description is null) description = string.Empty;
            _projectService.AddTaskToProject(_project.Id, taskName.Trim(), description.Trim());
            ReloadAll();
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
    }
}