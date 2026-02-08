namespace Cooker.Pages;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();
    }

    private async void Cuisine_Clicked(object sender, EventArgs e)
    {
        Button btn = (Button)sender;

        await DisplayAlertAsync(
            "Cuisine Selected",
            $"You selected {btn.Text}",
            "OK");
    }
}
