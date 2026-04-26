using CommunityToolkit.Maui.Alerts;
using Cooker.Layouts;
using Cooker.Models;
using Cooker.Services;
using System.Collections.ObjectModel;

namespace Cooker.Pages;

public partial class DishDetailsPage : ContentPage
{
    readonly INotificationService notificationService;
    readonly RecipeModel recipe;
    readonly DatabaseService database;
    CancellationTokenSource? timerToken;
    List<StepTimerModel> timers = [];
    readonly List<StepTimerModel> completedTimers = [];
    int currentStepIndex = 0;
    bool isSequentialMode = false;

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
        RefreshTimerView();
    }

    void LoadTimers()
    {
        timers = database.GetTimersByRecipe(recipe.Id) ?? [];

        TimerCollection.ItemsSource = null;
        TimerCollection.ItemsSource = timers;
    }

    void RefreshTimerView()
    {
        TimerCollection.ItemsSource = null;
        TimerCollection.ItemsSource = timers
            .Where(t => !t.IsCompleted)
            .OrderBy(t => t.StepIndex)
            .ToList();
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
        if (timers.IndexOf(timer) != 0 && completedTimers.Count == 0)
        {
            bool proceed = await DisplayAlertAsync(
                "Warning",
                "You are not starting from Step 1.",
                "Continue",
                "Cancel");

            if (!proceed) return;
        }

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
        // 1. Starting state
        timer.IsRunning = true;
        timerToken = new CancellationTokenSource();

        await Toast.Make($"Timer started: {timer.TimerSeconds} seconds").Show();

        // 2. Countdown loop
        while (timer.RemainingSeconds > 0 && !timerToken.IsCancellationRequested)
        {
            await Task.Delay(1000);
            timer.RemainingSeconds--;
        }

        // 3. Handle cancellation first
        if (timerToken.IsCancellationRequested)
        {
            timer.IsRunning = false;
            return;
        }

        // 4. Stopping state
        timer.IsRunning = false;

        // 5.1 FlashTorch for instant feedback - native structure
        await DishDetailsPage.FlashTorch();

        // 5.2 Vibration - native structure
        if (Preferences.Default.Get("vibration", true) &&
            DeviceInfo.Platform != DevicePlatform.WinUI)
        {
            try
            {
                Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(300));
            }
            catch (FeatureNotSupportedException)
            {
                // Safe fallback (do nothing)
            }
        }
        ;

        // 5.3 Notification if enabled in SettingsPage - native structure
        if (Preferences.Default.Get("notifications", true))
        {
            notificationService.SendNotification(
                "Cooking Timer Finished",
                timer.StepDescription,
                0);
        }

        await DisplayAlertAsync(
            "⏰ Time's Up!",
            timer.StepDescription,
            "OK");

        await Toast.Make($"Finished: {timer.StepDescription}").Show();

        // Mark as completed
        timer.IsCompleted = true;
        database.SaveTimer(timer);

        // Refresh UI
        RefreshTimerView();

        // 6. Check completion
        if (!isSequentialMode && timers.All(t => t.IsCompleted))
        {
            await DisplayAlertAsync(
                "Recipe Completed 🎉",
                "You completed all steps.",
                "OK");

            RestartRecipe();
        }
    }

    void PauseTimer(StepTimerModel timer)
    {
        timerToken?.Cancel();
        timer.IsRunning = false;
    }

    static async Task FlashTorch()
    {
        try
        {
            // Respect to the SettingsPage toggle
            if (!Preferences.Default.Get("torch", true))
                return;

            // Request camera permission if not granted
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();

            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Camera>();
            }

            if (status != PermissionStatus.Granted)
                return;

            await Flashlight.Default.TurnOnAsync();
            await Task.Delay(200);
            await Flashlight.Default.TurnOffAsync();
        }
        catch
        {
            // Device may not support flashlight or permission denied
        }
    }

    async void ResumeTimer(StepTimerModel timer)
    {
        await RunTimer(timer);
    }

    void RestartRecipe()
    {
        foreach (var t in timers)
        {
            t.IsCompleted = false;
            t.RemainingSeconds = t.TimerSeconds;
            t.IsRunning = false;
        }

        completedTimers.Clear();
        RefreshTimerView();
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

    async void StartSequentialCooking_Clicked(object sender, EventArgs e)
    {
        if (timers.Count == 0) return;

        isSequentialMode = true;
        currentStepIndex = 0;

        await RunStepSequence();

        isSequentialMode = false;
    }

    async Task RunStepSequence()
    {
        var remainingTimers = timers
            .Where(t => !t.IsCompleted)
            .OrderBy(t => t.StepIndex)
            .ToList();

        currentStepIndex = 0;

        while (currentStepIndex < remainingTimers.Count)
        {
            var timer = remainingTimers[currentStepIndex];

            await RunTimer(timer);
            RefreshTimerView();

            bool isLastStep = currentStepIndex == remainingTimers.Count - 1;

            string action;

            if (isLastStep)
            {
                action = await DisplayActionSheetAsync(
                    "Final Step",
                    "Cancel",
                    null,
                    "Finish Recipe",
                    "Redo Step",
                    "Add 10 Minutes"
                );
            }
            else
            {
                action = await DisplayActionSheetAsync(
                    "Next Action",
                    "Cancel",
                    null,
                    "Next Step",
                    "Redo Step",
                    "Add 10 Minutes",
                    "Skip Step"
                );
            }

            if (action == "Redo Step")
            {
                continue;
            }
            else if (action == "Add 10 Minutes")
            {
                timer.RemainingSeconds = 600;
                await RunTimer(timer);
                continue;
            }
            else if (action == "Skip Step")
            {
                timer.IsCompleted = true;
                database.SaveTimer(timer);
                RefreshTimerView();
                currentStepIndex++;
                continue;
            }
            else if (action == "Finish Recipe")
            {
                timer.IsCompleted = true;
                database.SaveTimer(timer);
                RefreshTimerView();
                currentStepIndex++;
            }
            else // Next Step
            {
                currentStepIndex++;
            }
        }

        await DisplayAlertAsync("Recipe Completed 🎉",
            "You completed all steps.", "OK");

        RestartRecipe();
    }

    void PlayPauseTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is StepTimerModel timer)
        {
            if (timer.IsRunning)
                PauseTimer(timer);
            else
                _ = RunTimer(timer);
        }
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

    void FavoriteClicked(object sender, EventArgs e)
    {
        if (sender is Button button &&
            button.CommandParameter is RecipeModel recipe)
        {
            recipe.IsFavorite = !recipe.IsFavorite;

            database.SaveRecipe(recipe);

            // Force UI refresh
            BindingContext = null;
            BindingContext = recipe;
        }
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

    async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        App.ApplyBackgroundToPage(this);

        BackgroundOverlay.IsVisible = 
            Preferences.Default.ContainsKey("background_image");
    }
}