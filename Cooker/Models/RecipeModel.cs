using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cooker.Models;

[SQLite.Table("Recipes")]
public class RecipeModel
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = "";

    public string Cuisine { get; set; } = "";

    public string Ingredients { get; set; } = "";

    public string Steps { get; set; } = "";

    public string ImagePath { get; set; } = "";

    public bool IsFavorite { get; set; }
}
