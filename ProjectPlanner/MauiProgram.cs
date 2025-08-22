using Microsoft.Extensions.Logging;
using ProjectPlanner.Database;

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
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "projectsplanner.db3");

            builder.Services.AddSingleton(sp =>
                DatabaseService.CreateAsync(dbPath).Result
            );

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}