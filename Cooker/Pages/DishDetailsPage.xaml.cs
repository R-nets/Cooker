using CommunityToolkit.Maui.Alerts;
using Cooker.Models;
using Cooker.Services;

namespace Cooker.Pages;

public partial class DishDetailsPage : ContentPage
{
    readonly INotificationService notificationService;
    readonly RecipeModel recipe;
    readonly DatabaseService database;
    readonly List<StepTimerModel> timers;

    public DishDetailsPage(RecipeModel model, INotificationService service)
    {
        InitializeComponent();

        recipe = model ?? new RecipeModel();
        database = new DatabaseService();
        notificationService = service;

        DishName.Text = recipe.Name ?? "";
        CuisineLabel.Text = recipe.Cuisine ?? "";
        IngredientsLabel.Text = recipe.Ingredients ?? "";
        StepsLabel.Text = recipe.Steps ?? "";

        if (!string.IsNullOrEmpty(recipe.ImagePath))
            DishImage.Source = recipe.ImagePath;

        timers = database.GetTimers(recipe.Id) ?? [];
        TimerCollection.ItemsSource = timers;
    }

    async void StartTimer_Clicked(object sender, EventArgs e)
    {
        if (sender is not Button btn || btn.BindingContext is not StepTimerModel timer)
            return;

        int seconds = timer.TimerSeconds;

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await Toast.Make($"Timer started: {seconds}s").Show();
        });

        await Task.Run(async () =>
        {
            await Task.Delay(seconds * 1000);
        });

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await Toast.Make("Cooking step finished").Show();

            if (Preferences.Default.Get("vibration", true)
                && DeviceInfo.Platform != DevicePlatform.WinUI)
            {
                Vibration.Default.Vibrate();
            }
        });

        notificationService.SendNotification(
            "Cooking Timer",
            timer.StepDescription,
            0
        );
    }

    async void EditRecipe_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CookingStepsPage(recipe));
    }
}