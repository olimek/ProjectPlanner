
using ProjectPlanner.Data.UnitOfWork;

using ProjectPlanner.ViewModels;

namespace ProjectPlanner
{
    public partial class MainPage : ContentPage
    {
        public MainPage(IUnitOfWork unitOfWork)
        {
           
            InitializeComponent();
            BindingContext = new MainPageViewModel(unitOfWork);
        }

    }
}