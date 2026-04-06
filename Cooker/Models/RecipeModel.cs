using SQLite;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Cooker.Models;

[Table("Recipes")]
public partial class RecipeModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    void OnPropertyChanged([CallerMemberName] string name = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = "";

    public string Cuisine { get; set; } = "";

    public string Ingredients { get; set; } = "";

    public string Steps { get; set; } = "";

    public string ImagePath { get; set; } = "";

    bool isFavorite;
    public bool IsFavorite
    {
        get => isFavorite;
        set
        {
            if (isFavorite != value)
            {
                isFavorite = value;
                OnPropertyChanged();
            }
        }
    }
}