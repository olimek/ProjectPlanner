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

// Debug logging provider is not required here. If needed, add logging providers via builder.Logging.AddProvider(...)
builder.Services.AddDbContext<ProjectContext>();
builder.Services.AddTransient<ProjectPage>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IProjectTypeService, ProjectTypeService>();
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