using Cooker.Models;
using Cooker.ViewModels;

namespace Cooker.Pages;

public partial class HomePage : ContentPage
{
    private readonly RecipeViewModel viewModel;

    public HomePage()
    {
        InitializeComponent();

        viewModel = new RecipeViewModel();
        RecipeCollection.ItemsSource = viewModel.Recipes;

        CuisinePicker.SelectedIndex = 0;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        viewModel.Refresh();
    }

    async void RecipeSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is RecipeModel recipe)
            await Navigation.PushAsync(new DishDetailsPage(recipe));
    }

    async void AddRecipe_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CookingStepsPage(new RecipeModel()));
    }

    async void Settings_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SettingsPage());
    }

    void CuisineChanged(object sender, EventArgs e)
    {
        var selectedItem = CuisinePicker.SelectedItem;
        string selected = selectedItem?.ToString() ?? string.Empty;

        if (selected == "All")
        {
            RecipeCollection.ItemsSource = viewModel.Recipes;
        }
        else
        {
            var filtered = new System.Collections.Generic.List<RecipeModel>();
            foreach (var r in viewModel.Recipes)
            {
                if (r.Cuisine == selected)
                    filtered.Add(r);
            }
            RecipeCollection.ItemsSource = filtered;
        }
    }

    void SearchChanged(object sender, TextChangedEventArgs e)
    {
        viewModel.Search(e.NewTextValue);
    }

    void DeleteRecipeSwipe(object sender, EventArgs e)
    {
        if (sender is not SwipeItem swipeItem)
            return;

        if (swipeItem.BindingContext is not RecipeModel item)
            return;

        viewModel.DeleteRecipe(item);
    }
}