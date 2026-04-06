using Cooker.Models;
using Cooker.Services;
using System.Collections.ObjectModel;

namespace Cooker.ViewModels;

public class RecipeViewModel
{
    public static RecipeViewModel Current { get; private set; } = null!;

    private readonly DatabaseService database;

    public ObservableCollection<RecipeModel> Recipes { get; set; }

    public RecipeViewModel()
    {
        Current = this;

        database = new DatabaseService();

        Recipes = new ObservableCollection<RecipeModel>(
                    database.GetRecipes());
    }

    public void Refresh()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Recipes.Clear();

            foreach (var r in database.GetRecipes())
                Recipes.Add(r);
        });
    }

    public void SaveRecipe(RecipeModel recipe)
    {
        database.SaveRecipe(recipe);
        Refresh();
    }

    public void DeleteRecipe(RecipeModel recipe)
    {
        database.DeleteRecipe(recipe);
        Refresh();
    }

    public void Search(string text)
    {
        Recipes.Clear();

        var list = database.GetRecipes()
            .Where(r => r.Name.Contains(text ?? "", StringComparison.CurrentCultureIgnoreCase));

        foreach (var r in list)
            Recipes.Add(r);
    }

    public void ToggleFavorite(RecipeModel recipe)
    {
        recipe.IsFavorite = !recipe.IsFavorite;

        database.SaveRecipe(recipe);

        Refresh();
    }
}