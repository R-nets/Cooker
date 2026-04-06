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
    CancellationTokenSource? timerToken;
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
            timerCard.StartTimerClicked -= OnStartTimer;
            timerCard.PauseTimerClicked -= OnPauseTimer;

            timerCard.StartTimerClicked += OnStartTimer;
            timerCard.PauseTimerClicked += OnPauseTimer;
        }
    }

    async void OnStartTimer(object? sender, StepTimerModel timer)
    {
        await RunTimer(timer);
        await this.FadeToAsync(0.7, 150);
        await this.FadeToAsync(1, 200);
    }

    void OnPauseTimer(object? sender, StepTimerModel timer)
    {
        PauseTimer(timer);
    }

    async Task RunTimer(StepTimerModel timer)
    {
        timer.IsRunning = true;

        // Only reset if first start
        if (timer.RemainingSeconds <= 0)
            timer.RemainingSeconds = timer.TimerSeconds;

        timerToken = new CancellationTokenSource();

        await Toast.Make($"Timer started: {timer.TimerSeconds} seconds").Show();

        while (timer.RemainingSeconds > 0 && !timerToken.IsCancellationRequested)
        {
            await Task.Delay(1000);
            timer.RemainingSeconds--;   // updates UI automatically
        }

        timer.IsRunning = false;

        if (timer.RemainingSeconds <= 0)
        {
            if (Preferences.Default.Get("vibration", true) && DeviceInfo.Platform != DevicePlatform.WinUI)
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

            // Only refresh ONCE when removing item
            TimerCollection.ItemsSource = timers;

            if (timers.Count == 0)
            {
                await DisplayAlertAsync("Recipe Completed 🎉",
                    "You completed all steps.", "OK");

                RestartRecipe();
            }
        }
    }

    void PauseTimer(StepTimerModel timer)
    {
        timerToken?.Cancel();
        timer.IsRunning = false;
    }

    async void ResumeTimer(StepTimerModel timer)
    {
        await RunTimer(timer);
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

    public static class PermissionHelper
    {
        public static async Task RequestWithExplanation<T>(string message)
            where T : Permissions.BasePermission, new()
        {
            var status = await Permissions.CheckStatusAsync<T>();

            if (status != PermissionStatus.Granted)
            {
                var page = Application.Current?.Windows[0].Page;

                if (page != null)
                {
                    await page.DisplayAlertAsync(
                        "Permission Required",
                        message,
                        "OK");
                }

                await Permissions.RequestAsync<T>();
            }
        }
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();

        App.ApplyBackgroundToPage(this);

        BackgroundOverlay.IsVisible = 
            Preferences.Default.ContainsKey("background_image");
    }
}