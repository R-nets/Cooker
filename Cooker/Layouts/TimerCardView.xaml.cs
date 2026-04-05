using Cooker.Models;

namespace Cooker.Layouts;

public partial class TimerCardView : ContentView
{
    public event EventHandler<StepTimerModel>? StartTimerClicked;
    public event EventHandler<StepTimerModel>? DeleteTimerClicked;

    public TimerCardView()
    {
        InitializeComponent();
    }

    public string DisplayTime
    {
        get
        {
            if (BindingContext is not StepTimerModel timer)
                return "Timer: 0 second(s)";

            int totalSeconds = timer.TimerSeconds;

            int hours = totalSeconds / 3600;
            int minutes = (totalSeconds % 3600) / 60;
            int seconds = totalSeconds % 60;

            List<string> parts = [];

            if (hours > 0)
                parts.Add($"{hours} hour(s)");

            if (minutes > 0)
                parts.Add($"{minutes} minute(s)");

            if (seconds > 0 || parts.Count == 0)
                parts.Add($"{seconds} second(s)");

            return "Timer: " + string.Join(", ", parts);
        }
    }

    void StartButton_Clicked(object sender, EventArgs e)
    {
        if (BindingContext is StepTimerModel timer)
        {
            StartTimerClicked?.Invoke(this, timer);
        }
    }

    void DeleteButton_Clicked(object sender, EventArgs e)
    {
        if (BindingContext is StepTimerModel timer)
        {
            DeleteTimerClicked?.Invoke(this, timer);
        }
    }
}