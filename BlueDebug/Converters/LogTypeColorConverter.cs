using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace BlueDebug.Converters;

public class LogTypeColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.ToString() switch
        {
            "TX" => new SolidColorBrush(Color.Parse("#3B82F6")),
            "RX" => new SolidColorBrush(Color.Parse("#10B981")),
            "SYS" => new SolidColorBrush(Color.Parse("#F59E0B")),
            _ => new SolidColorBrush(Color.Parse("#94A3B8"))
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}