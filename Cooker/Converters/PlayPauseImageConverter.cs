using System.Globalization;

namespace Cooker.Converters;

public class PlayPauseImageConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool isRunning = value is bool b && b;
        return isRunning ? "pause.png" : "play.png";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}