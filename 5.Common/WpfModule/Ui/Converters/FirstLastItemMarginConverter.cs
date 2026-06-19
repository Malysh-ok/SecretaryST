using System.Windows.Controls;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Common.WpfModule.Ui.Converters;

/// <summary>
/// Конвертер, возвращающий Thickness для элемента в зависимости от его позиции среди видимых элементов в родительской панели (Panel).
/// Первый видимый элемент получает левый отступ = LeftFirst, последний видимый – правый отступ = RightLast.
/// Остальные элементы получают MiddleLeft и MiddleRight.
/// </summary>
/// <remarks>
/// Параметры:
/// <list type="table">
/// <item><term>LeftFirst</term><description>Отступ слева для первого видимого элемента (по умолчанию 0).</description></item>
/// <item><term>RightLast</term><description>Отступ справа для последнего видимого элемента (по умолчанию 0).</description></item>
/// <item><term>MiddleLeft</term><description>Отступ слева для средних видимых элементов (по умолчанию 0).</description></item>
/// <item><term>MiddleRight</term><description>Отступ справа для средних видимых элементов (по умолчанию 0).</description></item>
/// </list>
/// </remarks>
/// <example>
/// Регистрация конвертера в ресурсах:
/// <code>
/// &lt;converters:FirstLastItemMarginConverter x:Key="MyMarginConverter"
///     LeftFirst="10" RightLast="0" MiddleLeft="2" MiddleRight="2"/&gt;
/// </code>
/// <br/>Использование в Binding для свойства Margin:
/// <code>
/// &lt;Button Margin="{Binding RelativeSource={RelativeSource Self},
///                            Converter={StaticResource MyMarginConverter}}"/&gt;
/// </code>
/// </example>
public class FirstLastItemMarginConverter : IValueConverter
{
    public double LeftFirst { get; set; }
    public double RightLast { get; set; }
    public double MiddleLeft { get; set; }
    public double MiddleRight { get; set; }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is FrameworkElement element)
        {
            var panel = VisualTreeHelper.GetParent(element) as Panel;
            if (panel != null)
            {
                // Собираем все видимые элементы
                var visibleChildren = new List<FrameworkElement>();
                foreach (var child in panel.Children)
                {
                    if (child is FrameworkElement fe && fe.Visibility == Visibility.Visible)
                        visibleChildren.Add(fe);
                }

                if (visibleChildren.Count == 0)
                    return new Thickness(0);

                // Определяем позицию текущего элемента
                int index = visibleChildren.IndexOf(element);
                if (index == 0) // первый
                    return new Thickness(LeftFirst, 0, MiddleRight, 0);
                else if (index == visibleChildren.Count - 1) // последний
                    return new Thickness(MiddleLeft, 0, RightLast, 0);
                else // средний
                    return new Thickness(MiddleLeft, 0, MiddleRight, 0);
            }
        }
        return new Thickness(MiddleLeft, 0, MiddleRight, 0);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
