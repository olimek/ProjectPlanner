using ProjectPlanner.Model;
using ProjectPlanner.Service;

namespace ProjectPlanner.Pages;

public partial class AddOrEditTask : ContentPage
{
    private readonly IProjectService? _projectService;
    private readonly Project _project;

    public AddOrEditTask(Project project, IProjectService? projectService)
    {
        InitializeComponent();
    }
}