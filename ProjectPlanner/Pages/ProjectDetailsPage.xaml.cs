using ProjectPlanner.Model;
using ProjectPlanner.ViewModels;
using ProjectPlanner.Data.UnitOfWork;

namespace ProjectPlanner.Pages
{
    public partial class ProjectDetailsPage : ContentPage
    {
        public ProjectDetailsPage(Project project)
        {
            InitializeComponent();
            var unitOfWork = MauiProgram.Services.GetService<IUnitOfWork>(); // jeśli używasz DI
            BindingContext = new ProjectDetailsViewModel(project, unitOfWork);
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
