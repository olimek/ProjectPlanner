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

    private void OnEntryTextChanged_project_name(object sender, EventArgs e)
    {
        var name = entry_project_name.Text ?? string.Empty;
        _project.Name = name;

        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        if (_project.Id == 0)
        {
            var created = _projectService?.AddProject(name.Trim(), _project.Description, _project.Type);
            if (created != null)
            {
                _project.Id = created.Id;
            }
        }
        else
        {
            _projectService?.UpdateProject(_project.Id, _project.Name, description: _project.Description, projectType: _project.Type.ToString());
        }
    }

    private void OnEntryTextChanged_project_description(object sender, EventArgs e)
    {
        var description = entry_project_description.Text ?? string.Empty;
        _project.Description = description;

        if (_project.Id != 0)
        {
            _projectService?.UpdateProject(_project.Id, _project.Name, description: _project.Description, projectType: _project.Type.ToString());
            return;
        }

        if (!string.IsNullOrWhiteSpace(_project.Name))
        {
            var created = _projectService?.AddProject(_project.Name, _project.Description, _project.Type);
            if (created != null)
            {
                _project.Id = created.Id;
            }
        }
    }

    private void OnPickerSelectedIndexChanged(object sender, EventArgs e)
    {
        if (picker.SelectedIndex == -1) return;

        string selectedItem = picker.Items[picker.SelectedIndex];
        _project.Type = Enum.Parse<ProjectType>(selectedItem);

        if (_project.Id != 0)
        {
            _projectService?.UpdateProject(_project.Id, _project.Name, description: _project.Description, projectType: _project.Type.ToString());
            return;
        }

        if (!string.IsNullOrWhiteSpace(_project.Name))
        {
            var created = _projectService?.AddProject(_project.Name, _project.Description, _project.Type);
            if (created != null)
            {
                _project.Id = created.Id;
            }
        }
    }
}