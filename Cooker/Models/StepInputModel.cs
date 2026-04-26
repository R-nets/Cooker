using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace Cooker.Models;
public partial class StepInputModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    int stepIndex;
    public int StepIndex
    {
        get => stepIndex;
        set { stepIndex = value; OnPropertyChanged(); }
    }

    string description = "";
    public string Description
    {
        get => description;
        set { description = value; OnPropertyChanged(); }
    }

    string hours = "0";
    public string Hours
    {
        get => hours;
        set { hours = value; OnPropertyChanged(); }
    }

    string minutes = "0";
    public string Minutes
    {
        get => minutes;
        set { minutes = value; OnPropertyChanged(); }
    }

    string seconds = "0";
    public string Seconds
    {
        get => seconds;
        set { seconds = value; OnPropertyChanged(); }
    }

    public int TimerId { get; set; }
}