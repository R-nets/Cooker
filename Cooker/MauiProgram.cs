using CommunityToolkit.Maui;
using Cooker.Services;
using Microsoft.Extensions.Logging;

#if ANDROID
using Cooker.Platforms.Android;
#endif

namespace Cooker;

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
            });


#if ANDROID
        builder.Services.AddSingleton<INotificationService, NotificationService>();
#else
        builder.Services.AddSingleton<INotificationService, DummyNotificationService>();
#endif

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}