using System.Windows;

namespace Common.WpfModule.Ui.AttachedProperties;

/// <summary>
/// Набор присоединённых свойств для гибкой настройки элементов в XAML.
/// Свойства не содержат обратных вызовов – вся логика применения вынесена в стили через триггеры.
/// Это позволяет централизованно управлять внешним видом элементов в ResourceDictionary.
/// </summary>
public static class ElementProperties
{
    // =====================================================
    // Горизонтальное выравнивание (FrameworkElement)
    // =====================================================
    public static readonly DependencyProperty HAlignProperty =
        DependencyProperty.RegisterAttached("HAlign", typeof(HorizontalAlignment), typeof(ElementProperties),
            new PropertyMetadata(HorizontalAlignment.Left));

    public static void SetHAlign(DependencyObject obj, HorizontalAlignment value)
        => obj.SetValue(HAlignProperty, value);
    public static HorizontalAlignment GetHAlign(DependencyObject obj)
        => (HorizontalAlignment)obj.GetValue(HAlignProperty);

    // =====================================================
    // Вертикальное выравнивание (FrameworkElement)
    // =====================================================
    public static readonly DependencyProperty VAlignProperty =
        DependencyProperty.RegisterAttached("VAlign", typeof(VerticalAlignment), typeof(ElementProperties),
            new PropertyMetadata(VerticalAlignment.Top));

    public static void SetVAlign(DependencyObject obj, VerticalAlignment value)
        => obj.SetValue(VAlignProperty, value);
    public static VerticalAlignment GetVAlign(DependencyObject obj)
        => (VerticalAlignment)obj.GetValue(VAlignProperty);

    // =====================================================
    // Горизонтальное выравнивание содержимого (Control)
    // =====================================================
    public static readonly DependencyProperty HContentAlignProperty =
        DependencyProperty.RegisterAttached("HContentAlign", typeof(HorizontalAlignment), typeof(ElementProperties),
            new PropertyMetadata(HorizontalAlignment.Left));

    public static void SetHContentAlign(DependencyObject obj, HorizontalAlignment value)
        => obj.SetValue(HContentAlignProperty, value);
    public static HorizontalAlignment GetHContentAlign(DependencyObject obj)
        => (HorizontalAlignment)obj.GetValue(HContentAlignProperty);

    // =====================================================
    // Вертикальное выравнивание содержимого (Control)
    // =====================================================
    public static readonly DependencyProperty VContentAlignProperty =
        DependencyProperty.RegisterAttached("VContentAlign", typeof(VerticalAlignment), typeof(ElementProperties),
            new PropertyMetadata(VerticalAlignment.Top));

    public static void SetVContentAlign(DependencyObject obj, VerticalAlignment value)
        => obj.SetValue(VContentAlignProperty, value);
    public static VerticalAlignment GetVContentAlign(DependencyObject obj)
        => (VerticalAlignment)obj.GetValue(VContentAlignProperty);

    // =====================================================
    // Жирный шрифт (флаг для триггеров)
    // =====================================================
    public static readonly DependencyProperty BoldProperty =
        DependencyProperty.RegisterAttached("Bold", typeof(bool), typeof(ElementProperties),
            new PropertyMetadata(false));

    public static void SetBold(DependencyObject obj, bool value)
        => obj.SetValue(BoldProperty, value);
    public static bool GetBold(DependencyObject obj)
        => (bool)obj.GetValue(BoldProperty);

    // =====================================================
    // Курсивный шрифт (флаг для триггеров)
    // =====================================================
    public static readonly DependencyProperty ItalicProperty =
        DependencyProperty.RegisterAttached("Italic", typeof(bool), typeof(ElementProperties),
            new PropertyMetadata(false));

    public static void SetItalic(DependencyObject obj, bool value)
        => obj.SetValue(ItalicProperty, value);
    public static bool GetItalic(DependencyObject obj)
        => (bool)obj.GetValue(ItalicProperty);

    // =====================================================
    // Первый элемент (флаг для триггеров)
    // =====================================================
    public static readonly DependencyProperty IsFirstProperty =
        DependencyProperty.RegisterAttached("IsFirst", typeof(bool), typeof(ElementProperties),
            new PropertyMetadata(false));

    public static void SetIsFirst(DependencyObject obj, bool value)
        => obj.SetValue(IsFirstProperty, value);
    public static bool GetIsFirst(DependencyObject obj)
        => (bool)obj.GetValue(IsFirstProperty);

    // =====================================================
    // Последний элемент (флаг для триггеров)
    // =====================================================
    public static readonly DependencyProperty IsLastProperty =
        DependencyProperty.RegisterAttached("IsLast", typeof(bool), typeof(ElementProperties),
            new PropertyMetadata(false));

    public static void SetIsLast(DependencyObject obj, bool value)
        => obj.SetValue(IsLastProperty, value);
    public static bool GetIsLast(DependencyObject obj)
        => (bool)obj.GetValue(IsLastProperty);
}