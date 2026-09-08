using System.Globalization;
using System.Windows.Data;
// ReSharper disable InvalidXmlDocComment

namespace Common.WpfModule.Ui.Converters;

/// <summary>
/// Конвертер для преобразования bool в Opacity.
/// </summary>
public class BoolToOpacityConverter : IValueConverter
{
    /// <summary>
    /// Конвертирует bool в double (Opacity).
    /// </summary>
    /// <param name="value">Исходное значение (bool).</param>
    /// <param name="parameter">Параметр для указания значения false.<br/>
    /// Если не указан - 0.5.
    /// </param>
    /// <returns>1.0 если true, параметр или 0.5 если false.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is true)
            return 1.0;
        
        if (parameter is string param && double.TryParse(param, NumberStyles.Any, 
                CultureInfo.InvariantCulture, out var falseOpacity))
            return falseOpacity;
        
        return 0.5;
    }
    
    /// <summary>
    /// Обратное преобразование не поддерживается.
    /// </summary>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}