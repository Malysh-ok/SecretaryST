using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Common.WpfModule.Ui.Converters;

/// <summary>
/// Конвертирует bool в FlowDirection.<br/>
/// true -> LeftToRight (по умолчанию), false -> RightToLeft.
/// </summary>
public class BoolToFlowDirectionConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true 
            ? FlowDirection.LeftToRight 
            : FlowDirection.RightToLeft;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}