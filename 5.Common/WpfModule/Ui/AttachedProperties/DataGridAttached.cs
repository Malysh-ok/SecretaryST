using System.Windows;
using System.Windows.Input;

namespace Common.WpfModule.Ui.AttachedProperties;

/// <summary>
/// Присоединённые свойства для передачи параметров в DataGrid.
/// </summary>
public static class DataGridAttached
{
    /// <summary>
    /// Команда для кнопки заголовка.
    /// </summary>
    public static readonly DependencyProperty HeaderButtonCommandProperty =
        DependencyProperty.RegisterAttached("HeaderButtonCommand", typeof(ICommand), typeof(DataGridAttached),
            new PropertyMetadata(null));
    /// <inheritdoc cref="HeaderButtonCommandProperty" />
    public static void SetHeaderButtonCommand(DependencyObject obj, ICommand value) => obj.SetValue(HeaderButtonCommandProperty, value);
    /// <inheritdoc cref="HeaderButtonCommandProperty" />
    public static ICommand GetHeaderButtonCommand(DependencyObject obj) => (ICommand)obj.GetValue(HeaderButtonCommandProperty);

    /// <summary>
    /// Признак того, что текст заголовка расположен справа от кнопки.
    /// </summary>
    public static readonly DependencyProperty IsTextOnRightProperty =
        DependencyProperty.RegisterAttached("IsTextOnRight", typeof(bool), typeof(DataGridAttached),
            new PropertyMetadata(true));
    /// <inheritdoc cref="IsTextOnRightProperty" />
    public static void SetIsTextOnRight(DependencyObject obj, bool value) => obj.SetValue(IsTextOnRightProperty, value);
    /// <inheritdoc cref="IsTextOnRightProperty" />
    public static bool GetIsTextOnRight(DependencyObject obj) => (bool)obj.GetValue(IsTextOnRightProperty);
    
    /// <summary>
    /// Текст заголовка.
    /// </summary>
    public static readonly DependencyProperty HeaderTextProperty =
        DependencyProperty.RegisterAttached("HeaderText", typeof(string), typeof(DataGridAttached),
            new PropertyMetadata(null));
    /// <inheritdoc cref="HeaderTextProperty" />
    public static void SetHeaderText(DependencyObject obj, string value) => obj.SetValue(HeaderTextProperty, value);
    /// <inheritdoc cref="HeaderTextProperty" />
    public static string GetHeaderText(DependencyObject obj) => (string)obj.GetValue(HeaderTextProperty);
    
    /// <summary>
    /// Содержимое кнопки заголовка.
    /// </summary>
    public static readonly DependencyProperty ButtonContentProperty =
        DependencyProperty.RegisterAttached("ButtonContent", typeof(string), typeof(DataGridAttached), 
            new PropertyMetadata(string.Empty));
    /// <inheritdoc cref="ButtonContentProperty" />
    public static void SetButtonContent(DependencyObject obj, string value) => obj.SetValue(ButtonContentProperty, value);
    /// <inheritdoc cref="ButtonContentProperty" />
    public static string GetButtonContent(DependencyObject obj) => (string)obj.GetValue(ButtonContentProperty);
    
    /// <summary>
    /// Иконка кнопки заголовка.
    /// </summary>
    public static readonly DependencyProperty ButtonIconProperty =
        DependencyProperty.RegisterAttached("ButtonIcon", typeof(object), typeof(DataGridAttached), 
            new PropertyMetadata(null));
    /// <inheritdoc cref="ButtonIconProperty" />
    public static void SetButtonIcon(DependencyObject obj, object value) => obj.SetValue(ButtonIconProperty, value);
    /// <inheritdoc cref="ButtonIconProperty" />
    public static object GetButtonIcon(DependencyObject obj) => obj.GetValue(ButtonIconProperty);
}