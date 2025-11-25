using ProjectPlanner.Model;
using ProjectPlanner.Service;
using System;
using System.Diagnostics;
using System.Linq;
using System.Xml;

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

        // Provide a default Project if none was supplied.
        _project = project ?? new Project();
        _projectService = projectService;

        var typeNames = Enum.GetValues(typeof(ProjectType))
                    .Cast<ProjectType>()
                    .Select(t => t.ToString())
                    .ToList();

        picker.ItemsSource = typeNames;

        // Safely set entry texts (avoid null refs).
        entry_project_name.Text = _project.Name ?? string.Empty;
        entry_project_description.Text = _project.Description ?? string.Empty;

        var selectedName = _project.Type.ToString();
        var index = typeNames.IndexOf(selectedName);
        picker.SelectedIndex = index;
    }

    private void OnEntryTextChanged_project_name(object sender, EventArgs e)
    { }

    private void OnEntryTextChanged_project_description(object sender, EventArgs e)
    { }

    private void OnPickerSelectedIndexChanged(object sender, EventArgs e)
    {
        if (picker.SelectedIndex != -1)
        {
            string selectedItem = picker.Items[picker.SelectedIndex];
            _project.Type = Enum.Parse<ProjectType>(selectedItem);
            // Call UpdateProject only if a service is available.
            _projectService?.UpdateProject(_project.Id, _project.Name, projectType: _project.Type.ToString());
        }
    }
}