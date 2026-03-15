using Cooker.Models;
using Cooker.Services;

namespace Cooker.Pages;

public partial class CookingStepsPage : ContentPage
{
    readonly RecipeModel recipe;
    readonly DatabaseService database;

    readonly List<StepTimerModel> timers = [];

    public CookingStepsPage(RecipeModel model)
    {
        InitializeComponent();

        recipe = model;
        database = new DatabaseService();

        NameEntry.Text = recipe.Name ?? "";
        IngredientsEditor.Text = recipe.Ingredients ?? "";
        StepsEditor.Text = recipe.Steps ?? "";

        timers = database.GetTimers(recipe.Id);

        TimerCollection.ItemsSource = timers;
    }

    void AddTimer_Clicked(object sender, EventArgs e)
    {
        if (!int.TryParse(TimerSecondsEntry.Text, out int seconds))
            return;

        StepTimerModel timer = new()
        {
            RecipeId = recipe.Id,
            StepDescription = TimerStepEntry.Text ?? "",
            TimerSeconds = seconds
        };

        timers.Add(timer);

        TimerCollection.ItemsSource = null;
        TimerCollection.ItemsSource = timers;
    }

    async void SaveRecipe_Clicked(object sender, EventArgs e)
    {
        recipe.Name = NameEntry.Text ?? "";
        recipe.Cuisine = CuisinePicker.SelectedItem?.ToString() ?? "";
        recipe.Ingredients = IngredientsEditor.Text ?? "";
        recipe.Steps = StepsEditor.Text ?? "";

        database.SaveRecipe(recipe);

        foreach (var t in timers)
        {
            t.RecipeId = recipe.Id;
            database.SaveTimer(t);
        }

        await Navigation.PopAsync();
    }
}