using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
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
        private List<SubTask> _allTasks { get; set; } = new();
        private string _searchQuery = string.Empty;
        private SearchScope _searchScope = SearchScope.Name;
        private TaskSortField _sortField = TaskSortField.Priority;
        private SortDirection _sortDirection = SortDirection.Descending;
        private bool _hideCompleted;
        private readonly Picker _searchScopePicker;
        private readonly Picker _sortFieldPicker;
        private readonly Picker _sortDirectionPicker;

        public ProjectPage(Project project, IProjectService projectService)
        {
            InitializeComponent();
            _project = project;
            _projectService = projectService;
            _projectTypeService = MauiProgram.Services?.GetService<IProjectTypeService>()
                ?? throw new InvalidOperationException("ProjectTypeService not available");

            _searchScopePicker = this.FindByName<Picker>("SearchScopePicker") ?? throw new InvalidOperationException("Search scope picker not found");
            _sortFieldPicker = this.FindByName<Picker>("SortFieldPicker") ?? throw new InvalidOperationException("Sort field picker not found");
            _sortDirectionPicker = this.FindByName<Picker>("SortDirectionPicker") ?? throw new InvalidOperationException("Sort direction picker not found");

            _searchScopePicker.SelectedIndex = 0;
            _sortFieldPicker.SelectedIndex = 0;
            _sortDirectionPicker.SelectedIndex = 0;
            chk_hide_done.IsChecked = false;
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

            _allTasks = _projectService.GetTasksForProject(_project.Id) ?? new List<SubTask>();
            ApplyTaskFilters();
        }

        private void ApplyTaskFilters()
        {
            if (_allTasks == null)
            {
                TasksList.ItemsSource = new List<SubTask>();
                return;
            }

            IEnumerable<SubTask> filtered = _allTasks;

            if (!string.IsNullOrWhiteSpace(_searchQuery))
            {
                if (_searchScope == SearchScope.Tags)
                {
                    var requestedTags = _searchQuery
                        .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                    if (requestedTags.Length > 0)
                    {
                        filtered = filtered.Where(task =>
                        {
                            if (string.IsNullOrWhiteSpace(task.Tags))
                                return false;

                            var taskTags = task.Tags
                                .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                            if (taskTags.Length == 0)
                                return false;

                            return requestedTags.All(tag => taskTags.Any(taskTag =>
                                taskTag.Contains(tag, StringComparison.OrdinalIgnoreCase)));
                        });
                    }
                }
                else
                {
                    filtered = filtered.Where(task =>
                        (!string.IsNullOrWhiteSpace(task.Name) &&
                         task.Name.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrWhiteSpace(task.Description) &&
                         task.Description.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase)));
                }
            }

            if (_hideCompleted)
            {
                filtered = filtered.Where(task => !task.IsDone);
            }

            filtered = (_sortField, _sortDirection) switch
            {
                (TaskSortField.Alphabetical, SortDirection.Ascending) => filtered
                    .OrderBy(task => task.Name, StringComparer.OrdinalIgnoreCase),
                (TaskSortField.Alphabetical, SortDirection.Descending) => filtered
                    .OrderByDescending(task => task.Name, StringComparer.OrdinalIgnoreCase),
                (TaskSortField.Priority, SortDirection.Ascending) => filtered
                    .OrderBy(task => task.Priority)
                    .ThenBy(task => task.Name, StringComparer.OrdinalIgnoreCase),
                _ => filtered
                    .OrderByDescending(task => task.Priority)
                    .ThenBy(task => task.Name, StringComparer.OrdinalIgnoreCase)
            };

            Tasks = filtered.ToList();
            TasksList.ItemsSource = Tasks;
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

        private void OnTaskSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            _searchQuery = e.NewTextValue?.Trim() ?? string.Empty;
            ApplyTaskFilters();
        }

        private void OnSearchScopeChanged(object sender, EventArgs e)
        {
            _searchScope = _searchScopePicker.SelectedIndex switch
            {
                1 => SearchScope.Tags,
                _ => SearchScope.Name
            };

            ApplyTaskFilters();
        }

        private void OnSortFieldChanged(object sender, EventArgs e)
        {
            _sortField = _sortFieldPicker.SelectedIndex switch
            {
                1 => TaskSortField.Alphabetical,
                _ => TaskSortField.Priority
            };

            ApplyTaskFilters();
        }

        private void OnSortDirectionChanged(object sender, EventArgs e)
        {
            _sortDirection = _sortDirectionPicker.SelectedIndex switch
            {
                1 => SortDirection.Ascending,
                _ => SortDirection.Descending
            };

            ApplyTaskFilters();
        }

        private void OnHideDoneChanged(object sender, CheckedChangedEventArgs e)
        {
            _hideCompleted = e.Value;
            ApplyTaskFilters();
        }

        private async void OnManageTypesClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ManageProjectTypesPage(_projectTypeService, _projectService));
        }

        private enum SearchScope
        {
            Name = 0,
            Tags = 1
        }

        private enum TaskSortField
        {
            Priority = 0,
            Alphabetical = 1
        }

        private enum SortDirection
        {
            Descending = 0,
            Ascending = 1
        }
    }
}