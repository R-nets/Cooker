using SQLite;

namespace Cooker.Models;

[Table("StepTimers")]
public class StepTimerModel
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int RecipeId { get; set; }

    public string StepDescription { get; set; } = "";

    public int TimerSeconds { get; set; }
}