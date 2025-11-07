using Microsoft.EntityFrameworkCore;
using ProjectPlanner.Data.Contexts;
using ProjectPlanner.Data.UnitOfWork;
using ProjectPlanner.Model;

namespace ProjectPlanner
{
    public partial class MainPage : ContentPage
    {
        private readonly IUnitOfWork _unitOfWork;
        private int count = 0;

        public MainPage(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            InitializeComponent();
            _unitOfWork.Project.GetAll();
        }

        private void OnCounterClicked(object? sender, EventArgs e)
        {
            count++;
            _unitOfWork.Project.Add(new Project { Name = $"{count} " });
            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }
    }
}