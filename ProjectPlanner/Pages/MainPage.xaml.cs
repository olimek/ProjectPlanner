using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Controls;
using ProjectPlanner.Model;
using ProjectPlanner.Model.Messaging;
using ProjectPlanner.Service;

namespace ProjectPlanner.Pages;

public partial class MainPage : ContentPage
{
    private readonly IProjectService _projectService;

    public List<Project> Projects { get; set; } = new();

    private List<Project> _allProjects = new();
    private string _projectSearchQuery = string.Empty;
    private ProjectSortField _projectSortField = ProjectSortField.Name;
    private SortDirection _projectSortDirection = SortDirection.Ascending;
    private bool _projectFiltersExpanded;
    private bool _suppressFilterUpdates;
    private bool _isMessengerRegistered;
    private Picker? _projectSortPicker;
    private Button? _projectSearchToggleButton;
    private Border? _projectSearchPanel;
    private SearchBar? _projectSearchBar;
    private Button? _addProjectButton;

    private const string ProjectSearchLabelCollapsed = "SEARCH";
    private const string ProjectSearchLabelExpanded = "CLOSE";

    public MainPage(IProjectService projectService)
    {
        InitializeComponent();
        _projectService = projectService;

        BindingContext = this;

        _projectSortPicker = this.FindByName<Picker>("ProjectSortPicker");
        _projectSearchToggleButton = this.FindByName<Button>("ProjectSearchToggleButton");
        _projectSearchPanel = this.FindByName<Border>("ProjectSearchPanel");
        _projectSearchBar = this.FindByName<SearchBar>("ProjectSearchBar");
        _addProjectButton = this.FindByName<Button>("AddProjectBtn");

        _suppressFilterUpdates = true;
        if (_projectSortPicker != null)
            _projectSortPicker.SelectedIndex = 0;
        _suppressFilterUpdates = false;
        UpdateProjectSearchPanelVisualState();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        RegisterForProjectUpdates();
        LoadProjects();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        UnregisterFromProjectUpdates();
    }

    private void RegisterForProjectUpdates()
    {
        if (_isMessengerRegistered)
            return;

        WeakReferenceMessenger.Default.Register<ProjectsUpdatedMessage>(this, OnProjectsUpdated);
        _isMessengerRegistered = true;
    }

    private void UnregisterFromProjectUpdates()
    {
        if (!_isMessengerRegistered)
            return;

        WeakReferenceMessenger.Default.Unregister<ProjectsUpdatedMessage>(this);
        _isMessengerRegistered = false;
    }

    private void OnProjectsUpdated(object recipient, ProjectsUpdatedMessage message)
    {
        MainThread.BeginInvokeOnMainThread(LoadProjects);
    }

    private void LoadProjects()
    {
        _allProjects = _projectService.GetAllProjects();
        UpdateGlobalActionsVisibility();
        ApplyProjectFilters();
    }

    private void ApplyProjectFilters()
    {
        if (_suppressFilterUpdates)
            return;

        IEnumerable<Project> filtered = _allProjects ?? Enumerable.Empty<Project>();

        if (!string.IsNullOrWhiteSpace(_projectSearchQuery))
        {
            var query = _projectSearchQuery;
            filtered = filtered.Where(p =>
                (!string.IsNullOrWhiteSpace(p.Name) &&
                 p.Name.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrWhiteSpace(p.Description) &&
                 p.Description.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrWhiteSpace(p.Type?.Name) &&
                 p.Type!.Name.Contains(query, StringComparison.OrdinalIgnoreCase)));
        }

        filtered = (_projectSortField, _projectSortDirection) switch
        {
            (ProjectSortField.Name, SortDirection.Descending) => filtered
                .OrderByDescending(p => p.Name, StringComparer.OrdinalIgnoreCase),
            (ProjectSortField.Category, SortDirection.Ascending) => filtered
                .OrderBy(p => p.Type?.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ThenBy(p => p.Name, StringComparer.OrdinalIgnoreCase),
            (ProjectSortField.Category, SortDirection.Descending) => filtered
                .OrderByDescending(p => p.Type?.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ThenBy(p => p.Name, StringComparer.OrdinalIgnoreCase),
            (ProjectSortField.Progress, SortDirection.Descending) => filtered
                .OrderByDescending(p => p.Progress)
                .ThenBy(p => p.Name, StringComparer.OrdinalIgnoreCase),
            (ProjectSortField.Progress, SortDirection.Ascending) => filtered
                .OrderBy(p => p.Progress)
                .ThenBy(p => p.Name, StringComparer.OrdinalIgnoreCase),
            _ => filtered
                .OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
        };

        Projects = filtered.ToList();
        ProjectsList.ItemsSource = Projects;
    }

    private void UpdateProjectSearchPanelVisualState()
    {
        if (_projectSearchPanel != null)
            _projectSearchPanel.IsVisible = _projectFiltersExpanded;

        if (_projectSearchToggleButton != null)
            _projectSearchToggleButton.Text = _projectFiltersExpanded ? ProjectSearchLabelExpanded : ProjectSearchLabelCollapsed;
    }

    private void UpdateGlobalActionsVisibility()
    {
        var hasProjects = _allProjects?.Any() == true;

        if (!hasProjects && _projectFiltersExpanded)
        {
            _projectFiltersExpanded = false;
            _projectSearchQuery = string.Empty;
            if (_projectSearchBar != null)
            {
                _projectSearchBar.TextChanged -= OnProjectSearchTextChanged;
                _projectSearchBar.Text = string.Empty;
                _projectSearchBar.TextChanged += OnProjectSearchTextChanged;
            }
        }

        if (_projectSearchToggleButton != null)
            _projectSearchToggleButton.IsVisible = hasProjects;

        if (_addProjectButton != null)
            _addProjectButton.IsVisible = hasProjects;

        UpdateProjectSearchPanelVisualState();
    }

    private void OnProjectToggleSearchPanelClicked(object sender, EventArgs e)
    {
        _projectFiltersExpanded = !_projectFiltersExpanded;
        UpdateProjectSearchPanelVisualState();

        if (_projectFiltersExpanded)
        {
            _projectSearchBar?.Focus();
        }
        else
        {
            _projectSearchBar?.Unfocus();
        }
    }

    private void OnProjectSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        _projectSearchQuery = e.NewTextValue?.Trim() ?? string.Empty;
        ApplyProjectFilters();
    }

    private void OnProjectSortChanged(object sender, EventArgs e)
    {
        if (_suppressFilterUpdates)
            return;

        var selectedSortIndex = _projectSortPicker?.SelectedIndex ?? 0;

        (_projectSortField, _projectSortDirection) = selectedSortIndex switch
        {
            1 => (ProjectSortField.Name, SortDirection.Descending),
            2 => (ProjectSortField.Category, SortDirection.Ascending),
            3 => (ProjectSortField.Category, SortDirection.Descending),
            4 => (ProjectSortField.Progress, SortDirection.Descending),
            5 => (ProjectSortField.Progress, SortDirection.Ascending),
            _ => (ProjectSortField.Name, SortDirection.Ascending)
        };

        ApplyProjectFilters();
    }

    private async void AddProjectBtn_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AddOrEditProject(null, _projectService));
    }

    private async void ProjectsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Project selected)
        {
            await Navigation.PushAsync(new ProjectPage(selected, _projectService));
            ((CollectionView)sender).SelectedItem = null;
        }
    }

    private async void OnBacklogClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new BacklogPage(_projectService));
    }

    private enum ProjectSortField
    {
        Name = 0,
        Category = 1,
        Progress = 2
    }

    private enum SortDirection
    {
        Ascending = 0,
        Descending = 1
    }
}