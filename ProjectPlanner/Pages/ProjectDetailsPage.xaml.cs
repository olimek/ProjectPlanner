using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using ProjectPlanner.Model;
using ProjectPlanner.Service;

namespace ProjectPlanner.Pages
{
    public partial class ProjectPage : ContentPage
    {
        private readonly IProjectService _projectService;
        private readonly IProjectTypeService _projectTypeService;
        private Project _project;
        private bool _isEditMode;
        private List<SubTask> _allTasks = new();
        private string _searchQuery = string.Empty;
        private SearchScope _searchScope = SearchScope.Name;
        private TaskSortField _sortField = TaskSortField.Priority;
        private SortDirection _sortDirection = SortDirection.Descending;
        private bool _hideCompleted;
        private bool _filtersExpanded;
        private const string SearchLabelCollapsed = "SEARCH";
        private const string SearchLabelExpanded = "CLOSE";
        private readonly Picker _searchScopePicker;
        private readonly Picker _sortFieldPicker;
        private const string HideCompletedPreferenceKeyPrefix = "ProjectPage.HideCompleted.";

        public ProjectPage(Project project, IProjectService projectService)
        {
            InitializeComponent();
            _project = project;
            _projectService = projectService;
            _projectTypeService = MauiProgram.Services?.GetService<IProjectTypeService>()
                ?? throw new InvalidOperationException("ProjectTypeService not available");

            _searchScopePicker = this.FindByName<Picker>("SearchScopePicker") ?? throw new InvalidOperationException("Search scope picker not found");
            _sortFieldPicker = this.FindByName<Picker>("SortFieldPicker") ?? throw new InvalidOperationException("Sort field picker not found");

            _searchScopePicker.SelectedIndex = 0;
            _sortFieldPicker.SelectedIndex = 0;
            RestoreHideCompletedPreference();

            UpdateSearchPanelVisualState();
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

            _allTasks = _projectService.GetTasksForProject(_project.Id) ?? [];
            ApplyTaskFilters();
        }

        private void ApplyTaskFilters()
        {
            if (_allTasks is not { Count: > 0 })
            {
                TasksList.ItemsSource = Array.Empty<SubTask>();
                return;
            }

            var requestedTags = _searchScope == SearchScope.Tags
                ? SplitTags(_searchQuery)
                : Array.Empty<string>();

            var filtered = _allTasks
                .Where(task => MatchesSearch(task, requestedTags) && (_hideCompleted is false || !task.IsDone));

            TasksList.ItemsSource = SortTasks(filtered).ToList();
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
            DelProjectBtn.IsVisible = !_isEditMode;
            AddTaskBtn.IsVisible = !_isEditMode;
            edit_buttons_grid.IsVisible = _isEditMode;
            EditBtn.Text = _isEditMode ? "X" : "EDIT";

            if (_isEditMode)
            {
                entry_project_name.Text = _project.Name;
                editor_description.Text = _project.Description;

                PopulateTypePicker();
            }
        }

        private async void AddTaskBtn_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddOrEditTask(_project, _projectService));
        }

        private void EditBtn_Clicked(object sender, EventArgs e)
        {
            var wasEditMode = _isEditMode;
            ToggleEditMode();

            if (wasEditMode)
            {
                ReloadAll();
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
            if (e.CurrentSelection.Count > 0 && e.CurrentSelection[0] is SubTask selected)
            {
                var task = _allTasks.FirstOrDefault(p => p.Id == selected.Id) ?? selected;

                await Navigation.PushAsync(new SubtaskDetailsPage(task, _projectService));
            }

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
            (_sortField, _sortDirection) = _sortFieldPicker.SelectedIndex switch
            {
                1 => (TaskSortField.Priority, SortDirection.Ascending),
                2 => (TaskSortField.Alphabetical, SortDirection.Descending),
                3 => (TaskSortField.Alphabetical, SortDirection.Ascending),
                _ => (TaskSortField.Priority, SortDirection.Descending)
            };

            ApplyTaskFilters();
        }

        private void OnHideDoneChanged(object sender, CheckedChangedEventArgs e)
        {
            _hideCompleted = e.Value;
            Preferences.Default.Set(GetHideCompletedPreferenceKey(), _hideCompleted);
            ApplyTaskFilters();
        }

        private string GetHideCompletedPreferenceKey() => $"{HideCompletedPreferenceKeyPrefix}{_project.Id}";

        private void RestoreHideCompletedPreference()
        {
            _hideCompleted = Preferences.Default.Get(GetHideCompletedPreferenceKey(), false);
            chk_hide_done.IsChecked = _hideCompleted;
        }

        private async void OnManageTypesClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ManageProjectTypesPage(_projectTypeService, _projectService));
        }

        private void OnToggleSearchPanelClicked(object sender, EventArgs e)
        {
            _filtersExpanded = !_filtersExpanded;
            UpdateSearchPanelVisualState();

            if (_filtersExpanded)
            {
                task_search_bar.Focus();
            }
            else
            {
                task_search_bar.Unfocus();
            }
        }

        private void UpdateSearchPanelVisualState()
        {
            task_filters_panel.IsVisible = _filtersExpanded;
            search_toggle_button.Text = _filtersExpanded ? SearchLabelExpanded : SearchLabelCollapsed;
        }

        private IEnumerable<SubTask> SortTasks(IEnumerable<SubTask> tasks)
        {
            return (_sortField, _sortDirection) switch
            {
                (TaskSortField.Alphabetical, SortDirection.Ascending) => tasks
                    .OrderBy(task => task.Name, StringComparer.OrdinalIgnoreCase),
                (TaskSortField.Alphabetical, SortDirection.Descending) => tasks
                    .OrderByDescending(task => task.Name, StringComparer.OrdinalIgnoreCase),
                (TaskSortField.Priority, SortDirection.Ascending) => tasks
                    .OrderBy(task => task.Priority)
                    .ThenBy(task => task.Name, StringComparer.OrdinalIgnoreCase),
                _ => tasks
                    .OrderByDescending(task => task.Priority)
                    .ThenBy(task => task.Name, StringComparer.OrdinalIgnoreCase)
            };
        }

        private bool MatchesSearch(SubTask task, string[] requestedTags)
        {
            if (string.IsNullOrWhiteSpace(_searchQuery))
            {
                return true;
            }

            return _searchScope switch
            {
                SearchScope.Tags => ContainsAllTags(task, requestedTags),
                _ => ContainsText(task, _searchQuery)
            };
        }

        private static bool ContainsText(SubTask task, string query)
        {
            return (!string.IsNullOrWhiteSpace(task.Name) &&
                     task.Name.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                   (!string.IsNullOrWhiteSpace(task.Description) &&
                     task.Description.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        private static bool ContainsAllTags(SubTask task, string[] requestedTags)
        {
            if (requestedTags.Length == 0)
            {
                return true;
            }

            var taskTags = SplitTags(task.Tags);
            if (taskTags.Length == 0)
            {
                return false;
            }

            return requestedTags.All(tag => taskTags.Any(taskTag =>
                taskTag.Contains(tag, StringComparison.OrdinalIgnoreCase)));
        }

        private static string[] SplitTags(string? value) =>
            string.IsNullOrWhiteSpace(value)
                ? Array.Empty<string>()
                : value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        private void OnSetStatusNone(object? sender, EventArgs e)
        {
            if (sender is MenuFlyoutItem { CommandParameter: SubTask task })
            {
                UpdateTaskStatus(task, SubTaskStatus.None);
            }
        }

        private void OnSetStatusOngoing(object? sender, EventArgs e)
        {
            if (sender is MenuFlyoutItem { CommandParameter: SubTask task })
            {
                UpdateTaskStatus(task, SubTaskStatus.Ongoing);
            }
        }

        private void OnSetStatusDone(object? sender, EventArgs e)
        {
            if (sender is MenuFlyoutItem { CommandParameter: SubTask task })
            {
                UpdateTaskStatus(task, SubTaskStatus.Done);
            }
        }

        private void OnSetPriority(object? sender, EventArgs e)
        {
            if (sender is MenuFlyoutItem { CommandParameter: (SubTask task, int priority) })
            {
                UpdateTaskPriority(task, priority);
            }
        }

        private void UpdateTaskStatus(SubTask task, SubTaskStatus status)
        {
            task.Status = status;
            _projectService.UpdateTask(task);
            ApplyTaskFilters();
        }

        private void UpdateTaskPriority(SubTask task, int priority)
        {
            task.Priority = priority;
            _projectService.UpdateTask(task);
            ApplyTaskFilters();
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