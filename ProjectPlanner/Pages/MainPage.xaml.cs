using Microsoft.EntityFrameworkCore;
using ProjectPlanner.Data;
using ProjectPlanner.Model;
namespace ProjectPlanner
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            ProjectContext _context = new ProjectContext();
            DbSet<Project> _dbSet = _context.Set<Project>();
            InitializeComponent();
            _dbSet.ToList();

            """
            var project = new Project(tutaj parametry konstruktora);
            _dbSet.Add(project);
            _context.SaveChanges();

            _dbSet.Remove(entity);
            """
        }

        private void OnCounterClicked(object? sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }
    }
}