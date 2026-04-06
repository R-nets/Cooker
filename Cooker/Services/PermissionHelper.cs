using Microsoft.Maui.ApplicationModel;

namespace Cooker.Services;

public static class PermissionHelper
{
    public static async Task RequestWithExplanation<T>(string message)
        where T : Permissions.BasePermission, new()
    {
        var status = await Permissions.CheckStatusAsync<T>();

        if (status != PermissionStatus.Granted)
        {
            var page = Application.Current?.Windows[0].Page;

            if (page != null)
            {
                await page.DisplayAlertAsync(
                    "Permission Required",
                    message,
                    "OK");
            }

            await Permissions.RequestAsync<T>();
        }
    }
}