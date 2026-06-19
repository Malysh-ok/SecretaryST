using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Common.WpfModule.Ui.Controls;

/// <summary>
/// Кастомный заголовок колонки DataGrid с поддержкой до 5 кнопок.
/// Кнопка отображается ТОЛЬКО если её свойство Content задано (не null).
/// Если Content не задан — кнопка автоматически скрывается без дополнительных атрибутов.
/// Все данные (команды, контент, тултипы) задаются в XAML окна.
/// Визуальная структура определяется в стиле (ресурсы).
/// </summary>
public class DataGridColumnHeaderWithButtons : DataGridColumnHeader
{
    #region [---------- КНОПКА 1 ----------]

    /// <summary>
    /// Команда для кнопки заголовка.
    /// </summary>
    public ICommand Button1Command
    {
        get => (ICommand)GetValue(Button1CommandProperty);
        set => SetValue(Button1CommandProperty, value);
    }
    /// <inheritdoc cref="Button1Command" />
    public static readonly DependencyProperty Button1CommandProperty =
        DependencyProperty.Register(nameof(Button1Command), typeof(ICommand), typeof(DataGridColumnHeaderWithButtons),
            new PropertyMetadata(null));
    
    /// <summary>
    /// Содержимое кнопки заголовка.
    /// </summary>
    public object Button1Content
    {
        get => GetValue(Button1ContentProperty);
        set => SetValue(Button1ContentProperty, value);
    }
    /// <inheritdoc cref="Button1Content" />
    public static readonly DependencyProperty Button1ContentProperty =
        DependencyProperty.Register(nameof(Button1Content), typeof(object), typeof(DataGridColumnHeaderWithButtons),
            new PropertyMetadata(null)); // по умолчанию null — кнопка скрыта

    /// <summary>
    /// Всплывающая подсказка для кнопки заголовка.
    /// </summary>
    public object Button1ToolTip
    {
        get => GetValue(Button1ToolTipProperty);
        set => SetValue(Button1ToolTipProperty, value);
    }
    /// <inheritdoc cref="Button1ToolTip" />
    public static readonly DependencyProperty Button1ToolTipProperty =
        DependencyProperty.Register(nameof(Button1ToolTip), typeof(object), typeof(DataGridColumnHeaderWithButtons),
            new PropertyMetadata(null));

    #endregion

    #region [---------- КНОПКА 2 ----------]
    
    /// <inheritdoc cref="Button1Command" />
    public ICommand Button2Command
    {
        get => (ICommand)GetValue(Button2CommandProperty);
        set => SetValue(Button2CommandProperty, value);
    }
    /// <inheritdoc cref="Button1Command" />
    public static readonly DependencyProperty Button2CommandProperty =
        DependencyProperty.Register(nameof(Button2Command), typeof(ICommand), typeof(DataGridColumnHeaderWithButtons),
            new PropertyMetadata(null));
    
    /// <inheritdoc cref="Button1Content" />
    public object Button2Content
    {
        get => GetValue(Button2ContentProperty);
        set => SetValue(Button2ContentProperty, value);
    }
    /// <inheritdoc cref="Button1Content" />
    public static readonly DependencyProperty Button2ContentProperty =
        DependencyProperty.Register(nameof(Button2Content), typeof(object), typeof(DataGridColumnHeaderWithButtons),
            new PropertyMetadata(null));
    
    /// <inheritdoc cref="Button1ToolTip" />
    public object Button2ToolTip
    {
        get => GetValue(Button2ToolTipProperty);
        set => SetValue(Button2ToolTipProperty, value);
    }
    /// <inheritdoc cref="Button1ToolTip" />
    public static readonly DependencyProperty Button2ToolTipProperty =
        DependencyProperty.Register(nameof(Button2ToolTip), typeof(object), typeof(DataGridColumnHeaderWithButtons),
            new PropertyMetadata(null));
    
    #endregion

    #region [---------- КНОПКА 3 ----------]
    
    /// <inheritdoc cref="Button1Command" />
    public ICommand Button3Command
    {
        get => (ICommand)GetValue(Button3CommandProperty);
        set => SetValue(Button3CommandProperty, value);
    }
    /// <inheritdoc cref="Button1Command" />
    public static readonly DependencyProperty Button3CommandProperty =
        DependencyProperty.Register(nameof(Button3Command), typeof(ICommand), typeof(DataGridColumnHeaderWithButtons),
            new PropertyMetadata(null));
    
    /// <inheritdoc cref="Button1Content" />
    public object Button3Content
    {
        get => GetValue(Button3ContentProperty);
        set => SetValue(Button3ContentProperty, value);
    }
    /// <inheritdoc cref="Button1Content" />
    public static readonly DependencyProperty Button3ContentProperty =
        DependencyProperty.Register(nameof(Button3Content), typeof(object), typeof(DataGridColumnHeaderWithButtons),
            new PropertyMetadata(null));
    
    /// <inheritdoc cref="Button1ToolTip" />
    public object Button3ToolTip
    {
        get => GetValue(Button3ToolTipProperty);
        set => SetValue(Button3ToolTipProperty, value);
    }
    /// <inheritdoc cref="Button1ToolTip" />
    public static readonly DependencyProperty Button3ToolTipProperty =
        DependencyProperty.Register(nameof(Button3ToolTip), typeof(object), typeof(DataGridColumnHeaderWithButtons),
            new PropertyMetadata(null));
    
    #endregion

    #region [---------- КНОПКА 4 ----------]

    /// <inheritdoc cref="Button1Command" />
    public ICommand Button4Command
    {
        get => (ICommand)GetValue(Button4CommandProperty);
        set => SetValue(Button4CommandProperty, value);
    }
    /// <inheritdoc cref="Button1Command" />
    public static readonly DependencyProperty Button4CommandProperty =
        DependencyProperty.Register(nameof(Button4Command), typeof(ICommand), typeof(DataGridColumnHeaderWithButtons),
            new PropertyMetadata(null));
    
    /// <inheritdoc cref="Button1Content" />
    public object Button4Content
    {
        get => GetValue(Button4ContentProperty);
        set => SetValue(Button4ContentProperty, value);
    }
    /// <inheritdoc cref="Button1Content" />
    public static readonly DependencyProperty Button4ContentProperty =
        DependencyProperty.Register(nameof(Button4Content), typeof(object), typeof(DataGridColumnHeaderWithButtons),
            new PropertyMetadata(null));
    
    /// <inheritdoc cref="Button1ToolTip" />
    public object Button4ToolTip
    {
        get => GetValue(Button4ToolTipProperty);
        set => SetValue(Button4ToolTipProperty, value);
    }
    /// <inheritdoc cref="Button1ToolTip" />
    public static readonly DependencyProperty Button4ToolTipProperty =
        DependencyProperty.Register(nameof(Button4ToolTip), typeof(object), typeof(DataGridColumnHeaderWithButtons),
            new PropertyMetadata(null));

    #endregion

    #region [---------- КНОПКА 5 ----------]

    /// <inheritdoc cref="Button1Command" />
    public ICommand Button5Command
    {
        get => (ICommand)GetValue(Button5CommandProperty);
        set => SetValue(Button5CommandProperty, value);
    }
    /// <inheritdoc cref="Button1Command" />
    public static readonly DependencyProperty Button5CommandProperty =
        DependencyProperty.Register(nameof(Button5Command), typeof(ICommand), typeof(DataGridColumnHeaderWithButtons),
            new PropertyMetadata(null));
    
    /// <inheritdoc cref="Button1Content" />
    public object Button5Content
    {
        get => GetValue(Button5ContentProperty);
        set => SetValue(Button5ContentProperty, value);
    }
    /// <inheritdoc cref="Button1Content" />
    public static readonly DependencyProperty Button5ContentProperty =
        DependencyProperty.Register(nameof(Button5Content), typeof(object), typeof(DataGridColumnHeaderWithButtons),
            new PropertyMetadata(null));
    
    /// <inheritdoc cref="Button1ToolTip" />
    public object Button5ToolTip
    {
        get => GetValue(Button5ToolTipProperty);
        set => SetValue(Button5ToolTipProperty, value);
    }
    /// <inheritdoc cref="Button1ToolTip" />
    public static readonly DependencyProperty Button5ToolTipProperty =
        DependencyProperty.Register(nameof(Button5ToolTip), typeof(object), typeof(DataGridColumnHeaderWithButtons),
            new PropertyMetadata(null));
    
    #endregion

    #region [---------- РАСПОЛОЖЕНИЕ ТЕКСТА ----------]

    /// <summary>
    /// Определяет, где отображать текст заголовка: справа от кнопок (true) или слева (false).
    /// По умолчанию true — текст справа.
    /// </summary>
    public static readonly DependencyProperty IsTextOnRightProperty =
        DependencyProperty.Register(nameof(IsTextOnRight), typeof(bool), typeof(DataGridColumnHeaderWithButtons),
            new PropertyMetadata(true));
    public bool IsTextOnRight
    {
        get => (bool)GetValue(IsTextOnRightProperty);
        set => SetValue(IsTextOnRightProperty, value);
    }
    
    #endregion
}