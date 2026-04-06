using Cooker.Models;

namespace Cooker.Layouts;

public partial class TimerCardView : ContentView
{
    public event EventHandler<StepTimerModel>? StartTimerClicked;
    public event EventHandler<StepTimerModel>? PauseTimerClicked;

    public TimerCardView()
    {
        InitializeComponent();
    }

    public string DisplayTime
    {
        get
        {
            if (BindingContext is not StepTimerModel timer)
                return "Timer: 0s";

            int totalSeconds = timer.TimerSeconds;

            int hours = totalSeconds / 3600;
            int minutes = (totalSeconds % 3600) / 60;
            int seconds = totalSeconds % 60;

            return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
        }
    }

    void MainButton_Clicked(object sender, EventArgs e)
    {
        if (BindingContext is not StepTimerModel timer) return;

        if (!timer.IsRunning)
        {
            StartTimerClicked?.Invoke(this, timer);
        }
        else
        {
            PauseTimerClicked?.Invoke(this, timer);
        }
    }
}