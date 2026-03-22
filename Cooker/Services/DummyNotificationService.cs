using System;
using System.Collections.Generic;
using System.Text;

using System.Diagnostics;

namespace Cooker.Services;

public class DummyNotificationService : INotificationService
{
    public void SendNotification(string title, string message, int seconds)
    {
        Debug.WriteLine($"[Notification] {title}: {message}");
    }
}