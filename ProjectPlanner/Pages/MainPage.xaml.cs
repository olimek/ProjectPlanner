using ProjectPlanner.Model;
using ProjectPlanner.Service;
using Microsoft.Maui.Controls;

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
        MessagingCenter.Subscribe<object>(this, "ProjectsUpdated", (sender) =>
        {
            MainThread.BeginInvokeOnMainThread(() => LoadProjects());
        });
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        MessagingCenter.Unsubscribe<object>(this, "ProjectsUpdated");
    }

    private void LoadProjects()
    {
        Projects = _projectService.GetAllProjects();
        ProjectsList.ItemsSource = Projects;
    }

    private async void AddProjectBtn_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AddOrEditProject(null, _projectService));
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