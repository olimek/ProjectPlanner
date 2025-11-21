using System;
using ProjectPlanner.Model;
using ProjectPlanner.Data.UnitOfWork;

namespace ProjectPlanner.Pages
{
    public partial class ProjectDetailsPage : ContentPage
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly Project _project;

        public ProjectDetailsPage(Project project, IUnitOfWork unitOfWork)
        {
            InitializeComponent();

            _project = project ?? throw new ArgumentNullException(nameof(project));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

            // TODO: użyj _unitOfWork do załadowania/zapisania danych lub przypisz BindingContext
            // BindingContext = new ProjectDetailsViewModel(_project, _unitOfWork);
        }

        private async void OnSubtaskSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is SubTask subtask)
            {
                await Navigation.PushAsync(new SubtaskDetailsPage(subtask));
                ((CollectionView)sender).SelectedItem = null;
            }
        }
    }
}