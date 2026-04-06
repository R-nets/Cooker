using System.Linq;
using Microsoft.Maui.Storage;

namespace Cooker.Pages;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();

        NotificationSwitch.IsToggled =
            Preferences.Default.Get("notifications", true);

        VibrationSwitch.IsToggled =
            Preferences.Default.Get("vibration", true);

        CameraSwitch.IsToggled =
            Preferences.Default.Get("camera", true);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        App.ApplyBackgroundToPage(this);

        BackgroundOverlay.IsVisible =
            Preferences.Default.ContainsKey("background_image");
        ThemePicker.SelectedIndex =
            Preferences.Default.Get("theme", 0);
    }

    void NotificationChanged(object sender, ToggledEventArgs e)
    {
        Preferences.Default.Set("notifications", e.Value);
    }

    void VibrationChanged(object sender, ToggledEventArgs e)
    {
        Preferences.Default.Set("vibration", e.Value);
    }

    void CameraChanged(object sender, ToggledEventArgs e)
    {
        Preferences.Default.Set("camera", e.Value);
    }

    void ThemePicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        Preferences.Default.Set("theme", ThemePicker.SelectedIndex);

        Application.Current!.UserAppTheme = ThemePicker.SelectedIndex switch
        {
            0 => AppTheme.Light,
            1 => AppTheme.Dark,
            _ => AppTheme.Light
        };
    }

    async void ChooseBackground_Clicked(object sender, EventArgs e)
    {
        string action = await DisplayActionSheetAsync(
            "Select Image Source",
            "Cancel",
            null,
            "Camera",
            "Gallery");

        FileResult? photo = null;

        if (action == "Camera")
        {
            if (!Preferences.Default.Get("camera", true))
            {
                await DisplayAlertAsync("Permission Disabled",
                    "Enable camera permission in settings first.",
                    "OK");
                return;
            }

            await DishDetailsPage.PermissionHelper
                .RequestWithExplanation<Permissions.Camera>(
                    "Camera access is needed to take photos.");

            photo = await MediaPicker.Default.CapturePhotoAsync();
        }
        else if (action == "Gallery")
        {
            await DishDetailsPage.PermissionHelper
                .RequestWithExplanation<Permissions.Photos>(
                    "Photo access is needed to select images.");

            var photos = await MediaPicker.Default.PickPhotosAsync();
            photo = photos?.FirstOrDefault();
        }

        if (photo == null)
            return;

        string path = Path.Combine(FileSystem.AppDataDirectory, photo.FileName);

        await using var sourceStream = await photo.OpenReadAsync();
        await using var localStream = File.OpenWrite(path);
        await sourceStream.CopyToAsync(localStream);

        Preferences.Default.Set("background_image", path);

        await DisplayAlertAsync("Background Updated", "Applied successfully", "OK");

        RefreshAllPagesBackground();
    }

    async void ResetBackground_Clicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlertAsync(
            "Reset Background",
            "Remove custom background?",
            "Yes",
            "No");

        if (!confirm)
            return;

        Preferences.Default.Remove("background_image");

        RefreshAllPagesBackground();
    }

    static void RefreshAllPagesBackground()
    {
        var window = Application.Current?.Windows.FirstOrDefault();
        var page = window?.Page;

        if (page != null)
        {
            App.ApplyBackgroundToPage(page);
        }
    }
}