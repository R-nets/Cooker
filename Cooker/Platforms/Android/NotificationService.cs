using Android.App;
using Android.Content;
using Android.OS;
using Cooker.Services;
using System.Runtime.Versioning;

namespace Cooker.Platforms.Android;

#pragma warning disable CA1416
#pragma warning disable CA1422

[SupportedOSPlatform("android")]
public class NotificationService : INotificationService
{
    const string CHANNEL_ID = "cooker_timer_channel";

    [SupportedOSPlatform("android")]
    public void SendNotification(string title, string message, int seconds)
    {
        var context = global::Android.App.Application.Context;


        if (context.GetSystemService(Context.NotificationService) is not NotificationManager notificationManager)
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
            context,
            0,
            intent,
            PendingIntentFlags.Immutable);

        var builder = new Notification.Builder(context)
            .SetContentTitle(title)
            .SetContentText(message)
            .SetSmallIcon(global::Android.Resource.Drawable.IcMenuInfoDetails)
            .SetContentIntent(pendingIntent);

        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            builder.SetChannelId(CHANNEL_ID);
        }

        notificationManager.Notify(0, builder.Build());
    }
}
#pragma warning restore CA1416
#pragma warning restore CA1422