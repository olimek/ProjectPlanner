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
                    fonts.AddFont("ChakraPetch-Bold.ttf", "HeaderFont");
                    fonts.AddFont("ChakraPetch-Regular.ttf", "TechFont");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif
            builder.Services.AddDbContext<ProjectContext>();
            builder.Services.AddTransient<ProjectPage>();
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

            var app = builder.Build();

            Services = app.Services;

            return app;
        }
    }
}