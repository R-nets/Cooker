using Cooker.Pages;

namespace Cooker;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void OpenHomeBtn_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(
    new Pages.HomePage(
        ((App)Application.Current!).notificationService
    )
);
    }

    async void OpenSettings_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SettingsPage());
    }
}