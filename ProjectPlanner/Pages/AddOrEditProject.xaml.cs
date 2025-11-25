using ProjectPlanner.Model;
using ProjectPlanner.Service;
using System;
using System.Linq;
using System.Xml;

namespace ProjectPlanner.Pages;

public partial class AddOrEditProject : ContentPage
{
    private readonly IProjectService _projectService;
    private readonly Project _project;

    public AddOrEditProject(Project project, IProjectService projectService)
    {
        InitializeComponent();
        _project = project;
        _projectService = projectService;
        var typeNames = Enum.GetValues(typeof(ProjectType))
                    .Cast<ProjectType>()
                    .Select(t => t.ToString())
                    .ToList();

        picker.ItemsSource = typeNames;

        entry_project_name.Text = _project.Name;
        entry_project_description.Text = _project.Description;

        var selectedName = _project.Type.ToString();
        var index = typeNames.IndexOf(selectedName);
        picker.SelectedIndex = index;
    }
}