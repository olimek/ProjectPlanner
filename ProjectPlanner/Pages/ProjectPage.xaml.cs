using System;
using System.Linq;
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

            if (_project.Tasks != null)
            {
                TasksList.ItemsSource = _project.Tasks;
            }
        }

        private async void AddTaskBtn_Clicked(object sender, EventArgs e)
        {
            _projectService.AddTaskToProject(_project, "test_" + _project.Name, "sdfsdthhsfgh");

            // Refresh tasks list from service to ensure we show the DB state (prevents duplicates/inconsistency)
            var updatedProject = _projectService.GetAllProjects().FirstOrDefault(p => p.Id == _project.Id);
            if (updatedProject != null)
            {
                // reset the ItemsSource to force UI refresh
                TasksList.ItemsSource = null;
                TasksList.ItemsSource = updatedProject.Tasks;

                // keep the local project instance in sync
                _project.Tasks = updatedProject.Tasks;
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

            // Wracamy do listy projektów
            await Navigation.PopAsync();
        }
    }
}