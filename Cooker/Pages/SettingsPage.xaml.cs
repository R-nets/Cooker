namespace Cooker.Pages;
using Microsoft.Maui.Storage;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();

        NotificationSwitch.IsToggled =
            Preferences.Default.Get("notifications", true);

        VibrationSwitch.IsToggled =
            Preferences.Default.Get("vibration", true);
    }

    void NotificationChanged(object sender, ToggledEventArgs e)
    {
        Preferences.Default.Set("notifications", e.Value);
    }

    void VibrationChanged(object sender, ToggledEventArgs e)
    {
        Preferences.Default.Set("vibration", e.Value);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        ThemePicker.SelectedIndex = Preferences.Default.Get("theme", 2);
    }

    void ThemePicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        Preferences.Default.Set("theme", ThemePicker.SelectedIndex);

        if (Application.Current == null)
            return;

        Application.Current.UserAppTheme = ThemePicker.SelectedIndex switch
        {
            0 => AppTheme.Light,
            1 => AppTheme.Dark,
            _ => AppTheme.Unspecified
        };
    }

    async void ChooseBackground_Clicked(object sender, EventArgs e)
    {
        if (ThemePicker.SelectedIndex != 2)
        {
            await DisplayAlertAsync(
                "custom Mode Only",
                "Background images can only be used in System mode.",
                "OK");
            return;
        }

        var photo = await MediaPicker.Default.PickPhotoAsync();

        if (photo == null)
            return;

        string path = Path.Combine(FileSystem.AppDataDirectory, photo.FileName);

        await using var sourceStream = await photo.OpenReadAsync();
        await using var localStream = File.OpenWrite(path);

        await sourceStream.CopyToAsync(localStream);

        Preferences.Default.Set("background_image", path);

        await DisplayAlertAsync(
            "Background Updated",
            "Custom background image saved.",
            "OK");
    }
}