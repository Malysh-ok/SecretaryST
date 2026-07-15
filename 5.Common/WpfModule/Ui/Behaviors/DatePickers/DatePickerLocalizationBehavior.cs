using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using Common.WpfModule.Ui.Converters;
using Microsoft.Xaml.Behaviors;

namespace Common.WpfModule.Ui.Behaviors.DatePickers;

/// <summary>
///     Бихейвер. Динамическая смена языка отображения у элемента <see cref="DatePicker"/> 
///     (текст выбранной даты и всплывающий календарь) во время выполнения.
/// </summary>
/// <remarks>
///     <para>
///         Стандартный <see cref="DatePicker"/> не обновляет внутреннее отображение (текстовое поле и календарь)
///         при изменении свойства <see cref="FrameworkElement.Language"/> после загрузки элемента. Данное поведение
///         решает эту проблему: при изменении свойства <see cref="LanguageTag"/> принудительно обновляет 
///         текстовое представление даты и содержимое выпадающего календаря.
///     </para>
/// </remarks>
public class DatePickerLocalizationBehavior : Behavior<DatePicker>
{
    /// <summary>
    /// Идентификатор свойства зависимостей <see cref="LanguageTag"/>.
    /// </summary>
    public static readonly DependencyProperty LanguageTagProperty =
        DependencyProperty.Register(
            nameof(LanguageTag),
            typeof(string),
            typeof(DatePickerLocalizationBehavior),
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
    /// Вызывается при присоединении поведения к элементу <see cref="DatePicker"/>.
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
        if (d is DatePickerLocalizationBehavior behavior && behavior.AssociatedObject != null)
        {
            var newLangCode = e.NewValue as string;
            if (!string.IsNullOrEmpty(newLangCode))
            {
                behavior.ApplyLanguage(newLangCode);
            }
        }
    }

    /// <summary>
    /// Применяет указанный язык к <see cref="DatePicker"/> и обновляет его отображение.
    /// </summary>
    /// <param name="langTag">Тэг языка (например, "ru-RU").</param>
    private void ApplyLanguage(string langTag)
    {
        var picker = AssociatedObject;
        if (picker == null) return;

        // 1. Устанавливаем язык самого DatePicker (через конвертер)
        picker.Language = new StrToXmlLangConverter().Convert(langTag, null, null, null) as XmlLanguage;

        // 2. Обновляем отображение в текстовом поле и календаре
        RefreshDatePickerDisplay(picker);
    }

    /// <summary>
    /// Обновляет отображение даты в текстовом поле и содержимое календаря.
    /// </summary>
    private static void RefreshDatePickerDisplay(DatePicker picker)
    {
        // Сохраняем текущую дату
        DateTime? currentDate = picker.SelectedDate;

        // Временно сбрасываем и восстанавливаем дату, чтобы перегенерировать текст
        picker.SelectedDate = null;
        picker.SelectedDate = currentDate;

        // Если календарь уже открыт – обновляем его содержимое
        if (picker.IsDropDownOpen)
        {
            UpdateCalendarContent(picker);
        }

        // Подписываемся на открытие календаря (для будущих открытий)
        RoutedEventHandler? handler = null;
        handler = (_, _) =>
        {
            picker.CalendarOpened -= handler;
            UpdateCalendarContent(picker);
        };
        picker.CalendarOpened += handler;
    }

    /// <summary>
    /// Принудительно обновляет содержимое всплывающего календаря.
    /// </summary>
    private static void UpdateCalendarContent(DatePicker picker)
    {
        // Находим всплывающее окно с календарём внутри DatePicker
        if (picker.Template.FindName("PART_Popup", picker) is Popup popup && popup.Child is Calendar cal)
        {
            // Принудительно сбрасываем и заново устанавливаем язык
            cal.ClearValue(Calendar.FirstDayOfWeekProperty);
            cal.Language = XmlLanguage.Empty;
            cal.Language = picker.Language;

            // Вызываем внутренний метод обновления ячеек календаря (через рефлексию)
            var updateMethod = cal.GetType().GetMethod("UpdateCellItems", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            updateMethod?.Invoke(cal, null);
        }
    }
}