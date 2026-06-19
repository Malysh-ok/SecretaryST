using System.Windows.Data;
using System.Windows.Markup;
using Common.WpfModule.Ui.Converters;

namespace Common.WpfModule.Ui.Markups;

/// <summary>
/// MarkupExtension для динамического задания отступов (Margin) элемента в зависимости от его позиции среди видимых элементов в родительской панели.
/// Возвращает Binding с конвертером, который вычисляет Thickness на основе того, является ли элемент первым, последним или средним видимым.
/// Используется для установки свойства Margin у элементов внутри StackPanel, WrapPanel и других панелей.
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
/// <code>
/// &lt;Button Margin="{markups:FirstLastItemMargin LeftFirst=10, RightLast=0, MiddleLeft=2, MiddleRight=2}" /&gt;
/// </code>
/// </example>
// ReSharper disable once ClassNeverInstantiated.Global
public class FirstLastItemMarginExtension : MarkupExtension
{
    public double LeftFirst { get; set; }
    public double RightLast { get; set; }
    public double MiddleLeft { get; set; }
    public double MiddleRight { get; set; }

    /// <summary>
    /// Конструктор.
    /// </summary>
    public FirstLastItemMarginExtension()
    {
    }
    
    /// <summary>
    /// Конструктор с параметрами.
    /// </summary>
    public FirstLastItemMarginExtension(double leftFirst, double rightLast, double middleLeft, double middleRight)
    {
        LeftFirst = leftFirst;
        RightLast = rightLast;
        MiddleLeft = middleLeft;
        MiddleRight = middleRight;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var converter = new FirstLastItemMarginConverter
        {
            LeftFirst = LeftFirst,
            RightLast = RightLast,
            MiddleLeft = MiddleLeft,
            MiddleRight = MiddleRight
        };

        var binding = new Binding
        {
            RelativeSource = new RelativeSource(RelativeSourceMode.Self),
            Converter = converter
        };

        // Возвращаем Binding через ProvideValue (это работает для свойств типа Thickness)
        return binding.ProvideValue(serviceProvider);
    }
}