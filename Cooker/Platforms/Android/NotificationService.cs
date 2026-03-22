using Android.App;
using Android.Content;
using Android.OS;
using Cooker.Services;
using System.Runtime.Versioning;

namespace Cooker.Platforms.Android;

[SupportedOSPlatform("android")]
public class NotificationService : INotificationService
{
    const string CHANNEL_ID = "cooker_timer_channel";

    public void SendNotification(string title, string message, int seconds)
    {
        if (!OperatingSystem.IsAndroid())
            return;

        var context = global::Android.App.Application.Context;

        var notificationManager =
            (NotificationManager?)context.GetSystemService(Context.NotificationService);

        if (notificationManager == null)
            return;

        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var channel = new NotificationChannel(
                CHANNEL_ID,
                "Cooking Timer",
                NotificationImportance.Default);

            notificationManager.CreateNotificationChannel(channel);
        }

        var intent = new Intent(context, typeof(MainActivity));

        var pendingIntent = PendingIntent.GetActivity(
            context, 0, intent, PendingIntentFlags.Immutable);

        var builder = new Notification.Builder(context)
            .SetContentTitle(title)
            .SetContentText(message)
            .SetSmallIcon(Resource.Drawable.appicon)
            .SetContentIntent(pendingIntent);

        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            builder.SetChannelId(CHANNEL_ID);

        notificationManager.Notify(0, builder.Build());
    }
}