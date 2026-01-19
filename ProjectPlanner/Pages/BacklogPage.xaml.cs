using System.Collections.Generic;
using System.Linq;
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

        public BacklogPage(IProjectService projectService)
        {
            InitializeComponent();
            _projectService = projectService;

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

        private async void OnTaskSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.Count > 0 && e.CurrentSelection[0] is SubTask selected)
            {
                // Ensure we navigate with the instance from the aggregated list (contains Project reference)
                var task = _allTasks.FirstOrDefault(t => t.Id == selected.Id) ?? selected;
                await Navigation.PushAsync(new SubtaskDetailsPage(task, _projectService));
            }

            if (sender is CollectionView cv)
            {
                cv.SelectedItem = null;
            }
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
