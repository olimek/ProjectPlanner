using ProjectPlanner.Service;

namespace ProjectPlanner
{
    public partial class MainPage : ContentPage
    {
        private readonly IProjectService _projectService;

        public MainPage(IProjectService projectService)
        {
            InitializeComponent();
            _projectService = projectService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            ReloadPage();
        }

        private void ReloadPage()
        {
            var dupa = _projectService.GetAllProjects();
            ProjectsList.ItemsSource = _projectService.GetAllProjects();
        }

        private async void AddProjectBtn_Clicked(object sender, EventArgs e)
        {
            string name = await DisplayPromptAsync("Nowy projekt", "Podaj nazwę projektu:");
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            _projectService.AddProject(name);

            ReloadPage();
        }
    }
}