using Microsoft.Maui.Controls;
using ProjectPlanner.Model;
using ProjectPlanner.Service;
using System.Linq;

namespace ProjectPlanner.Pages;

public partial class AddOrEditTask : ContentPage
{
    private readonly IProjectService _projectService;
    private readonly Project _project;
    private readonly int? _taskId;
    public int? TaskId => _taskId;

    public AddOrEditTask(Project project, IProjectService projectService, int? taskId = null)
    {
        InitializeComponent();
        _project = project;
        _projectService = projectService;
        _taskId = taskId;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Safely get tasks for the project (guard against null)
        var tasks = _projectService.GetTasksForProject(_project.Id) ?? Enumerable.Empty<SubTask>();

        // Determine the SubTask instance to use for populating the UI
        SubTask task;
        if (_taskId.HasValue)
        {
            task = tasks.FirstOrDefault(t => t.Id == _taskId.Value);
        }
        else
        {
            task = new SubTask
            {
                ProjectId = _project.Id,
                Name = string.Empty,
                Description = string.Empty
            };
        }

        entry_project_name.Text = _project.Name ?? string.Empty;
        entry_task_name.Text = task.Name ?? string.Empty;
        entry_task_description.Text = task.Description ?? string.Empty;
    }

    private void OnEntryTextChanged_project_name(object sender, TextChangedEventArgs e)
    {
        // Handle project name text changed logic here
    }

    private void OnEntryTextChanged_task_name(object sender, TextChangedEventArgs e)
    {
        // Handle task name text changed logic here
    }

    private void OnEntryTextChanged_task_description(object sender, TextChangedEventArgs e)
    {
        // Handle task description text changed logic here
    }
}