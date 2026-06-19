using System.Globalization;
using System.Windows.Data;

namespace Common.WpfModule.Ui.Converters;

/// <summary>
/// Конвертер, вычисляющий размер на основе минимального из двух входных значений,
/// умноженного на заданный коэффициент. Удобен для масштабирования элементов,
/// например, для приведения иконки к квадратному размеру, вписывающемуся в прямоугольную область.
/// </summary>
/// <remarks>
/// Принимает два значения (например, ширину и высоту родительского элемента),
/// находит меньшее из них и возвращает его, умноженное на <see cref="Ratio"/>.
/// При невозможности преобразования возвращает значение по умолчанию (<see cref="DefaultResultValue"/>).
/// </remarks>
public class MinValueConverter : IMultiValueConverter
{
    private const double DefaultResultValue = 16.0;
    
    /// <summary>
    /// Доля от меньшей стороны кнопки, которую займёт иконка. По умолчанию 0.75 (75%).
    /// </summary>
    public double Ratio { get; set; } = 1;

    /// <summary>
    /// Возвращает размер иконки = Min(ширина, высота) * Ratio.
    /// </summary>
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values is [double w, double h, ..])
            return Math.Min(w, h) * Ratio;
        
        return DefaultResultValue;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}