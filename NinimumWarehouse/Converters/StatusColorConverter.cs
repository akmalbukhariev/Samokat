using System.Globalization;
namespace NinimumWarehouse.Converters;
public sealed class StatusColorConverter : IValueConverter
{
    public object Convert(object? value,Type targetType,object? parameter,CultureInfo culture)
    {
        var tone = value?.ToString() switch { "READY" => "Success", "BLOCKED" => "Warning", "PICKING" => "Primary", _ => "Muted" };
        return Application.Current!.Resources[tone + (parameter?.ToString()=="background" ? "Soft" : "Color")];
    }
    public object ConvertBack(object? value,Type targetType,object? parameter,CultureInfo culture) => throw new NotSupportedException();
}
