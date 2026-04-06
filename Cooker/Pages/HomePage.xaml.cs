using Cooker.Models;
using Cooker.Services;
using Cooker.ViewModels;

namespace Cooker.Pages;

public partial class HomePage : ContentPage
{
    private readonly RecipeViewModel viewModel;
    private readonly INotificationService notificationService;

    bool isMultiSelectMode = false;

    public HomePage(INotificationService service)
    {
        InitializeComponent();

        notificationService = service;

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
        if (RecipeCollection.SelectionMode == SelectionMode.None)
            return;

        if (e.CurrentSelection.Count == 0)
            return;

        if (e.CurrentSelection[0] is not RecipeModel selectedRecipe)
            return;

        var animation = new Animation
        {
            {
                0,
                0.5,
                new Animation(v =>
        {
            RecipeCollection.Scale = v;
        }, 1, 0.95)
            },
            {
                0.5,
                1,
                new Animation(v =>
            {
                RecipeCollection.Scale = v;
            }, 0.95, 1)
            },
            {
                0,
                1,
                new Animation(v =>
            {
                RecipeCollection.Opacity = v;
            }, 1, 0.7)
            }
        };

        animation.Commit(
            this,
            "RecipeSelectAnimation",
            16,
            300,
            Easing.CubicInOut,
            async (v, c) =>
            {
                RecipeCollection.SelectedItem = null;

                await Navigation.PushAsync(
                    new DishDetailsPage(
                        selectedRecipe,
                        notificationService));
            });
    }

    async void AddRecipe_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CookingStepsPage(new RecipeModel()));
    }

    async void Settings_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SettingsPage());
    }

    async void Favorites_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new FavoriteRecipePage());
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

    async void FavoriteClicked(object sender, EventArgs e)
    {
        if (sender is not Button btn)
            return;

        if (btn.CommandParameter is not RecipeModel recipe)
            return;

        RecipeCollection.SelectionMode = SelectionMode.None;

        recipe.IsFavorite = !recipe.IsFavorite;

        var database = new DatabaseService();
        database.SaveRecipe(recipe);

        btn.InputTransparent = true;

        await btn.ScaleToAsync(1.2, 100);
        await btn.ScaleToAsync(1.0, 100);

        btn.InputTransparent = false;

        RecipeViewModel.Current?.Refresh();

        await Task.Delay(100);

        RecipeCollection.SelectionMode = isMultiSelectMode
            ? SelectionMode.Multiple
            : SelectionMode.Single;
    }

    void RecipeTapped(object sender, TappedEventArgs e)
    {
        if (!isMultiSelectMode)
            return;
    }

    async void RecipeLongPressed(object sender, TappedEventArgs e)
    {
        isMultiSelectMode = true;

        RecipeCollection.SelectionMode = SelectionMode.Multiple;

        bool confirm = await DisplayAlertAsync(
            "Multi Delete Mode",
            "You can now select multiple recipes for deletion.",
            "OK",
            "Cancel");

        if (!confirm)
        {
            isMultiSelectMode = false;
            RecipeCollection.SelectionMode = SelectionMode.Single;
        }
    }

    async void DeleteSelectedRecipes_Clicked(object sender, EventArgs e)
    {
        if (RecipeCollection.SelectedItems.Count == 0)
            return;

        bool confirm = await DisplayAlertAsync(
            "Delete Selected",
            $"Delete {RecipeCollection.SelectedItems.Count} selected recipes?",
            "Yes",
            "No");

        if (!confirm)
            return;

        foreach (var item in RecipeCollection.SelectedItems.ToList())
        {
            if (item is RecipeModel recipe)
            {
                viewModel.DeleteRecipe(recipe);
            }
        }

        RecipeCollection.SelectedItems.Clear();
        RecipeCollection.SelectionMode = SelectionMode.Single;
        isMultiSelectMode = false;
    }

    async void DeleteRecipe(object sender, EventArgs e)
    {
        if (sender is not SwipeItem swipe || swipe.BindingContext is not RecipeModel recipe)
            return;

        bool confirm = await DisplayAlertAsync(
            "Delete Recipe",
            $"Delete {recipe.Name}?",
            "Yes",
            "No");

        if (!confirm)
            return;

        await RecipeCollection.FadeToAsync(0.5, 120);
        await RecipeCollection.ScaleToAsync(0.97, 120);

        viewModel.DeleteRecipe(recipe);

        await RecipeCollection.ScaleToAsync(1, 120);
        await RecipeCollection.FadeToAsync(1, 120);
    }
}