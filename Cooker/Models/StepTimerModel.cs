using SQLite;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Cooker.Models;

[Table("StepTimers")]
public partial class StepTimerModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    void OnPropertyChanged([CallerMemberName] string name = null!)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int RecipeId { get; set; }

    public int StepIndex { get; set; }

    public string StepDescription { get; set; } = "";

    public int TimerSeconds { get; set; }

    int remainingSeconds;
    public int RemainingSeconds
    {
        get => remainingSeconds;
        set
        {
            if (remainingSeconds != value)
            {
                remainingSeconds = value;
                OnPropertyChanged(nameof(RemainingSeconds));
                OnPropertyChanged(nameof(RemainingDisplay));
            }
        }
    }

    public string RemainingDisplay =>
    $"{RemainingSeconds / 3600:D2}:{(RemainingSeconds % 3600) / 60:D2}:{RemainingSeconds % 60:D2}";

    public bool IsCompleted { get; set; }

    bool isRunning;
    public bool IsRunning
    {
        get => isRunning;
        set
        {
            if (isRunning == value) return;
            isRunning = value;
            OnPropertyChanged();
        }
    }

    public bool IsPaused { get; set; }
}