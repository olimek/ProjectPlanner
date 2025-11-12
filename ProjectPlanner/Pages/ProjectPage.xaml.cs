using ProjectPlanner.Model;
using ProjectPlanner.Service;

namespace ProjectPlanner.Pages
{
    public partial class ProjectPage : ContentPage
    {
        private readonly IProjectService _projectService;
        private readonly Project _project;

        public ProjectPage(Project project, IProjectService projectService)
        {
            InitializeComponent();
            _project = project;
            _projectService = projectService;

            NameLabel.Text = _project.Name;
            DescriptionLabel.Text = _project.Description;
            TypeLabel.Text = _project.Type.ToString();

            if (_project.tasks != null)
            {
                TasksList.ItemsSource = _project.tasks;
            }
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

            // Usuń projekt przez serwis
            _projectService.DeleteProject(_project);

            // Wracamy do listy projektów
            await Navigation.PopAsync();
        }
    }
}