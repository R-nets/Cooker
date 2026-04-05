using Cooker.Models;
using Cooker.Services;
using Microsoft.Maui.Storage;

namespace Cooker.Pages;

public partial class CookingStepsPage : ContentPage
{
    private RecipeModel recipe;
    private readonly DatabaseService database;
    private bool isTimerMultiDeleteMode = false;
    private List<StepTimerModel> timers = [];

    public CookingStepsPage(RecipeModel model)
    {
        InitializeComponent();

        recipe = model ?? throw new ArgumentNullException(nameof(model));
        database = new DatabaseService();

        NameEntry.Text = recipe.Name ?? string.Empty;
        IngredientsEditor.Text = recipe.Ingredients ?? string.Empty;
        StepsEditor.Text = recipe.Steps ?? string.Empty;

        if (!string.IsNullOrEmpty(recipe.ImagePath))
            RecipeImage.Source = recipe.ImagePath;

        timers = database.GetTimers(recipe.Id);

        TimerCollection.ItemsSource = timers;
    }

    async void AddImage_Clicked(object sender, EventArgs e)
    {
        var photos = await MediaPicker.Default.PickPhotosAsync();
        var photo = photos.FirstOrDefault();

        if (photo == null)
            return;

        string newPath = Path.Combine(FileSystem.AppDataDirectory, photo.FileName);

        await using var stream = await photo.OpenReadAsync();
        await using var newStream = File.OpenWrite(newPath);

        await stream.CopyToAsync(newStream);

        recipe.ImagePath = newPath;
        RecipeImage.Source = newPath;
    }

    async void AddTimer_Clicked(object sender, EventArgs e)
    {
        if (recipe.Id == 0)
        {
            recipe.Name = NameEntry?.Text ?? "New Recipe";
            recipe.Cuisine = CuisinePicker?.SelectedItem?.ToString() ?? string.Empty;
            recipe.Ingredients = IngredientsEditor?.Text ?? string.Empty;
            recipe.Steps = StepsEditor?.Text ?? string.Empty;

            database.SaveRecipe(recipe);

            recipe = database.GetRecipes().Last();
        }

        bool hasHours = int.TryParse(HourEntry.Text, out int hours);
        bool hasMinutes = int.TryParse(MinuteEntry.Text, out int minutes);
        bool hasSeconds = int.TryParse(SecondEntry.Text, out int seconds);

        if (!hasHours)
            hours = 0;

        if (!hasMinutes)
            minutes = 0;

        if (!hasSeconds)
            seconds = 0;

        int totalSeconds = (hours * 3600) + (minutes * 60) + seconds;

        if (totalSeconds <= 0)
        {
            await DisplayAlertAsync("Error", "Please enter a valid time greater than 0.", "OK");
            return;
        }

        var timer = new StepTimerModel
        {
            RecipeId = recipe.Id,
            StepDescription = string.IsNullOrWhiteSpace(StepDescriptionEntry.Text)
                ? "Cooking Step"
                : StepDescriptionEntry.Text,
            TimerSeconds = totalSeconds
        };

        database.SaveTimer(timer);

        timers.Add(timer);

        TimerCollection.ItemsSource = null;
        TimerCollection.ItemsSource = timers;

        await DisplayAlertAsync("Success", "Timer added successfully.", "OK");

        StepDescriptionEntry.Text = string.Empty;
        HourEntry.Text = string.Empty;
        MinuteEntry.Text = string.Empty;
        SecondEntry.Text = string.Empty;
    }

    async void DeleteTimer_Clicked(object sender, EventArgs e)
    {
        if (sender is Button btn &&
            btn.CommandParameter is StepTimerModel timer)
        {
            bool confirm = await DisplayAlertAsync(
                "Delete Timer",
                "Are you sure you want to delete this timer?",
                "Yes",
                "No");

            if (!confirm)
                return;

            database.DeleteTimer(timer);

            timers.Remove(timer);

            TimerCollection.ItemsSource = null;
            TimerCollection.ItemsSource = timers;
        }
    }

    void EnableMultiDelete_Clicked(object sender, EventArgs e)
    {
        isTimerMultiDeleteMode = true;
        TimerCollection.SelectionMode = SelectionMode.Multiple;
    }

    async void DeleteSelectedTimers_Clicked(object sender, EventArgs e)
    {
        if (!isTimerMultiDeleteMode)
        {
            await DisplayAlertAsync(
                "Multi Delete",
                "Please enable multi delete mode first.",
                "OK");
            return;
        }

        if (TimerCollection.SelectedItems.Count == 0)
        {
            await DisplayAlertAsync(
                "No Timers Selected",
                "Please select at least one timer.",
                "OK");
            return;
        }

        bool confirm = await DisplayAlertAsync(
            "Delete Timers",
            $"Delete {TimerCollection.SelectedItems.Count} selected timer(s)?",
            "Yes",
            "No");

        if (!confirm)
            return;

        foreach (var item in TimerCollection.SelectedItems.ToList())
        {
            if (item is StepTimerModel timer)
            {
                database.DeleteTimer(timer);
                timers.Remove(timer);
            }
        }

        TimerCollection.ItemsSource = null;
        TimerCollection.ItemsSource = timers;

        TimerCollection.SelectedItems.Clear();
        TimerCollection.SelectionMode = SelectionMode.None;
        isTimerMultiDeleteMode = false;
    }

    async void SaveRecipe_Clicked(object sender, EventArgs e)
    {
        recipe.Name = NameEntry?.Text ?? string.Empty;
        recipe.Cuisine = CuisinePicker?.SelectedItem?.ToString() ?? string.Empty;
        recipe.Ingredients = IngredientsEditor?.Text ?? string.Empty;
        recipe.Steps = StepsEditor?.Text ?? string.Empty;

        database.SaveRecipe(recipe);

        await Navigation.PopAsync();
    }
}