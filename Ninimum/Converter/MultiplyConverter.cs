using System.Globalization;

namespace Ninimum.Converters;

public class MultiplyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return 0d;

        if (!double.TryParse(value.ToString(), out double number))
            return 0d;

        if (!double.TryParse(parameter.ToString(), out double factor))
            return 0d;

        return number * factor;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}