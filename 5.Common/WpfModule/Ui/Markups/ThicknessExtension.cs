using System.Windows;
using System.Windows.Markup;
// ReSharper disable MemberCanBePrivate.Global

namespace Common.WpfModule.Ui.Markups;

/// <summary>
/// MarkupExtension для создания Thickness из четырёх чисел (left, top, right, bottom).
/// Позволяет задавать отступы в XAML без использования конструктора Thickness с параметрами.
/// </summary>
/// <remarks>
/// Поддерживает как позиционный синтаксис (5,10,5,10), так и именованные параметры (Left=5, Top=10, Right=5, Bottom=10).<br/>
/// Если один из параметров не задан, будет подставлен 0.
/// </remarks>
/// <example>
/// <code>
/// &lt;Button Margin="{local:Thickness 5,10,5,10}" /&gt;
/// &lt;Button Margin="{local:Thickness Left=5, Top=10, Right=5, Bottom=10}" /&gt;
/// </code>
/// </example>
public class ThicknessExtension : MarkupExtension
{
    public double Left { get; set; }
    public double Top { get; set; }
    public double Right { get; set; }
    public double Bottom { get; set; }

    /// <summary>
    /// Конструктор без параметров (обязателен для использования в XAML).
    /// </summary>
    public ThicknessExtension() { }

    /// <summary>
    /// Конструктор с параметрами.
    /// </summary>
    public ThicknessExtension(double? left, double? top, double? right, double? bottom)
    {
        Left = left ?? 0;
        Top = top ?? 0;
        Right = right ?? 0;
        Bottom = bottom ?? 0;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return new Thickness(Left, Top, Right, Bottom);
    }
}