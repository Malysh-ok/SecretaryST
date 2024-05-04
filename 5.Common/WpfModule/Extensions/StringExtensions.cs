using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace Common.WpfModule.Extensions;

/// <summary>
/// Методы-расширения для <see cref="string" />.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Получить размеры строки.
    /// </summary>
    /// <param name="text">Исходная строка.</param>
    /// <param name="fontFamily">Семейство шрифтов.</param>
    /// <param name="fontSize">Размер шрифта.</param>
    /// <param name="visual"></param>
    /// <returns></returns>
    public static Size GetSize(this string text, string fontFamily, float fontSize, Visual? visual = null)
    {
        // TODO: Протестировать в WPF-окнах
        var pixelsPerDip = visual == null   // масштабный пиксельный коэффициент
            ? 1.0
            : VisualTreeHelper.GetDpi(visual).PixelsPerDip;
        var formatted = new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, 
            new Typeface(fontFamily), fontSize, 
            null, pixelsPerDip);

        return new Size(formatted.Width, formatted.Height);
    }
}