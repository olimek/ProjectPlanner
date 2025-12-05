using ProjectPlanner.Pages;
using Microsoft.Extensions.DependencyInjection;
using ProjectPlanner.Service;

namespace ProjectPlanner
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            Application.Current.UserAppTheme = AppTheme.Dark;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Resolve IProjectService from the DI container and start app at MainPage
            if (MauiProgram.Services == null)
                throw new InvalidOperationException("Application services are not available.");

            var projectService = MauiProgram.Services.GetRequiredService<IProjectService>();
            var main = new MainPage(projectService);
            return new Window(main);
        }
    }
}