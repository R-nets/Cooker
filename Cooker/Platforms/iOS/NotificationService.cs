using System;
using System.Collections.Generic;
using System.Text;
using Cooker.Services;

namespace Cooker.Platforms.iOS;

public class NotificationService : INotificationService
{
    public void SendNotification(string title, string message, int seconds)
    {
        System.Diagnostics.Debug.WriteLine($"Notification: {title} - {message}");
    }
}