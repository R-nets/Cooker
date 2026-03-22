using System;
using System.Collections.Generic;
using System.Text;

namespace Cooker.Services;

public interface INotificationService
{
    void SendNotification(string title, string message, int seconds);
}