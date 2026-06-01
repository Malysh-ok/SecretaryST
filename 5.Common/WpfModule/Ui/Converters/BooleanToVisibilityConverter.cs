using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Common.WpfModule.Ui.Converters;

/// <summary>
/// Преобразует логическое значение в значение видимости (<see cref="Visibility" />).
/// </summary>
[ValueConversion(typeof(bool), typeof(Visibility))]
public class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is Visibility.Visible;
    }
}