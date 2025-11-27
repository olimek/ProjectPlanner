using ProjectPlanner.Model;
using ProjectPlanner.Service;

namespace ProjectPlanner.Pages;

public partial class AddOrEditTask : ContentPage
{
    private readonly IProjectService _projectService;
    private readonly Project _project;
    private SubTask? _currentTask;
    private readonly int? _taskId;

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
        LoadData();
    }

    private void LoadData()
    {
        lbl_project_name.Text = _project.Name?.ToUpper() ?? "NIEZNANY PROJEKT";
        if (_taskId.HasValue)
        {
            var tasks = _projectService.GetTasksForProject(_project.Id);
            _currentTask = tasks.FirstOrDefault(t => t.Id == _taskId.Value);
        }
        if (_currentTask != null)
        {
            entry_task_name.Text = _currentTask.Name;
            entry_task_description.Text = _currentTask.Description;
            Title = "EDYCJA ZADANIA";
        }
        else
        {
            Title = "NOWE ZADANIE";
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        var nameInput = entry_task_name.Text?.Trim();
        var descInput = entry_task_description.Text?.Trim();

        if (string.IsNullOrWhiteSpace(nameInput))
        {
            await DisplayAlert("Błąd", "Nazwa zadania jest wymagana.", "OK");
            return;
        }
        if (_currentTask != null)
        {
            _currentTask.Name = nameInput;
            _currentTask.Description = descInput;

            _projectService.UpdateTask(_currentTask);
        }
        else
        {
            var newTask = new SubTask
            {
                ProjectId = _project.Id,
                Name = nameInput,
                Description = descInput,
                IsDone = false
            };

            _projectService.AddTaskToProject(_project, newTask.Name, newTask.Description);
        }

        await Shell.Current.GoToAsync("..");
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}