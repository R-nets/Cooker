#if ANDROID
using Cooker.Platforms.Android;
#endif
using Cooker.Models;
using Cooker.Services;

namespace Cooker.Pages;


public partial class FavoriteRecipePage : ContentPage
{
    readonly DatabaseService database = new();
    readonly INotificationService? notificationService;

    public FavoriteRecipePage()
    {
        InitializeComponent();

#if ANDROID
        notificationService = new NotificationService();
#endif
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        var recipes = database.GetRecipes()
            .Where(r => r.IsFavorite)
            .ToList();

        FavoriteCollection.ItemsSource = recipes;

        App.ApplyBackgroundToPage(this);
        int theme = Preferences.Default.Get("theme", 2);
        BackgroundOverlay.IsVisible = Preferences.Default.ContainsKey("background_image");
    }

    async void RecipeSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count == 0)
            return;

        if (e.CurrentSelection[0] is not RecipeModel selectedRecipe)
            return;

        if (sender is CollectionView collectionView)
            collectionView.SelectedItem = null;

        if (notificationService != null)
        {
            await Navigation.PushAsync(
                new DishDetailsPage(selectedRecipe, notificationService));
        }
    }

    void FavoriteClicked(object sender, EventArgs e)
    {
        if (sender is Button button &&
            button.CommandParameter is RecipeModel recipe)
        {
            recipe.IsFavorite = !recipe.IsFavorite;

            database.SaveRecipe(recipe);

            // Refresh list
            FavoriteCollection.ItemsSource = database.GetRecipes()
                .Where(r => r.IsFavorite)
                .ToList();
        }
    }

    async void CopyRecipe_Clicked(object sender, EventArgs e)
    {
        if (sender is not ImageButton button ||
            button.CommandParameter is not RecipeModel recipe)
            return;

        string text =
            $"Recipe: {recipe.Name}\n\n" +
            $"Cuisine: {recipe.Cuisine}\n\n" +
            $"Ingredients:\n{recipe.Ingredients}\n\n" +
            $"Steps:\n{recipe.Steps}";

        await Clipboard.Default.SetTextAsync(text);
        HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        await DisplayAlertAsync("Copied", "Recipe copied to clipboard", "OK");
    }
}