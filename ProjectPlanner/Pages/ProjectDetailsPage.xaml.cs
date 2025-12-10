using ProjectPlanner.Model;
using ProjectPlanner.Service;

namespace ProjectPlanner.Pages
{
    public partial class ProjectPage : ContentPage
    {
        private readonly IProjectService _projectService;
        private readonly IProjectTypeService _projectTypeService;
        private Project _project;
        private bool _isEditMode = false;
        public List<SubTask> Tasks { get; set; } = new();

        public ProjectPage(Project project, IProjectService projectService)
        {
            InitializeComponent();
            _project = project;
            _projectService = projectService;
            _projectTypeService = MauiProgram.Services?.GetService<IProjectTypeService>()
                ?? throw new InvalidOperationException("ProjectTypeService not available");
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ReloadAll();

            if (_isEditMode)
            {
                PopulateTypePicker();
            }
        }

        private void ReloadAll()
        {
            var currentProject = _projectService.GetProjectByID(_project.Id);
            if (currentProject == null) return;

            _project = currentProject;

            if (!_isEditMode)
            {
                NameLabel.Text = _project.Name;
                DescriptionLabel.Text = string.IsNullOrWhiteSpace(_project.Description) ? "NO DATA" : _project.Description;
                TypeLabel.Text = _project.Type?.Name?.ToUpper() ?? "NO TYPE";

                TypeLabel.IsVisible = true;
                DescriptionLabel.IsVisible = true;
                view_mode_content.IsVisible = true;
            }

            Tasks = _projectService.GetTasksForProject(_project.Id);
            TasksList.ItemsSource = Tasks ?? new List<SubTask>();
        }

        private void PopulateTypePicker()
        {
            var types = _projectTypeService.GetAllProjectTypes();
            picker_type.ItemsSource = types;
            picker_type.ItemDisplayBinding = new Binding("Name");

            ProjectType? selection = null;
            if (_project.ProjectTypeId.HasValue)
            {
                selection = types.FirstOrDefault(t => t.Id == _project.ProjectTypeId.Value);
            }

            if (selection == null && _project.Type != null)
            {
                selection = types.FirstOrDefault(t => t.Id == _project.Type.Id);
            }

            picker_type.SelectedItem = selection;
        }

        private void ToggleEditMode()
        {
            _isEditMode = !_isEditMode;

            view_mode_content.IsVisible = !_isEditMode;
            edit_mode_content.IsVisible = _isEditMode;

            if (_isEditMode)
            {
                entry_project_name.Text = _project.Name;
                editor_description.Text = _project.Description;

                PopulateTypePicker();

                EditBtn.Text = "X";
                DelProjectBtn.IsVisible = false;
                AddTaskBtn.IsVisible = false;
                edit_buttons_grid.IsVisible = true;
            }
            else
            {
                EditBtn.Text = "EDIT";
                DelProjectBtn.IsVisible = true;
                AddTaskBtn.IsVisible = true;
                edit_buttons_grid.IsVisible = false;
            }
        }

        private async void AddTaskBtn_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddOrEditTask(_project, _projectService));
        }

        private void EditBtn_Clicked(object sender, EventArgs e)
        {
            if (_isEditMode)
            {
                ToggleEditMode();
                ReloadAll();
            }
            else
            {
                ToggleEditMode();
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            var nameInput = entry_project_name.Text?.Trim();
            var descInput = editor_description.Text?.Trim();

            if (string.IsNullOrWhiteSpace(nameInput))
            {
                await DisplayAlert("Error", "Project name is required.", "OK");
                return;
            }

            _project.Name = nameInput;
            _project.Description = descInput;

            if (picker_type.SelectedItem is ProjectType selectedType)
            {
                _project.ProjectTypeId = selectedType.Id;
            }

            _projectService.UpdateProject(_project);

            ToggleEditMode();
            ReloadAll();
        }

        private async void DelProjectBtn_Clicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("DELETE PROJECT",
                $"Permanently delete project [{_project.Name}]? This operation cannot be undone.",
                "DELETE", "CANCEL");

            if (!confirm) return;

            _projectService.DeleteProject(_project);
            await Navigation.PopAsync();
        }

        private async void OnCollectionViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = e.CurrentSelection.FirstOrDefault() as SubTask;
            if (selected == null) return;

            var task = _projectService.GetTasksForProject(_project.Id).FirstOrDefault(p => p.Id == selected.Id) ?? selected;

            await Navigation.PushAsync(new SubtaskDetailsPage(task, _projectService));

            if (sender is CollectionView cv)
            {
                cv.SelectedItem = null;
            }
        }

        private async void OnManageTypesClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ManageProjectTypesPage(_projectTypeService, _projectService));
        }
    }
}