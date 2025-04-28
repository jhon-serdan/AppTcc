using AppTcc.Helper;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace AppTcc
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("Lato-Black.ttf", "LatoBlack");
                    fonts.AddFont("Lato-BlackItalic.tft", "LatoBlackItalic");
                    fonts.AddFont("Lato-Bold.ttf", "LatoBold");
                    fonts.AddFont("Lato-BoldItalic.ttf", "LatoBoldItalic");
                    fonts.AddFont("Lato-Italic.ttf", "LatoItalic");
                    fonts.AddFont("Lato-Light.ttf", "LatoLight");
                    fonts.AddFont("Lato-LightItalic.ttf", "LatoLightItalic");
                    fonts.AddFont("Lato-Regular.ttf", "LatoRegular");
                    fonts.AddFont("Lato-Thin.ttf", "LatoThin");
                });

            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "transacoes.db3");

            builder.Services.AddSingleton<SQLiteDatabaseHelper>(s => new SQLiteDatabaseHelper(dbPath));

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
