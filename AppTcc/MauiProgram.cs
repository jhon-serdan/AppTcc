using AppTcc.Helper;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microcharts.Maui;


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
                .UseMicrocharts()
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

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
