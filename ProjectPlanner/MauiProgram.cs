using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProjectPlanner.Data.Contexts;
using ProjectPlanner.Data.UnitOfWork;
using ProjectPlanner.Pages;
using ProjectPlanner.Service;

namespace ProjectPlanner
{
    public static class MauiProgram
    {
        // Expose the built service provider so other code can access IServiceProvider via MauiProgram.Services
        public static IServiceProvider? Services { get; private set; }

        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif
            builder.Services.AddDbContext<ProjectContext>();
            builder.Services.AddTransient<ProjectPage>();
            builder.Services.AddScoped<IProjectService, ProjectService>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            // perform migrations using a temporary provider scope
            using (var scope = builder.Services.BuildServiceProvider().CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ProjectContext>();

                try
                {
                    db.Database.Migrate();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Database initialization error: {ex.Message}");
                }
            }

            var app = builder.Build();

            // assign the built service provider so callers can access DI container: MauiProgram.Services
            Services = app.Services;

            return app;
        }
    }
}