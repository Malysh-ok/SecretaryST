using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Common.WpfModule.Ui.Converters;

/// <summary>
/// Конвертер для привязки Foreground: возвращает указанную кисть для null,
/// и системный цвет текста для любых других значений.
/// </summary>
public class NullToSystemBrushConverter : IValueConverter
{
    /// <summary>
    /// Кисть, которая будет возвращена, если значение равно null.
    /// По умолчанию — серый цвет (Brushes.Gray).
    /// Можно задать в XAML при регистрации конвертера.
    /// </summary>
    public Brush NullBrush { get; set; }

    /// <summary>
    /// Кисть, которая будет возвращена, если значение не равно null.
    /// По умолчанию — системный цвет текста (SystemColors.WindowTextBrush).
    /// </summary>
    public Brush DefaultBrush { get; set; } = SystemColors.WindowTextBrush;

    public NullToSystemBrushConverter()
    {
        // Получаем переопределённую кисть из ресурсов приложения
        // (если она переопределена в App.xaml или в словарях окна)
        var brush = Application.Current.TryFindResource(SystemColors.GrayTextBrushKey) as Brush;
        NullBrush = brush ?? SystemColors.GrayTextBrush;
    }
    
    /// <summary>
    /// Преобразует значение в кисть.
    /// </summary>
    /// <param name="value">Значение для проверки (обычно Key из KeyValuePair).</param>
    /// <param name="targetType">Не используется.</param>
    /// <param name="parameter">Не используется.</param>
    /// <param name="culture">Не используется.</param>
    /// <returns>
    /// NullBrush, если value == null.
    /// Иначе DefaultBrush.
    /// </returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value == null ? NullBrush : DefaultBrush;
    }

    /// <summary>
    /// Обратное преобразование не поддерживается.
    /// </summary>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}