using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Common.WpfModule.Ui.Converters
{
    /// <summary>
    /// Если значение null — возвращает Collapsed, иначе Visible.
    /// Используется для автоматического скрытия кнопок с не заданным Content.
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value == null ? Visibility.Collapsed : Visibility.Visible;

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}