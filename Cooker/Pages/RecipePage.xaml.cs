using Cooker.Models;
using Cooker.ViewModels;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Alerts;

namespace Cooker.Pages;

public partial class RecipePage : ContentPage
{
    readonly RecipeModel recipe;
    readonly List<StepTimerModel> timers = [];

    public RecipePage(RecipeModel model)
    {
        InitializeComponent();

        recipe = model;

        NameEntry.Text = recipe.Name ?? string.Empty;
        IngredientsEditor.Text = recipe.Ingredients ?? string.Empty;
        StepsEditor.Text = recipe.Steps ?? string.Empty;

        if (!string.IsNullOrEmpty(recipe.ImagePath))
            RecipeImage.Source = recipe.ImagePath;
    }

    async void AddPhoto_Clicked(object sender, EventArgs e)
    {
        try
        {
            var photos = await MediaPicker.Default.PickPhotosAsync();

            if (photos == null || photos.Count == 0)
                return;

            var photo = photos[0];

            string newPath = Path.Combine(FileSystem.AppDataDirectory, photo.FileName);

            using var stream = await photo.OpenReadAsync();
            using var newStream = File.OpenWrite(newPath);

            await stream.CopyToAsync(newStream);

            recipe.ImagePath = newPath;

            RecipeImage.Source = newPath;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", ex.Message, "OK");
        }
    }

    async void AddTimer_Clicked(object sender, EventArgs e)
    {
        if (!int.TryParse(TimerSecondsEntry.Text, out int seconds))
        {
            await DisplayAlertAsync("Error", "Please enter a valid number.", "OK");
            return;
        }

        var timer = new StepTimerModel
        {
            RecipeId = recipe.Id,
            StepDescription = TimerStepEntry.Text ?? string.Empty,
            TimerSeconds = seconds
        };

        timers.Add(timer);

        TimerCollection.ItemsSource = null;
        TimerCollection.ItemsSource = timers;
    }

    async void Share_Clicked(object sender, EventArgs e)
    {
        string text =
            $"Recipe: {NameEntry.Text}\n\nIngredients:\n{IngredientsEditor.Text}\n\nSteps:\n{StepsEditor.Text}";

        try
        {
            await Clipboard.Default.SetTextAsync(text);
            await DisplayAlertAsync("Copied", "Recipe copied to clipboard", "OK");
        }
        catch
        {
            await DisplayAlertAsync("Clipboard", "Clipboard not available.", "OK");
        }
    }

    async void StartTimer_Clicked(object sender, EventArgs e)
    {
        await DisplayAlertAsync("Timer", "Cooking timer started for 5 seconds (demo)", "OK");

        await Task.Delay(5000);

        if (Preferences.Default.Get("vibration", true))
        {
            if (DeviceInfo.Platform != DevicePlatform.WinUI)
                Vibration.Default.Vibrate();
        }

        if (Preferences.Default.Get("notifications", true))
            await DisplayAlertAsync("Cooking Timer", "Food should be ready!", "OK");
    }

    static async Task StartTimer(int seconds)
    {
        int remaining = seconds;

        while (remaining > 0)
        {
            await Task.Delay(1000);
            remaining--;
        }

        if (Preferences.Default.Get("vibration", true) &&
            DeviceInfo.Platform != DevicePlatform.WinUI)
        {
            Vibration.Default.Vibrate();
        }

        var toast = Toast.Make("Cooking step finished");

        await toast.Show();
    }

    async void Save_Clicked(object sender, EventArgs e)
    {
        recipe.Name = NameEntry.Text ?? string.Empty;
        recipe.Cuisine = CuisinePicker.SelectedItem?.ToString() ?? string.Empty;
        recipe.Ingredients = IngredientsEditor.Text ?? string.Empty;
        recipe.Steps = StepsEditor.Text ?? string.Empty;

        RecipeViewModel.Current.SaveRecipe(recipe);

        await Navigation.PopAsync();
    }

    async void Delete_Clicked(object sender, EventArgs e)
    {
        RecipeViewModel.Current.DeleteRecipe(recipe);

        await Navigation.PopAsync();
    }
}