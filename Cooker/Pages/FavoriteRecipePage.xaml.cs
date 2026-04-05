using Cooker.Models;
using Cooker.Services;

namespace Cooker.Pages;

public partial class FavoriteRecipePage : ContentPage
{
    readonly DatabaseService database = new();
    readonly INotificationService notificationService;

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
    }

    async void RecipeSelected(object sender, SelectionChangedEventArgs e)
{
    if (e.CurrentSelection.Count == 0)
        return;

    if (e.CurrentSelection[0] is not RecipeModel selectedRecipe)
        return;

    FavoriteCollection.SelectedItem = null;

    await Navigation.PushAsync(
        new DishDetailsPage(selectedRecipe, notificationService));
}
}