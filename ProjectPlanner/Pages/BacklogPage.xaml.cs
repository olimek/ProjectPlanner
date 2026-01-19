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
    public partial class BacklogPage : ContentPage
    {
        private readonly IProjectService _projectService;
        private List<Project> _projects = [];
        private List<SubTask> _allTasks = [];
        private string _searchQuery = string.Empty;
        private SortOption _sortOption = SortOption.PriorityDescending;
        private bool _hideCompleted;
        private bool _onlyInProgress;
        private bool _filtersExpanded;
        private const string FiltersCollapsedText = "FILTERS";
        private const string FiltersExpandedText = "CLOSE";
        private const string HideCompletedPreferenceKey = "BacklogPage.HideCompleted";
        private const string OnlyInProgressPreferenceKey = "BacklogPage.OnlyInProgress";

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

        public BacklogPage(IProjectService projectService)
        {
            InitializeComponent();
            _projectService = projectService;

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

            SortPicker.SelectedIndex = 0;
            RestorePreferences();
            UpdateFilterPanelVisualState();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadTasks();
        }


        private void LoadTasks()
        {
            _projects = _projectService.GetAllProjects() ?? [];

            var aggregated = new List<SubTask>();
            foreach (var project in _projects)
            {
                if (project.Tasks == null)
                    continue;


                foreach (var task in project.Tasks)
                {
                    task.Project ??= project;
                    aggregated.Add(task);
                }
            }

            _allTasks = aggregated;
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            IEnumerable<SubTask> filtered = _allTasks ?? Enumerable.Empty<SubTask>();

            if (!string.IsNullOrWhiteSpace(_searchQuery))
            {
                filtered = filtered.Where(task =>
                    (!string.IsNullOrWhiteSpace(task.Name) &&
                     task.Name.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(task.Description) &&
                     task.Description.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(task.Tags) &&
                     task.Tags.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase)));
            }

            if (_onlyInProgress)
            {
                filtered = filtered.Where(task => task.Status == SubTaskStatus.Ongoing);
            }
            else if (_hideCompleted)
            {
                filtered = filtered.Where(task => task.Status != SubTaskStatus.Done);
            }

            filtered = _sortOption switch
            {
                SortOption.PriorityAscending => filtered
                    .OrderBy(task => task.Priority)
                    .ThenBy(task => task.Name, StringComparer.OrdinalIgnoreCase),
                SortOption.NameAscending => filtered
                    .OrderBy(task => task.Name, StringComparer.OrdinalIgnoreCase),
                SortOption.NameDescending => filtered
                    .OrderByDescending(task => task.Name, StringComparer.OrdinalIgnoreCase),
                SortOption.ProjectAscending => filtered
                    .OrderBy(task => task.Project?.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(task => task.Name, StringComparer.OrdinalIgnoreCase),
                _ => filtered
                    .OrderByDescending(task => task.Priority)
                    .ThenBy(task => task.Name, StringComparer.OrdinalIgnoreCase)
            };

            TasksList.ItemsSource = filtered.ToList();
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            _searchQuery = e.NewTextValue?.Trim() ?? string.Empty;
            ApplyFilters();
        }

        private void OnSortChanged(object sender, EventArgs e)
        {
            _sortOption = SortPicker.SelectedIndex switch
            {
                1 => SortOption.PriorityAscending,
                2 => SortOption.NameAscending,
                3 => SortOption.NameDescending,
                4 => SortOption.ProjectAscending,
                _ => SortOption.PriorityDescending
            };

            ApplyFilters();
        }

        private void OnHideCompletedChanged(object sender, CheckedChangedEventArgs e)
        {
            _hideCompleted = e.Value;
            Preferences.Default.Set(HideCompletedPreferenceKey, _hideCompleted);
            
            // If "Only In Progress" is checked and we check "Hide Completed", uncheck "Only In Progress"
            if (_hideCompleted && _onlyInProgress)
            {
                _onlyInProgress = false;
                OnlyInProgressCheckbox.IsChecked = false;
                Preferences.Default.Set(OnlyInProgressPreferenceKey, false);
            }
            
            ApplyFilters();
        }

        private void OnOnlyInProgressChanged(object sender, CheckedChangedEventArgs e)
        {
            _onlyInProgress = e.Value;
            Preferences.Default.Set(OnlyInProgressPreferenceKey, _onlyInProgress);
            
            // If "Only In Progress" is checked, uncheck "Hide Completed" as it's more restrictive
            if (_onlyInProgress && _hideCompleted)
            {
                _hideCompleted = false;
                HideCompletedCheckbox.IsChecked = false;
                Preferences.Default.Set(HideCompletedPreferenceKey, false);
            }
            
            ApplyFilters();
        }

        private void OnToggleFiltersClicked(object sender, EventArgs e)
        {
            _filtersExpanded = !_filtersExpanded;
            UpdateFilterPanelVisualState();

            if (_filtersExpanded)
            {
                BacklogSearchBar.Focus();
            }
            else
            {
                BacklogSearchBar.Unfocus();
            }
        }

        private void UpdateFilterPanelVisualState()
        {
            FiltersPanel.IsVisible = _filtersExpanded;
            FiltersToggleButton.Text = _filtersExpanded ? FiltersExpandedText : FiltersCollapsedText;
        }

        private void RestorePreferences()
        {
            _hideCompleted = Preferences.Default.Get(HideCompletedPreferenceKey, false);
            HideCompletedCheckbox.IsChecked = _hideCompleted;
            
            _onlyInProgress = Preferences.Default.Get(OnlyInProgressPreferenceKey, false);
            OnlyInProgressCheckbox.IsChecked = _onlyInProgress;
        }

        private async Task OnTaskTappedAsync(SubTask? task)
        {
            if (task == null) return;

            var fullTask = _allTasks.FirstOrDefault(t => t.Id == task.Id) ?? task;
            await Navigation.PushAsync(new SubtaskDetailsPage(fullTask, _projectService));
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

            await ShowTaskActionSheet(task);
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
            ApplyFilters();
        }

        private void UpdateTaskPriority(SubTask task, int priority)
        {
            task.Priority = priority;
            _projectService.UpdateTask(task);
            ApplyFilters();
        }

        private enum SortOption
        {
            PriorityDescending = 0,
            PriorityAscending = 1,
            NameAscending = 2,
            NameDescending = 3,
            ProjectAscending = 4
        }
    }
}
