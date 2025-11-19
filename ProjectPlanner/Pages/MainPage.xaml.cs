using ProjectPlanner.Model;
using ProjectPlanner.Pages;
using ProjectPlanner.Service;

namespace ProjectPlanner.Pages;

public partial class MainPage : ContentPage
{
    private readonly IProjectService _projectService;

    public List<Project> Projects { get; set; } = new();

    public MainPage(IProjectService projectService)
    {
        InitializeComponent();
        _projectService = projectService;

        BindingContext = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadProjects();
    }

    private void LoadProjects()
    {
        Projects = _projectService.GetAllProjects();
        ProjectsList.ItemsSource = Projects;
    }

    private async void AddProjectBtn_Clicked(object sender, EventArgs e)
    {
        string name = await DisplayPromptAsync("Nowy projekt", "Podaj nazwę projektu:");
        if (string.IsNullOrWhiteSpace(name))
            return;

        _projectService.AddProject(name);
        LoadProjects();
    }

    private async void ProjectsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Project selected)
        {
            await Navigation.PushAsync(new ProjectPage(selected, _projectService));
            ((CollectionView)sender).SelectedItem = null;
        }
    }
}