using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Common.WpfModule.Ui.Converters;

/// <summary>
/// Конвертер для частичного изменения Thickness (Margin/Padding).
/// Позволяет переопределить только указанные стороны, остальные берутся из исходного значения.
/// </summary>
public class ThicknessModifyConverter : IValueConverter
{
    /// <summary>
    /// Новое значение для левого отступа (null = оставить как есть)
    /// </summary>
    public double? Left { get; set; }

    /// <summary>
    /// Новое значение для верхнего отступа (null = оставить как есть)
    /// </summary>
    public double? Top { get; set; }

    /// <summary>
    /// Новое значение для правого отступа (null = оставить как есть)
    /// </summary>
    public double? Right { get; set; }

    /// <summary>
    /// Новое значение для нижнего отступа (null = оставить как есть)
    /// </summary>
    public double? Bottom { get; set; }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Создаём новый Thickness, подставляя новые значения, если они заданы,
        // иначе сохраняем исходные
        if (value is Thickness original)
        {
            return new Thickness(
                Left ?? original.Left,
                Top ?? original.Top,
                Right ?? original.Right,
                Bottom ?? original.Bottom
            );
        }
        
        // Если исходное значение не Thickness, возвращаем пустой
        return new Thickness(0);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}