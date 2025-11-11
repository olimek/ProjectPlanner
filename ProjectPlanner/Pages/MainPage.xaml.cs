using ProjectPlanner.Model;
using ProjectPlanner.Data.UnitOfWork;

using ProjectPlanner.ViewModels;

namespace ProjectPlanner.Pages
{
    public partial class MainPage : ContentPage
    {
        public MainPage(IUnitOfWork unitOfWork)
        {
           
            InitializeComponent();
            BindingContext = new MainPageViewModel(unitOfWork);
        }
        private async void OnProjectSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Project selectedProject)
            {
                await Navigation.PushAsync(new ProjectDetailsPage(selectedProject));
                ((CollectionView)sender).SelectedItem = null;
            }
        }

    }
}