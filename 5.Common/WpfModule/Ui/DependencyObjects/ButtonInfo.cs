using System.Windows;
using System.Windows.Input;

namespace Common.WpfModule.Ui.DependencyObjects;

/// <summary>
/// Представляет модель данных для кнопки, которая может быть динамически создана в любом месте интерфейса.
/// </summary>
/// <remarks>
/// Содержит имя команды (для последующего поиска в DataContext), отображаемое содержимое (текст, изображение),
/// всплывающую подсказку и флаг видимости. Используется для декларативного описания кнопок
/// без необходимости создавать их в коде или привязывать команду напрямую.
/// </remarks>
public class ButtonInfo : DependencyObject
{
    /// <summary>
    /// Команда для биндинга.
    /// </summary>
    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }
    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(ButtonInfo),
            new PropertyMetadata(null));
    
    /// <summary>
    /// Имя команды для биндинга.
    /// </summary>
    /// <remarks>
    /// REMARK: Устарело. Используйте <see cref="Command"/>.
    /// </remarks>
    [Obsolete($"Используйте {nameof(Command)} вместо данного свойства.")]
    public string CommandName
    {
        get => (string)GetValue(CommandNameProperty);
        set => SetValue(CommandNameProperty, value);
    }
    [Obsolete("Obsolete")]
    public static readonly DependencyProperty CommandNameProperty =
        DependencyProperty.Register(nameof(CommandName), typeof(string), typeof(ButtonInfo), 
            new PropertyMetadata(null));
    
    /// <summary>
    /// Содержимое кнопки заголовка (может быть строка, изображение, любой объект).
    /// </summary>
    public object Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }
    public static readonly DependencyProperty ContentProperty =
        DependencyProperty.Register(nameof(Content), typeof(object), typeof(ButtonInfo),
            new PropertyMetadata(null));

    /// <summary>
    /// Иконка кнопки заголовка.
    /// </summary>
    public object Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(object), typeof(ButtonInfo),
            new PropertyMetadata(null));

    /// <summary>
    /// Всплывающая подсказка.
    /// </summary>
    public object ToolTip
    {
        get => GetValue(ToolTipProperty);
        set => SetValue(ToolTipProperty, value);
    }
    public static readonly DependencyProperty ToolTipProperty =
        DependencyProperty.Register(nameof(ToolTip), typeof(object), typeof(ButtonInfo),
            new PropertyMetadata(null));

    /// <summary>
    /// Признак видимости кнопки заголовка.
    /// </summary>
    public bool IsVisible
    {
        get => (bool)GetValue(IsVisibleProperty);
        set => SetValue(IsVisibleProperty, value);
    }
    public static readonly DependencyProperty IsVisibleProperty =
        DependencyProperty.Register(nameof(IsVisible), typeof(bool), typeof(ButtonInfo),
            new PropertyMetadata(true));
}