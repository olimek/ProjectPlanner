using ProjectPlanner.Model;
using ProjectPlanner.Service;

namespace ProjectPlanner.Pages
{
    public partial class SubtaskDetailsPage : ContentPage
    {
        private readonly IProjectService? _projectService;

        private SubTask _subtask;
        private bool _isHandlingToggle = false;

        public SubtaskDetailsPage()
        {
            InitializeComponent();
            _subtask = new SubTask();
            _projectService = null;
        }

        public SubtaskDetailsPage(SubTask subtask, IProjectService projectService)
        {
            InitializeComponent();
            _subtask = subtask;
            _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));

            //BindingContext = _subtask;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ReloadPage();
        }

        private void ReloadPage()
        {
            if (_projectService == null || _subtask == null || _subtask.Id == 0) return;

            var freshTasks = _projectService.GetTasksForProject((int)_subtask.ProjectId);
            var refreshedTask = freshTasks?.FirstOrDefault(t => t.Id == _subtask.Id);

            if (refreshedTask != null)
            {
                _subtask = refreshedTask;

                BindingContext = _subtask;
            }
        }

        private void OnStatusToggled(object sender, ToggledEventArgs e)
        {
            if (_isHandlingToggle) return;

            if (_projectService == null) return;

            _isHandlingToggle = true;

            try
            {
                _subtask.IsDone = e.Value;
                _projectService.UpdateTask(_subtask);

                ReloadPage();
            }
            finally
            {
                _isHandlingToggle = false;
            }
        }

        private async void OnEditClicked(object sender, EventArgs e)
        {
            if (_projectService == null) return;

            var parentProject = _projectService.GetProjectByID((int)_subtask.ProjectId);

            if (parentProject != null)
            {
                await Navigation.PushAsync(new AddOrEditTask(parentProject, _projectService, _subtask.Id));
            }
            else
            {
                await DisplayAlert("Błąd", "Nie znaleziono projektu nadrzędnego.", "OK");
            }
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            if (_projectService == null) return;

            bool confirm = await DisplayAlert("USUWANIE",
                "Czy na pewno usunąć to zadanie?",
                "USUŃ", "ANULUJ");

            if (confirm)
            {
                _projectService.DeleteTask(_subtask);
                await Navigation.PopAsync();
            }
        }
    }
}