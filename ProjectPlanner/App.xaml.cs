using ProjectPlanner.Pages;

using ProjectPlanner.Service;

namespace ProjectPlanner
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            if (Application.Current != null)
            {
                Application.Current.UserAppTheme = AppTheme.Dark;
            }
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            if (MauiProgram.Services == null)
                throw new InvalidOperationException("Application services are not available.");

            var projectService = MauiProgram.Services.GetRequiredService<IProjectService>();
            var main = new MainPage(projectService);
            return new Window(new NavigationPage(main));
        }
    }
}