using ProjectPlanner.Model;
using ProjectPlanner.Service;
using Microsoft.Maui.Controls;
using CommunityToolkit.Mvvm.Messaging;
using ProjectPlanner.Model.Messaging;

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
        WeakReferenceMessenger.Default.Register<ProjectsUpdatedMessage>(this, (r, m) =>
        {
            MainThread.BeginInvokeOnMainThread(() => LoadProjects());
        });
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        WeakReferenceMessenger.Default.Unregister<ProjectsUpdatedMessage>(this);
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