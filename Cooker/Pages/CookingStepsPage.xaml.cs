using Cooker.Models;
using Cooker.Services;
using Microsoft.Maui.Storage;

namespace Cooker.Pages;

public partial class CookingStepsPage : ContentPage
{
    RecipeModel recipe;
    readonly DatabaseService database;

    readonly List<StepTimerModel> timers = [];

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

        for (int i = 0; i < 24; i++)
            HourPicker.Items.Add(i.ToString());

        for (int i = 0; i < 60; i++)
        {
            MinutePicker.Items.Add(i.ToString());
            SecondPicker.Items.Add(i.ToString());
        }

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

        using var stream = await photo.OpenReadAsync();
        using var newStream = File.OpenWrite(newPath);

        await stream.CopyToAsync(newStream);

        recipe.ImagePath = newPath;

        RecipeImage.Source = newPath;
    }

    async void AddTimer_Clicked(object sender, EventArgs e)
    {
        if (recipe.Id == 0)
        {
            recipe.Name = NameEntry?.Text ?? "New Recipe";
            recipe.Cuisine = CuisinePicker?.SelectedItem?.ToString() ?? "";
            recipe.Ingredients = IngredientsEditor?.Text ?? "";
            recipe.Steps = StepsEditor?.Text ?? "";

            database.SaveRecipe(recipe);

            recipe = database.GetRecipes().Last();
        }

        if (HourPicker.SelectedIndex == -1 &&
            MinutePicker.SelectedIndex == -1 &&
            SecondPicker.SelectedIndex == -1)
        {
            await DisplayAlertAsync("Error", "Please select time", "OK");
            return;
        }

        int hours = HourPicker.SelectedIndex < 0 ? 0 : HourPicker.SelectedIndex;
        int minutes = MinutePicker.SelectedIndex < 0 ? 0 : MinutePicker.SelectedIndex;
        int seconds = SecondPicker.SelectedIndex < 0 ? 0 : SecondPicker.SelectedIndex;

        int totalSeconds = (hours * 3600) + (minutes * 60) + seconds;

        if (totalSeconds <= 0)
        {
            await DisplayAlertAsync("Error", "Timer must be greater than 0", "OK");
            return;
        }

        var timer = new StepTimerModel
        {
            RecipeId = recipe.Id,
            StepDescription = StepDescriptionEntry.Text ?? "Cooking Step",
            TimerSeconds = totalSeconds
        };

        database.SaveTimer(timer);

        timers.Add(timer);

        TimerCollection.ItemsSource = null;
        TimerCollection.ItemsSource = timers;

        await DisplayAlertAsync("Success", "Timer added", "OK");

        // Reset UI
        StepDescriptionEntry.Text = "";
        HourPicker.SelectedIndex = -1;
        MinutePicker.SelectedIndex = -1;
        SecondPicker.SelectedIndex = -1;
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