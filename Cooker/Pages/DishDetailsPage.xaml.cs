using Cooker.Models;
using Cooker.Services;
using CommunityToolkit.Maui.Alerts;

namespace Cooker.Pages;

public partial class DishDetailsPage : ContentPage
{
    readonly RecipeModel recipe;
    readonly DatabaseService database;

    readonly List<StepTimerModel> timers;

    public DishDetailsPage(RecipeModel model)
    {
        InitializeComponent();

        recipe = model;
        database = new DatabaseService();

        DishName.Text = recipe.Name ?? "";
        CuisineLabel.Text = recipe.Cuisine ?? "";
        IngredientsLabel.Text = recipe.Ingredients ?? "";
        StepsLabel.Text = recipe.Steps ?? "";

        if (!string.IsNullOrEmpty(recipe.ImagePath))
            DishImage.Source = recipe.ImagePath;

        timers = database.GetTimers(recipe.Id);

        TimerCollection.ItemsSource = timers;
    }

    async void StartTimer_Clicked(object sender, EventArgs e)
    {
        if ((sender as Button)?.BindingContext is not StepTimerModel timer)
            return;

        int seconds = timer.TimerSeconds;

        while (seconds > 0)
        {
            await Task.Delay(1000);
            seconds--;
        }

        await Toast.Make("Cooking step finished").Show();

        if (Preferences.Default.Get("vibration", true)
            && DeviceInfo.Platform != DevicePlatform.WinUI)
        {
            Vibration.Default.Vibrate();
        }
    }

    async void EditRecipe_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CookingStepsPage(recipe));
    }
}