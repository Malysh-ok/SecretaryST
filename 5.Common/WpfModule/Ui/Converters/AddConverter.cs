using System.Globalization;
using System.Windows.Data;
// ReSharper disable InvalidXmlDocComment

namespace Common.WpfModule.Ui.Converters;

/// <summary>
/// Конвертер для прибавления значения к числовому свойству.
/// </summary>
public class AddConverter : IValueConverter
{
    /// <summary>
    /// Прибавляет указанное число к входному значению.
    /// </summary>
    /// <param name="value">Исходное число.</param>
    /// <param name="parameter">Число, которое нужно прибавить (например, "1").</param>
    /// <returns>value + parameter.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int intValue)
            return "0";

        var add = parameter != null ? System.Convert.ToInt32(parameter) : 0;
        return (intValue + add).ToString();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}