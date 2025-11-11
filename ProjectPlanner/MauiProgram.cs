using Microsoft.Extensions.Logging;
using ProjectPlanner.Data.Contexts;
using ProjectPlanner.Data.UnitOfWork;
using ProjectPlanner.WinUI;

namespace ProjectPlanner
{

    public static class MauiProgram
    {
        public static IServiceProvider Services { get; private set; }
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
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            Services = builder.Build().Services;
            return builder.Build();
        }
    }
}