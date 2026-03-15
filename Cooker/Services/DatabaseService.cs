using Cooker.Models;
using SQLite;

namespace Cooker.Services;

public class DatabaseService
{
    readonly SQLiteConnection connection;

    public DatabaseService()
    {
        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "cooker.db");

        connection = new SQLiteConnection(dbPath);

        connection.CreateTable<RecipeModel>();
        connection.CreateTable<StepTimerModel>();
    }

    public List<RecipeModel> GetRecipes()
    {
        return connection.Table<RecipeModel>().ToList();
    }

    public void SaveRecipe(RecipeModel recipe)
    {
        if (recipe.Id == 0)
            connection.Insert(recipe);
        else
            connection.Update(recipe);
    }

    public void DeleteRecipe(RecipeModel recipe)
    {
        connection.Delete(recipe);
    }

    public List<StepTimerModel> GetTimers(int recipeId)
    {
        return connection.Table<StepTimerModel>()
            .Where(t => t.RecipeId == recipeId)
            .ToList();
    }

    public void SaveTimer(StepTimerModel timer)
    {
        if (timer.Id == 0)
            connection.Insert(timer);
        else
            connection.Update(timer);
    }

    public void DeleteTimer(StepTimerModel timer)
    {
        connection.Delete(timer);
    }
}