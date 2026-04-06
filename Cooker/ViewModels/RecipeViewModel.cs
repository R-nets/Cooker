using Cooker.Models;
using Cooker.Services;
using System.Collections.ObjectModel;

namespace Cooker.ViewModels;

public class RecipeViewModel
{
    private readonly DatabaseService database;

    public ObservableCollection<RecipeModel> Recipes { get; set; } = [];

    public static RecipeViewModel? Current { get; private set; }

    public bool IsLoading { get; set; }

    public RecipeViewModel()
    {
        database = new DatabaseService();
        Current = this;

        Refresh();
    }

    public void Refresh()
    {
        IsLoading = true;
        Recipes.Clear();

        var items = database.GetRecipes();

        foreach (var item in items)
            Recipes.Add(item);
    }

    public void DeleteRecipe(RecipeModel recipe)
    {
        database.DeleteRecipe(recipe);
        Recipes.Remove(recipe);
    }

    public void Search(string text)
    {
        var all = database.GetRecipes();

        if (string.IsNullOrWhiteSpace(text))
        {
            Recipes.Clear();
            foreach (var r in all)
                Recipes.Add(r);
            return;
        }

        var filtered = all
            .Where(r => r.Name.Contains(text, StringComparison.OrdinalIgnoreCase))
            .ToList();

        Recipes.Clear();
        foreach (var r in filtered)
            Recipes.Add(r);
    }
}