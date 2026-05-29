using System.Drawing;
using System.Windows.Media;
using Common.BaseExtensions;
using Common.WpfModule.Extensions;
using Brush = System.Windows.Media.Brush;

namespace Presentation.ViewModels.Common;

/// <summary>
/// Данные для статус-бара.
/// </summary>
public class StatusBarData
{
    /// <summary>
    /// Тип текста статус-бара.
    /// </summary>
    public enum StatusBarTextType
    {
        /// <summary>
        /// Информация.
        /// </summary>
        Info = 1,
        
        /// <summary>
        /// Предупреждение.
        /// </summary>
        Warning,
        
        /// <summary>
        /// Ошибка.
        /// </summary>
        Error
    }

    /// <summary>
    /// Максимальная длина текста статус-бара.
    /// </summary>
    public static int MaxTextLength { get; protected set; } = 120;

    /// <summary>
    /// Кисть для информации.
    /// </summary>
    public static Brush InfoBrush { get; protected set; } = new SolidColorBrush(SystemColors.ControlText.ToMediaColor());

    /// <summary>
    /// Кисть для предупреждения.
    /// </summary>
    public static Brush WarningBrush { get; protected set; } = new SolidColorBrush(Colors.MidnightBlue);

    /// <summary>
    /// Кисть для ошибки.
    /// </summary>
    public static Brush ErrorBrush { get; protected set; } = new SolidColorBrush(Colors.Maroon);

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="brush">Кисть текста статус-бара.</param>
    /// <param name="text">Текст для статус-бара.</param>
    /// <param name="maxTextLength">Максимальная длина текста статус-бара.</param>
    public StatusBarData(Brush brush, string? text = null, int? maxTextLength = null)
    {
        MaxTextLength = maxTextLength ?? MaxTextLength;
        Text = text ?? string.Empty;
        Brush = brush;
    }

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="textType">Тип текста.</param>
    /// <param name="text">Текст для статус-бара.</param>
    /// <param name="maxTextLength">Максимальная длина текста статус-бара.</param>
    public StatusBarData(StatusBarTextType textType = StatusBarTextType.Info, string? text = null, int? maxTextLength = null)
    {
        MaxTextLength = maxTextLength ?? MaxTextLength;
        Text = text ?? string.Empty;
        Brush = textType switch
        {
            StatusBarTextType.Info => InfoBrush,
            StatusBarTextType.Warning => WarningBrush,
            StatusBarTextType.Error => ErrorBrush,
            _ => throw new ArgumentOutOfRangeException(nameof(textType), textType, null)
        };
    }

    /// <summary>
    /// Кисть текста статус-бара.
    /// </summary>
    public Brush Brush { get; set; }

    private string _text = string.Empty;
    /// <summary>
    /// Текст статус-бара.
    /// </summary>
    public string Text
    {
        get => _text;
        set => _text = value.Truncate(MaxTextLength, "...");
    }

    /// <summary>
    /// Переустановка кистей статус-бара.
    /// </summary>
    /// <param name="infoBrush">Кисть для информации.</param>
    /// <param name="warningBrush">Кисть для предупреждения.</param>
    /// <param name="errorBrush">Кисть для ошибки.</param>
    public static void ReSet(Brush? infoBrush = null, Brush? warningBrush = null, Brush? errorBrush = null)
    {
        InfoBrush = infoBrush ?? InfoBrush;
        WarningBrush = warningBrush ?? WarningBrush;
        ErrorBrush = errorBrush ?? ErrorBrush;
    }
    
    /// <inheritdoc />
    public override string ToString()
        => Text;
}
