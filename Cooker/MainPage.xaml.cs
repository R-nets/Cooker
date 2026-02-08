using Cooker.Pages;

namespace Cooker;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private void OpenHomeBtn_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new HomePage());
    }
}
