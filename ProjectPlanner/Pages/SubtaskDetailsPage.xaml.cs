using ProjectPlanner.Model;

namespace ProjectPlanner.Pages
{
    public partial class SubtaskDetailsPage : ContentPage
    {
        public SubtaskDetailsPage(SubTask subtask)
        {
            InitializeComponent();
            BindingContext = subtask;
        }
    }
}