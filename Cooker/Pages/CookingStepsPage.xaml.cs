using Cooker.Models;
using Cooker.Services;
using Cooker.ViewModels;
using Microsoft.Maui.Storage;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;

namespace Cooker.Pages;

public partial class CookingStepsPage : ContentPage
{
    readonly RecipeModel recipe;
    private readonly DatabaseService database;
    readonly bool isInitializing = true;
    bool isAutoSaving = false;
    readonly bool isNewRecipe;
    bool hasUnsavedChanges = false;
    readonly ObservableCollection<StepTimerModel> timers = [];
    readonly ObservableCollection<StepInputModel> stepInputs = [];
    static readonly string[] LineSeparators = ["\r\n", "\n", "\r"];

    public CookingStepsPage(RecipeModel model)
    {
        InitializeComponent();

        // lock initialization mode
        isInitializing = true;

        // If null, create empty recipe
        recipe = model ?? new RecipeModel();
        database = new DatabaseService();

        // No ID means new recipe
        isNewRecipe = recipe.Id == 0;

        // detach old handlers (safety)
        NameEntry.TextChanged -= OnNameChanged;
        IngredientsEditor.TextChanged -= OnIngredientsChanged;

        // attach clean handlers (ONLY ONCE)
        NameEntry.TextChanged += OnNameChanged;
        IngredientsEditor.TextChanged += OnIngredientsChanged;

        NameEntry.Text = recipe.Name ?? string.Empty;
        IngredientsEditor.Text = recipe.Ingredients ?? string.Empty;

        // Load cooking steps from SQLite
        var existingTimers = isNewRecipe
            ? []
            : database.GetTimersByRecipe(recipe.Id) ?? [];

        // Set counter
        int totalSteps = existingTimers.Count > 0
            ? existingTimers.Max(t => t.StepIndex)
            : 1;

        StepsStepper.Value = totalSteps;
        StepsCountLabel.Text = totalSteps.ToString();

        if (!string.IsNullOrEmpty(recipe.ImagePath))
            RecipeImage.Source = recipe.ImagePath;

        LoadStepInputs();

        // Allow user editing again
        // Reset the unsaved changes
        hasUnsavedChanges = false;
        isInitializing = false;

        // If new recipe
        if (isNewRecipe)
        {
            stepInputs.Clear();
            StepsStepper.Value = 1;
            StepsCountLabel.Text = "1";
            LoadStepInputs();
        }
    }

    void OnNameChanged(object? sender, TextChangedEventArgs e)
    {
        // IF still initializing ignore event
        // ELSE: Mark recipe as "not saved"
        if (!isInitializing)
            hasUnsavedChanges = true;
    }

    void OnIngredientsChanged(object? sender, TextChangedEventArgs e)
    {
        // IF still initializing ignore event
        // ELSE: Mark recipe as "not saved"
        if (!isInitializing)
            hasUnsavedChanges = true;
    }

    void LoadStepInputs()
    {
        stepInputs.Clear();

        int totalSteps = (int)StepsStepper.Value;

        var existingTimers = isNewRecipe
            ? []
            : database.GetTimersByRecipe(recipe.Id) ?? [];

        for (int i = 1; i <= totalSteps; i++)
        {
            var existing = existingTimers.FirstOrDefault(t => t.StepIndex == i);

            StepInputModel step;

            if (existing != null)
            {
                step = new StepInputModel
                {
                    StepIndex = i,
                    Description = existing.StepDescription,
                    Hours = (existing.TimerSeconds / 3600).ToString(),
                    Minutes = ((existing.TimerSeconds % 3600) / 60).ToString(),
                    Seconds = (existing.TimerSeconds % 60).ToString(),
                    TimerId = existing.Id
                };
            }
            else
            {
                // Retain Blank timers
                step = new StepInputModel
                {
                    StepIndex = i,
                    Description = "",
                    Hours = "0",
                    Minutes = "0",
                    Seconds = "0",
                    TimerId = 0
                };
            }

            step.PropertyChanged += (_, __) =>
            {
                if (!isInitializing)
                    hasUnsavedChanges = true;
                AutoSave();
            };

            stepInputs.Add(step);
        }

        StepCollection.ItemsSource = stepInputs;
    }

    async void AutoSave()
    {
        if (isAutoSaving) return;

        isAutoSaving = true;

        foreach (var step in stepInputs)
        {
            int h = int.TryParse(step.Hours, out var hh) ? hh : 0;
            int m = int.TryParse(step.Minutes, out var mm) ? mm : 0;
            int s = int.TryParse(step.Seconds, out var ss) ? ss : 0;

            int total = (h * 3600) + (m * 60) + s;

            if (total <= 0) continue;

            var timer = new StepTimerModel
            {
                Id = step.TimerId,
                RecipeId = recipe.Id,
                StepIndex = step.StepIndex,
                StepDescription = step.Description,
                TimerSeconds = total
            };

            database.SaveTimer(timer);
        }

        isAutoSaving = false;
    }

    async void AddImage_Clicked(object sender, EventArgs e)
    {
        string action = await DisplayActionSheetAsync(
            "Select Image Source",
            "Cancel",
            null,
            "Camera",
            "Gallery");

        FileResult? photo = null;

        if (action == "Camera")
        {
            if (!Preferences.Default.Get("camera", true))
            {
                await DisplayAlertAsync("Permission Disabled",
                    "Enable camera permission in settings first.",
                    "OK");
                return;
            }

            await PermissionHelper.RequestWithExplanation<Permissions.Camera>(
                "Camera access is needed to take photos.");

            photo = await MediaPicker.Default.CapturePhotoAsync();
        }
        else if (action == "Gallery")
        {
            if (!Preferences.Default.Get("gallery", true))
            {
                await DisplayAlertAsync("Permission Disabled",
                    "Enable gallery permission in settings first.",
                    "OK");
                return;
            }

            await PermissionHelper.RequestWithExplanation<Permissions.Photos>(
                "Photo access is needed to select images.");

            var photos = await MediaPicker.Default.PickPhotosAsync();
            photo = photos?.FirstOrDefault();
        }

        if (photo == null)
            return;

        string newPath = Path.Combine(FileSystem.AppDataDirectory, photo.FileName);

        await using var stream = await photo.OpenReadAsync();
        await using var newStream = File.OpenWrite(newPath);

        await stream.CopyToAsync(newStream);

        recipe.ImagePath = newPath;
        RecipeImage.Source = newPath;
    }

    void StepsStepper_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        int newValue = (int)e.NewValue;

        // double-check with users before reducing
        if (newValue < stepInputs.Count)
        {
            bool hasData = stepInputs
                .Skip(newValue)
                .Any(s => !string.IsNullOrWhiteSpace(s.Description)
                       || s.Hours != "0"
                       || s.Minutes != "0"
                       || s.Seconds != "0");

            if (hasData)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    bool confirm = await DisplayAlertAsync(
                        "Warning",
                        "You are removing steps with data. Continue?",
                        "Yes",
                        "No");

                    if (!confirm)
                    {
                        StepsStepper.Value = stepInputs.Count;
                        return;
                    }

                    ApplyStepCount(newValue);
                });

                return;
            }
        }

        ApplyStepCount(newValue);
    }

    void ApplyStepCount(int totalSteps)
    {
        StepsCountLabel.Text = totalSteps.ToString();

        while (stepInputs.Count < totalSteps)
        {
            var step = new StepInputModel
            {
                StepIndex = stepInputs.Count + 1
            };

            step.PropertyChanged += (_, __) =>
            {
                if (!isInitializing)
                    hasUnsavedChanges = true;
                AutoSave();
            };

            stepInputs.Add(step);
        }

        while (stepInputs.Count > totalSteps)
        {
            stepInputs.RemoveAt(stepInputs.Count - 1);
        }

        // For re-indexing
        for (int i = 0; i < stepInputs.Count; i++)
        {
            stepInputs[i].StepIndex = i + 1;
        }
    }

    void GenerateSteps()
    {
        stepInputs.Clear();

        int totalSteps = (int)StepsStepper.Value;
        if (totalSteps <= 0)
            return;

        for (int i = 0; i < totalSteps; i++)
        {
            stepInputs.Add(new StepInputModel
            {
                StepIndex = i + 1
            });
        }

        StepCollection.ItemsSource = stepInputs;
    }

    async void SaveRecipe_Clicked(object sender, EventArgs e)
    {
        hasUnsavedChanges = false;
        var database = new DatabaseService();

        recipe.Name = NameEntry?.Text ?? string.Empty;
        recipe.Cuisine = CuisinePicker?.SelectedItem?.ToString() ?? string.Empty;
        recipe.Ingredients = IngredientsEditor?.Text ?? string.Empty;
        recipe.Steps = ((int)StepsStepper.Value).ToString();

        database.SaveRecipe(recipe);

        foreach (var step in stepInputs)
        {
            int h = int.TryParse(step.Hours, out var hh) ? hh : 0;
            int m = int.TryParse(step.Minutes, out var mm) ? mm : 0;
            int s = int.TryParse(step.Seconds, out var ss) ? ss : 0;

            int total = (h * 3600) + (m * 60) + s;

            if (total <= 0)
                continue;

            var timer = new StepTimerModel
            {
                Id = step.TimerId,
                RecipeId = recipe.Id,
                StepIndex = step.StepIndex,
                StepDescription = step.Description,
                TimerSeconds = total
            };

            // Update if timer Id exists
            database.SaveTimer(timer); 
        }

        RecipeViewModel.Current?.Refresh();

        await DisplayAlertAsync("Saved", "Recipe saved successfully", "OK");
        hasUnsavedChanges = false;
        await Navigation.PopAsync();
    }

    protected override bool OnBackButtonPressed()
    {
        if (!hasUnsavedChanges)
            return base.OnBackButtonPressed();

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            bool confirm = await DisplayAlertAsync(
                "Unsaved Changes",
                "You have unsaved recipe data. Leave without saving?",
                "Leave",
                "Stay");

            if (confirm)
            {
                hasUnsavedChanges = false;
                await Navigation.PopAsync();
            }
        });

        // To cancel default navigation
        return true;
    }

    void RefreshUI()
    {
        StepCollection.ItemsSource = null;
        StepCollection.ItemsSource = stepInputs;
    }

    async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        App.ApplyBackgroundToPage(this);

        BackgroundOverlay.IsVisible = 
            Preferences.Default.ContainsKey("background_image");

        RefreshUI();
    }

}