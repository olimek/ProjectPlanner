using ProjectPlanner.Model;
using ProjectPlanner.Service;

namespace ProjectPlanner.Pages;

public partial class AddOrEditProject : ContentPage
{
    private readonly IProjectService? _projectService;
    private readonly Project _project;

    public AddOrEditProject()
        : this(null, null)
    {
    }

    public AddOrEditProject(Project? project, IProjectService? projectService)
    {
        InitializeComponent();
        _project = project ?? new Project();
        _projectService = projectService;

        var typeNames = Enum.GetValues(typeof(ProjectType))
                    .Cast<ProjectType>()
                    .Select(t => t.ToString())
                    .ToList();

        picker.ItemsSource = typeNames;

        entry_project_name.Text = _project.Name ?? string.Empty;
        entry_project_description.Text = _project.Description ?? string.Empty;

        var selectedName = _project.Type.ToString();
        var index = typeNames.IndexOf(selectedName);
        picker.SelectedIndex = index;
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        var nameInput = entry_project_name.Text?.Trim();
        var descriptionInput = entry_project_description.Text?.Trim();

        if (string.IsNullOrWhiteSpace(nameInput))
        {
            await DisplayAlert("Błąd", "Nazwa projektu jest wymagana.", "OK");
            return;
        }

        if (picker.SelectedIndex == -1)
        {
            await DisplayAlert("Błąd", "Musisz wybrać typ projektu.", "OK");
            return;
        }

        _project.Name = nameInput;
        _project.Description = descriptionInput;

        string selectedItem = picker.Items[picker.SelectedIndex];
        _project.Type = Enum.Parse<ProjectType>(selectedItem);

        if (_project.Id == 0)
        {
            var createdProject = _projectService?.AddProject(
                _project.Name,
                _project.Description,
                _project.Type
            );

            if (createdProject != null)
            {
                _project.Id = createdProject.Id;
            }
        }
        else
        {
            _projectService?.UpdateProject(
                _project.Id,
                _project.Name,
                description: _project.Description,
                projectType: _project.Type.ToString()
            );
        }

        await Shell.Current.GoToAsync("..");
    }
}