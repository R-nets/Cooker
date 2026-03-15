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
    }

    void NotificationChanged(object sender, ToggledEventArgs e)
    {
        Preferences.Default.Set("notifications", e.Value);
    }

    void VibrationChanged(object sender, ToggledEventArgs e)
    {
        Preferences.Default.Set("vibration", e.Value);
    }
}