using ProjectPlanner.Data.UnitOfWork;

namespace ProjectPlanner
{
    public partial class MainPage : ContentPage
    {
        public MainPage(IUnitOfWork unitOfWork)
        {
            InitializeComponent();
        }
    }
}