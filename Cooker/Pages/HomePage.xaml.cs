using Cooker.Models;
using Cooker.Services;
using Cooker.ViewModels;

namespace Cooker.Pages;

public partial class HomePage : ContentPage
{
    private readonly RecipeViewModel viewModel;
    private readonly INotificationService notificationService;
    bool isMultiSelectMode = false;
    bool isFilterHidden = false;
    bool isAnimating = false;
    const double threshold = 10;
    double filterHeight = -1;
    const double showButtonOffset = 126;

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
        App.ApplyBackgroundToPage(this);
        BackgroundOverlay.IsVisible =
            Preferences.Default.ContainsKey("background_image");
    }

    async void RecipeSelected(object sender, SelectionChangedEventArgs e)
    {
        if (isAnimating) return;

        if (isMultiSelectMode)
            return;

        if (e.CurrentSelection.Count == 0)
            return;

        if (e.CurrentSelection[0] is not RecipeModel selectedRecipe)
            return;

        RecipeCollection.SelectedItem = null;

        await Navigation.PushAsync(
            new DishDetailsPage(selectedRecipe, notificationService));
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

    async void OnCollectionScrolled(object sender, ItemsViewScrolledEventArgs e)
    {
        if (isAnimating) return;

        if (filterHeight <= 0)
            filterHeight = FilterContainer.Height;

        if (e.VerticalDelta > threshold && !isFilterHidden)
        {
            isAnimating = true;
            isFilterHidden = true;

            await Task.WhenAll(
                FilterBar.FadeToAsync(0, 100),
                FilterContainer.TranslateToAsync(0, -filterHeight, 150)
            );

            FilterRow.Height = new GridLength(0);

            isAnimating = false;
        }
        else if (e.VerticalDelta < -threshold && isFilterHidden)
        {
            isAnimating = true;

            FilterRow.Height = GridLength.Auto;
            FilterContainer.TranslationY = -filterHeight;

            await Task.WhenAll(
                FilterContainer.TranslateToAsync(0, 0, 150),
                FilterBar.FadeToAsync(1, 150)
            );

            isFilterHidden = false;
            isAnimating = false;
        }

        if (e.VerticalOffset > showButtonOffset)
        {
            if (!ScrollToTopButton.IsVisible)
            {
                ScrollToTopButton.IsVisible = true;
                await ScrollToTopButton.FadeToAsync(1, 200);
            }
        }
        else
        {
            if (ScrollToTopButton.IsVisible)
            {
                await ScrollToTopButton.FadeToAsync(0, 200);
                ScrollToTopButton.IsVisible = false;
            }
        }
    }

    async void ScrollToTop_Clicked(object sender, EventArgs e)
    {
        if (isAnimating) return;

        isAnimating = true;

        // Up to top
        RecipeCollection.ScrollTo(0, position: ScrollToPosition.Start, animate: true);

        // UI settles before restoring drawer
        await Task.Delay(100);

        FilterRow.Height = GridLength.Auto;

        if (filterHeight <= 0)
            filterHeight = FilterContainer.Height;

        FilterContainer.TranslationY = -filterHeight;

        await Task.WhenAll(
            FilterContainer.TranslateToAsync(0, 0, 150),
            FilterBar.FadeToAsync(1, 150)
        );

        isFilterHidden = false;
        isAnimating = false;
    }

    void CuisineChanged(object sender, EventArgs e)
    {
        var selected = CuisinePicker.SelectedItem?.ToString() ?? "";

        viewModel.IsLoading = true;

        if (selected == "All")
        {
            RecipeCollection.ItemsSource = viewModel.Recipes;
        }
        else
        {
            var filtered = viewModel.Recipes
                .Where(r => r.Cuisine == selected)
                .ToList();

            RecipeCollection.ItemsSource = filtered;
        }

        viewModel.IsLoading = false;
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

        recipe.IsFavorite = !recipe.IsFavorite;

        var database = new DatabaseService();
        database.SaveRecipe(recipe);

        await btn.ScaleToAsync(1.2, 150);
        await btn.ScaleToAsync(1.0, 150);
    }

    void EnableMultiSelect_Clicked(object sender, EventArgs e)
    {
        isMultiSelectMode = !isMultiSelectMode;

        RecipeCollection.SelectionMode = isMultiSelectMode
            ? SelectionMode.Multiple
            : SelectionMode.Single;
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

        await RecipeCollection.FadeToAsync(0.5, 150);
        await RecipeCollection.ScaleToAsync(0.97, 150);

        viewModel.DeleteRecipe(recipe);

        await RecipeCollection.ScaleToAsync(1, 150);
        await RecipeCollection.FadeToAsync(1, 150);
    }
}