using ProjectPlanner.Model;
using ProjectPlanner.Service;

namespace ProjectPlanner.Pages;

public partial class AddOrEditProject : ContentPage
{
    private readonly IProjectService? _projectService;
    private readonly IProjectTypeService? _projectTypeService;
    private readonly Project _project;
    private List<ProjectType> _projectTypes = new();
    private const string ADD_CUSTOM_TYPE_OPTION = "➕ Add own project type...";

    public AddOrEditProject()
        : this(null, null, null)
    {
    }

    public AddOrEditProject(Project? project, IProjectService? projectService)
        : this(project, projectService, null)
    {
    }

    public AddOrEditProject(Project? project, IProjectService? projectService, IProjectTypeService? projectTypeService)
    {
        InitializeComponent();
        _project = project ?? new Project();
        _projectService = projectService;
        _projectTypeService = projectTypeService ?? MauiProgram.Services?.GetService<IProjectTypeService>();

        LoadProjectTypes();
        picker.SelectedIndexChanged += OnPickerSelectedIndexChanged;
    }

    private void LoadProjectTypes()
    {
        if (_projectTypeService == null)
        {
            _projectTypes = new List<ProjectType>();
            return;
        }

        _projectTypes = _projectTypeService.GetAllProjectTypes();
        var typeNames = _projectTypes.Select(t => t.Name).ToList();
        
        // Dodaj opcję tworzenia własnego typu na końcu
        typeNames.Add(ADD_CUSTOM_TYPE_OPTION);
        
        picker.ItemsSource = typeNames;

        entry_project_name.Text = _project.Name ?? string.Empty;
        entry_project_description.Text = _project.Description ?? string.Empty;

        if (_project.Type != null)
        {
            var selectedIndex = _projectTypes.FindIndex(t => t.Id == _project.Type.Id);
            if (selectedIndex >= 0)
                picker.SelectedIndex = selectedIndex;
        }
    }

    private async void OnPickerSelectedIndexChanged(object? sender, EventArgs e)
    {
        if (picker.SelectedIndex == -1)
            return;

        var selectedItem = picker.Items[picker.SelectedIndex];
        
        if (selectedItem == ADD_CUSTOM_TYPE_OPTION)
        {
            await HandleAddCustomType();
        }
    }

    private async Task HandleAddCustomType()
    {
        if (_projectTypeService == null)
        {
            await DisplayAlert("Error", "Project type service is not available.", "OK");
            picker.SelectedIndex = -1;
            return;
        }

        string? typeName = await DisplayPromptAsync(
            "New Project Type",
            "Enter the name for your custom project type:",
            placeholder: "e.g., Gardening, 3D Printing, etc.",
            maxLength: 100);

        if (string.IsNullOrWhiteSpace(typeName))
        {
            picker.SelectedIndex = -1;
            return;
        }

        string? description = await DisplayPromptAsync(
            "Description (Optional)",
            "Enter a description for this type:",
            placeholder: "Optional description...");

        try
        {
            var newType = _projectTypeService.AddCustomProjectType(typeName, description);
            
            // Przeładuj listę typów
            _projectTypes = _projectTypeService.GetAllProjectTypes();
            var typeNames = _projectTypes.Select(t => t.Name).ToList();
            typeNames.Add(ADD_CUSTOM_TYPE_OPTION);
            picker.ItemsSource = typeNames;
            
            // Wybierz nowo utworzony typ
            var newTypeIndex = _projectTypes.FindIndex(t => t.Id == newType.Id);
            if (newTypeIndex >= 0)
            {
                picker.SelectedIndex = newTypeIndex;
            }
            
            await DisplayAlert("Success", $"Project type '{typeName}' created successfully!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
            picker.SelectedIndex = -1;
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        if (Navigation != null)
        {
            await Navigation.PopAsync();
            return;
        }

        if (Shell.Current != null)
        {
            await Shell.Current.GoToAsync("..");
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        var nameInput = entry_project_name.Text?.Trim();
        var descriptionInput = entry_project_description.Text?.Trim();

        if (string.IsNullOrWhiteSpace(nameInput))
        {
            await DisplayAlert("Error", "Project name is required.", "OK");
            return;
        }

        if (picker.SelectedIndex == -1)
        {
            await DisplayAlert("Error", "You must select a project type.", "OK");
            return;
        }

        // Sprawdź, czy użytkownik przypadkowo nie wybrał opcji "Add own project type"
        var selectedItem = picker.Items[picker.SelectedIndex];
        if (selectedItem == ADD_CUSTOM_TYPE_OPTION)
        {
            await DisplayAlert("Error", "Please select a project type or create a new one.", "OK");
            return;
        }

        _project.Name = nameInput;
        _project.Description = descriptionInput;

        var selectedProjectType = _projectTypes[picker.SelectedIndex];
        _project.ProjectTypeId = selectedProjectType.Id;

        if (_project.Id == 0)
        {
            if (_projectService == null)
            {
                await DisplayAlert("Error", "Project service is not available.", "OK");
                return;
            }

            var createdProject = _projectService.AddProject(
                _project.Name,
                _project.Description,
                selectedProjectType
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
                projectType: selectedProjectType.Name
            );
        }

        if (Navigation != null)
        {
            await Navigation.PopAsync();
            return;
        }

        if (Shell.Current != null)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}