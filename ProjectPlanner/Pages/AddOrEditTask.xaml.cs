using ProjectPlanner.Model;
using ProjectPlanner.Service;

namespace ProjectPlanner.Pages;

public partial class AddOrEditTask : ContentPage
{
    private readonly IProjectService _projectService;
    private readonly Project _project;
    private readonly int? _taskId;
    public int? TaskId => _taskId;

    private SubTask? _currentTask;

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

        var tasks = _projectService.GetTasksForProject(_project.Id) ?? Enumerable.Empty<SubTask>();

        if (_taskId.HasValue)
        {
            _currentTask = tasks.FirstOrDefault(t => t.Id == _taskId.Value);
            if (_currentTask == null)
            {
                _currentTask = new SubTask
                {
                    ProjectId = _project.Id,
                    Name = string.Empty,
                    Description = string.Empty
                };
            }
        }
        else
        {
            _currentTask = new SubTask
            {
                ProjectId = _project.Id,
                Name = string.Empty,
                Description = string.Empty
            };
        }

        entry_project_name.Text = _project.Name ?? string.Empty;
        entry_task_name.Text = _currentTask.Name ?? string.Empty;
        entry_task_description.Text = _currentTask.Description ?? string.Empty;
    }

    private void OnEntryTextChanged_project_name(object sender, TextChangedEventArgs e)
    {
    }

    private void OnEntryTextChanged_task_name(object sender, TextChangedEventArgs e)
    {
        if (_currentTask == null)
        {
            return;
        }

        _currentTask.Name = e.NewTextValue ?? string.Empty;

        PersistTaskChange();
    }

    private void OnEntryTextChanged_task_description(object sender, TextChangedEventArgs e)
    {
        if (_currentTask == null)
        {
            return;
        }

        _currentTask.Description = e.NewTextValue ?? string.Empty;

        PersistTaskChange();
    }

    private void PersistTaskChange()
    {
        if (_currentTask == null)
        {
            return;
        }

        if (_currentTask.Id > 0)
        {
            _projectService.UpdateTask(_currentTask);
        }
        else
        {
            _projectService.AddTaskToProject(_project, _currentTask.Name, _currentTask.Description);
            var newTask = _projectService.GetTasksForProject(_project.Id).FirstOrDefault(t => t.Name == _currentTask.Name);
            if (newTask != null)
            {
                _currentTask = newTask;
            }
        }
    }
}