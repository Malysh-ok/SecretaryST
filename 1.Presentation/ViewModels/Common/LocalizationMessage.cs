using AppDomain.Setting.Entities;

namespace Presentation.ViewModels.Common;

/// <summary>
/// Сообщение, рассылаемое представлениям (окнам) при локализации приложения.
/// </summary>
public class LocalizationMessage
{
    /// <summary>
    /// Язык, на который происходит локализация приложения.
    /// </summary>
    public Lang Lang { get; }
    
    /// <summary>
    /// Текущий (старый) язык локализации.
    /// </summary>
    public Lang OldLang { get; }

    /// <summary>
    /// Конструктор, запрещающий создание экземпляра без параметров.
    /// </summary>
    public LocalizationMessage()
    {
        Lang = null!;
        OldLang = null!;
    }

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="lang">Язык, на который происходит локализация приложения.</param>
    /// <param name="oldLang">Текущий (старый) язык локализации.</param>
    public LocalizationMessage(Lang lang, Lang oldLang)
    {
        Lang = lang;
        OldLang = oldLang;
    }
}