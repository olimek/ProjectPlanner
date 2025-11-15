using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProjectPlanner.Data.Contexts;
using ProjectPlanner.Data.UnitOfWork;
using ProjectPlanner.Service;
using ProjectPlanner.Pages;

namespace ProjectPlanner
{
    public static class MauiProgram
    {
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
            builder.Services.AddScoped<ProjectPage>();
            builder.Services.AddScoped<IProjectService, ProjectService>();

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
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
            return builder.Build();
        }
    }
}