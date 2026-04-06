using System.Collections;
using System.Globalization;

namespace Cooker.Converters;

public class IsEmptyConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is IEnumerable collection)
        {
            return !collection.Cast<object>().Any();
        }
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}