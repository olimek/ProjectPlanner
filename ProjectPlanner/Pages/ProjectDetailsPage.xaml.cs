using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
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
        private bool _isSwipeAction;
        private const string SearchLabelCollapsed = "SEARCH";
        private const string SearchLabelExpanded = "CLOSE";
        private readonly Picker _searchScopePicker;
        private readonly Picker _sortFieldPicker;
        private const string HideCompletedPreferenceKeyPrefix = "ProjectPage.HideCompleted.";

        public ICommand TapCommand { get; }
        public ICommand LongPressCommand { get; }

        // Status commands for SwipeView
        public ICommand SetStatusNoneCommand { get; }
        public ICommand SetStatusOngoingCommand { get; }
        public ICommand SetStatusDoneCommand { get; }

        // Priority commands for SwipeView
        public ICommand SetPriority0Command { get; }
        public ICommand SetPriority1Command { get; }
        public ICommand SetPriority2Command { get; }
        public ICommand SetPriority3Command { get; }
        public ICommand SetPriority4Command { get; }
        public ICommand SetPriority5Command { get; }

        public ProjectPage(Project project, IProjectService projectService)
        {
            InitializeComponent();
            _project = project;
            _projectService = projectService;
            _projectTypeService = MauiProgram.Services?.GetService<IProjectTypeService>()
                ?? throw new InvalidOperationException("ProjectTypeService not available");

            TapCommand = new AsyncRelayCommand<SubTask>(OnTaskTappedAsync);
            LongPressCommand = new AsyncRelayCommand<SubTask>(OnTaskLongPressedAsync);

            // Initialize status commands
            SetStatusNoneCommand = new RelayCommand<SubTask>(task => UpdateTaskStatus(task!, SubTaskStatus.None));
            SetStatusOngoingCommand = new RelayCommand<SubTask>(task => UpdateTaskStatus(task!, SubTaskStatus.Ongoing));
            SetStatusDoneCommand = new RelayCommand<SubTask>(task => UpdateTaskStatus(task!, SubTaskStatus.Done));

            // Initialize priority commands
            SetPriority0Command = new RelayCommand<SubTask>(task => UpdateTaskPriority(task!, 0));
            SetPriority1Command = new RelayCommand<SubTask>(task => UpdateTaskPriority(task!, 1));
            SetPriority2Command = new RelayCommand<SubTask>(task => UpdateTaskPriority(task!, 2));
            SetPriority3Command = new RelayCommand<SubTask>(task => UpdateTaskPriority(task!, 3));
            SetPriority4Command = new RelayCommand<SubTask>(task => UpdateTaskPriority(task!, 4));
            SetPriority5Command = new RelayCommand<SubTask>(task => UpdateTaskPriority(task!, 5));

            BindingContext = this;

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

        private async Task OnTaskTappedAsync(SubTask? task)
        {
            if (task == null) return;

            var fullTask = _allTasks.FirstOrDefault(t => t.Id == task.Id) ?? task;
            await Navigation.PushAsync(new SubtaskDetailsPage(fullTask, _projectService));
        }

        private async void OnTaskSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isSwipeAction)
            {
                ((CollectionView)sender).SelectedItem = null;
                return;
            }

            if (e.CurrentSelection.FirstOrDefault() is not SubTask task)
                return;

            // Clear selection to allow reselection
            ((CollectionView)sender).SelectedItem = null;

            await OnTaskTappedAsync(task);
        }

        private async Task OnTaskLongPressedAsync(SubTask? task)
        {
            if (task == null) return;

            await ShowTaskActionSheet(task);
        }

        private async void OnTaskSwiped(object sender, SwipedEventArgs e)
        {
            if (sender is not BindableObject { BindingContext: SubTask task })
            {
                return;
            }

            _isSwipeAction = true;
            await ShowTaskActionSheet(task);
            _isSwipeAction = false;
        }

        private async Task ShowTaskActionSheet(SubTask task)
        {
            var action = await DisplayActionSheet(
                $"⚡ {task.Name}",
                "CANCEL",
                null,
                "── STATUS ──",
                "   ○ NONE",
                "   ► ONGOING",
                "   ● DONE",
                "── PRIORITY ──",
                "   [0] NONE",
                "   [1] LOW",
                "   [2] MEDIUM",
                "   [3] HIGH",
                "   [4] URGENT",
                "   [5] CRITICAL");

            if (string.IsNullOrEmpty(action) || action == "CANCEL" || action.StartsWith("──"))
                return;

            var trimmedAction = action.Trim();

            switch (trimmedAction)
            {
                case "○ NONE":
                    UpdateTaskStatus(task, SubTaskStatus.None);
                    break;
                case "► ONGOING":
                    UpdateTaskStatus(task, SubTaskStatus.Ongoing);
                    break;
                case "● DONE":
                    UpdateTaskStatus(task, SubTaskStatus.Done);
                    break;
                case "[0] NONE":
                    UpdateTaskPriority(task, 0);
                    break;
                case "[1] LOW":
                    UpdateTaskPriority(task, 1);
                    break;
                case "[2] MEDIUM":
                    UpdateTaskPriority(task, 2);
                    break;
                case "[3] HIGH":
                    UpdateTaskPriority(task, 3);
                    break;
                case "[4] URGENT":
                    UpdateTaskPriority(task, 4);
                    break;
                case "[5] CRITICAL":
                    UpdateTaskPriority(task, 5);
                    break;
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