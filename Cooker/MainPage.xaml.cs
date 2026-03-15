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
        await Navigation.PushAsync(new Pages.HomePage());
    }

    async void OpenSettings_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SettingsPage());
    }
}