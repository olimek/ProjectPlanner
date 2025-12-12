using ProjectPlanner.Model;
using ProjectPlanner.Service;

namespace ProjectPlanner.Pages;

public partial class ManageProjectTypesPage : ContentPage
{
    private readonly IProjectTypeService _projectTypeService;
    private readonly IProjectService _projectService;

    public ManageProjectTypesPage(IProjectTypeService projectTypeService, IProjectService projectService)
    {
        InitializeComponent();
        _projectTypeService = projectTypeService;
        _projectService = projectService;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadProjectTypes();
    }

    private void LoadProjectTypes()
    {
        var predefinedTypes = _projectTypeService.GetPredefinedProjectTypes();
        var customTypes = _projectTypeService.GetCustomProjectTypes();

        PredefinedTypesList.ItemsSource = predefinedTypes;
        CustomTypesList.ItemsSource = customTypes;
    }

    private async void OnAddCustomTypeClicked(object sender, EventArgs e)
    {
        string? name = await DisplayPromptAsync(
            "New Project Type",
            "Enter the name for your custom project type:",
            placeholder: "e.g., Gardening, 3D Printing, etc.",
            maxLength: 100);

        if (string.IsNullOrWhiteSpace(name))
            return;

        string? description = await DisplayPromptAsync(
            "Description (Optional)",
            "Enter a description for this type:",
            placeholder: "Optional description...");

        try
        {
            _projectTypeService.AddCustomProjectType(name, description);
            LoadProjectTypes();
            await DisplayAlert("Success", $"Project type '{name}' created successfully!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnCustomTypeSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not ProjectType selectedType)
            return;

        var action = await DisplayActionSheet(
            $"Manage '{selectedType.Name}'",
            "Cancel",
            null,
            "[EDIT]",
            "[DELETE]");

        if (action == "[EDIT]")
        {
            await EditCustomType(selectedType);
        }
        else if (action == "[DELETE]")
        {
            await DeleteCustomType(selectedType);
        }

        ((CollectionView)sender).SelectedItem = null;
    }

    private async Task EditCustomType(ProjectType projectType)
    {
        string? name = await DisplayPromptAsync(
            "Edit Type",
            "Enter new name:",
            initialValue: projectType.Name,
            maxLength: 100);

        if (string.IsNullOrWhiteSpace(name))
            return;

        string? description = await DisplayPromptAsync(
            "Edit Description",
            "Enter new description:",
            initialValue: projectType.Description ?? string.Empty);

        try
        {
            _projectTypeService.UpdateProjectType(projectType.Id, name, description);
            LoadProjectTypes();
            await DisplayAlert("Success", "Project type updated successfully!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async Task DeleteCustomType(ProjectType projectType)
    {
        // Sprawd?, czy typ jest u?ywany
        var projects = _projectService.GetAllProjects();
        var usageCount = projects.Count(p => p.ProjectTypeId == projectType.Id);

        string message = usageCount > 0
            ? $"This type is used by {usageCount} project(s). Deleting it will prevent you from creating new projects with this type, but existing projects will keep their type. Continue?"
            : $"Are you sure you want to delete '{projectType.Name}'?";

        bool confirm = await DisplayAlert(
            "Delete Type",
            message,
            "Delete",
            "Cancel");

        if (!confirm)
            return;

        try
        {
            _projectTypeService.DeleteCustomProjectType(projectType.Id);
            LoadProjectTypes();
            await DisplayAlert("Success", "Project type deleted successfully!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}
