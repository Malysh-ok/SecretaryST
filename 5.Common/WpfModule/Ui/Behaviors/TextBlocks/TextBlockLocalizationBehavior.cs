using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Common.WpfModule.Ui.Converters;
using Microsoft.Xaml.Behaviors;

namespace Common.WpfModule.Ui.Behaviors.TextBlocks;

/// <summary>
///     Бихейвер. Динамическая смена языка отображения у элемента <see cref="TextBlock"/> 
///     (переформатирование текста, например, чисел, дат, валют) во время выполнения.
/// </summary>
/// <remarks>
///     <para>
///         Стандартный <see cref="TextBlock"/> не обновляет форматирование текста
///         при изменении свойства <see cref="FrameworkElement.Language"/> после загрузки элемента.
///         Данное поведение решает эту проблему: при изменении свойства <see cref="LanguageTag"/>
///         принудительно обновляет текстовое представление в соответствии с новым языком.
///     </para>
///     <para>
///         Особенно полезно для отображения дат, чисел и валют в многоязычных приложениях,
///         когда язык может быть изменён во время выполнения.
///     </para>
/// </remarks>
public class TextBlockLocalizationBehavior : Behavior<TextBlock>
{
    /// <summary>
    /// Идентификатор свойства зависимостей <see cref="LanguageTag"/>.
    /// </summary>
    public static readonly DependencyProperty LanguageTagProperty =
        DependencyProperty.Register(
            nameof(LanguageTag),
            typeof(string),
            typeof(TextBlockLocalizationBehavior),
            new FrameworkPropertyMetadata(null, OnLanguageCodeChanged));

    /// <summary>
    /// Получает или задает тэг языка (например, "ru-RU", "en-US").
    /// </summary>
    public string LanguageTag
    {
        get => (string)GetValue(LanguageTagProperty);
        set => SetValue(LanguageTagProperty, value);
    }

    /// <summary>
    /// Вызывается при присоединении поведения к элементу <see cref="TextBlock"/>.
    /// </summary>
    protected override void OnAttached()
    {
        base.OnAttached();
        // Инициализация языка при присоединении, если LanguageTag уже задан
        if (!string.IsNullOrEmpty(LanguageTag))
        {
            ApplyLanguage(LanguageTag);
        }
    }

    /// <summary>
    /// Обработчик изменения свойства <see cref="LanguageTag"/>.
    /// </summary>
    private static void OnLanguageCodeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TextBlockLocalizationBehavior behavior && behavior.AssociatedObject != null)
        {
            var newLangCode = e.NewValue as string;
            if (!string.IsNullOrEmpty(newLangCode))
            {
                behavior.ApplyLanguage(newLangCode);
            }
        }
    }

    /// <summary>
    /// Применяет указанный язык к <see cref="TextBlock"/> и обновляет его отображение.
    /// </summary>
    /// <param name="langTag">Тэг языка (например, "ru-RU").</param>
    private void ApplyLanguage(string langTag)
    {
        var textBlock = AssociatedObject;
        if (textBlock == null) return;
        
        // Устанавливаем язык TextBlock через конвертер
        textBlock.Language = new StrToXmlLangConverter().Convert(langTag, null, null, null) as XmlLanguage;

        // Принудительно обновляем отображение текста
        RefreshTextBlockDisplay(textBlock);
    }

    /// <summary>
    /// Обновляет отображение текста в TextBlock.
    /// </summary>
    private static void RefreshTextBlockDisplay(TextBlock textBlock)
    {
        // Сохраняем язык
        var language = textBlock.Language;

        // Сбрасываем язык
        textBlock.Language = XmlLanguage.Empty;

        // Восстанавливаем язык
        textBlock.Language =  language;
    }
}