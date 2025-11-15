using Microsoft.Maui.Controls;
using ProjectPlanner.Model;
using ProjectPlanner.Service;

namespace ProjectPlanner.Pages
{
    public partial class ProjectPage : ContentPage
    {
        private readonly IProjectService _projectService;
        private readonly Project _project;
        public List<SubTask> Tasks { get; set; } = new();

        public ProjectPage(Project project, IProjectService projectService)
        {
            InitializeComponent();
            _project = project;
            _projectService = projectService;

            NameLabel.Text = _project.Name;
            DescriptionLabel.Text = _project.Description;
            TypeLabel.Text = _project.Type.ToString();
            LoadTasks();
            if (_project.tasks != null)
            {
                LoadTasks();
            }
        }

        private void LoadTasks()
        {
            Tasks = _projectService.GetTasksForProject(_project.Id);
            TasksList.ItemsSource = Tasks;
        }

        private async void addTaskBtn_Clicked(object sender, EventArgs e)
        {
            var taskName = await DisplayPromptAsync("Task name", "");
            if (string.IsNullOrWhiteSpace(taskName)) return;

            var description = await DisplayPromptAsync("Task description", "");
            if (description is null) description = string.Empty;
            _projectService.AddTaskToProject(_project.Id, taskName.Trim(), description.Trim());
            // jeżeli Tasks to ItemsSource
            TasksList.ItemsSource = null;
            LoadTasks();
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

            // Wracamy do listy projektów
            await Navigation.PopAsync();
        }
    }
}