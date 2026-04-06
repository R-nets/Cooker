using CommunityToolkit.Maui.Alerts;
using Cooker.Layouts;
using Cooker.Models;
using Cooker.Services;

namespace Cooker.Pages;

public partial class DishDetailsPage : ContentPage
{
    readonly INotificationService notificationService;
    readonly RecipeModel recipe;
    readonly DatabaseService database;

    List<StepTimerModel> timers = [];
    readonly List<StepTimerModel> completedTimers = [];

    public DishDetailsPage(RecipeModel model, INotificationService service)
    {
        InitializeComponent();

        recipe = model;
        BindingContext = recipe;

        notificationService = service;
        database = new DatabaseService();

        DishName.Text = recipe.Name ?? string.Empty;
        CuisineLabel.Text = recipe.Cuisine ?? string.Empty;
        IngredientsLabel.Text = recipe.Ingredients ?? string.Empty;
        StepsLabel.Text = recipe.Steps ?? string.Empty;

        if (!string.IsNullOrEmpty(recipe.ImagePath))
            DishImage.Source = recipe.ImagePath;

        LoadTimers();
    }

    void LoadTimers()
    {
        timers = database.GetTimersByRecipe(recipe.Id) ?? [];

        TimerCollection.ItemsSource = null;
        TimerCollection.ItemsSource = timers;
    }

    void TimerCard_Loaded(object sender, EventArgs e)
    {
        if (sender is TimerCardView timerCard)
        {
            timerCard.StartTimerClicked += async (_, timer) =>
            {
                await RunTimer(timer);
            };

            timerCard.DeleteTimerClicked += async (_, timer) =>
            {
                bool confirm = await DisplayAlertAsync(
                    "Delete Timer",
                    "Delete this timer?",
                    "Yes",
                    "No");

                if (!confirm)
                    return;

                timers.Remove(timer);

                TimerCollection.ItemsSource = null;
                TimerCollection.ItemsSource = timers;
            };
        }
    }

    async Task RunTimer(StepTimerModel timer)
    {
        int seconds = timer.TimerSeconds;

        await Toast.Make($"Timer started: {seconds} seconds").Show();

        await Task.Delay(seconds * 1000);

        if (Preferences.Default.Get("vibration", true) &&
            DeviceInfo.Platform != DevicePlatform.WinUI)
        {
            Vibration.Default.Vibrate();
        }

        notificationService.SendNotification(
            "Cooking Timer Finished",
            timer.StepDescription,
            0);

        await Toast.Make($"Finished: {timer.StepDescription}").Show();

        completedTimers.Add(timer);
        timers.Remove(timer);

        TimerCollection.ItemsSource = null;
        TimerCollection.ItemsSource = timers;

        if (timers.Count == 0)
        {
            await DisplayAlertAsync(
                "Recipe Completed 🎉",
                "Congratulations! You have completed all cooking steps for this recipe.",
                "Awesome");

            RestartRecipe();

            await Toast.Make("Recipe restarted automatically").Show();
        }
    }

    void RestartRecipe()
    {
        completedTimers.Clear();
        LoadTimers();
    }

    async void RestartRecipe_Clicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlertAsync(
            "Restart Recipe",
            "Do you want to restart all recipe timers?",
            "Yes",
            "No");

        if (!confirm)
            return;

        RestartRecipe();

        await Toast.Make("Recipe restarted").Show();
    }

    async void CopyRecipe_Clicked(object sender, EventArgs e)
    {
        string text =
            $"Recipe: {recipe.Name}\n\n" +
            $"Cuisine: {recipe.Cuisine}\n\n" +
            $"Ingredients:\n{recipe.Ingredients}\n\n" +
            $"Steps:\n{recipe.Steps}";

        try
        {
            await Clipboard.Default.SetTextAsync(text);
            await DisplayAlertAsync("Copied", "Recipe copied to clipboard", "OK");
        }
        catch
        {
            await DisplayAlertAsync("Error", "Clipboard is not available", "OK");
        }
    }

    async void EditRecipe_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CookingStepsPage(recipe));
    }
}