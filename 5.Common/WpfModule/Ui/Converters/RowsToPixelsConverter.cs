using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Common.BaseExtensions;

namespace Common.WpfModule.Ui.Converters;

/// <summary>
/// Конвертер для преобразования количества строк в высоту в пикселях.
/// Подходит для DataGrid, ListView, ItemsControl и других контролов с ItemsSource.
/// Количество строк берётся из ConverterParameter.
/// </summary>
/// <remarks>
/// Использование в XAML:
/// MinHeight="{Binding RelativeSource={RelativeSource Self}, 
///            Converter={StaticResource RowsToPixelsConverter}, 
///            ConverterParameter=5}"
/// 
/// Пример с указанием высоты строки:
/// &lt;local:RowsToPixelsConverter x:Key="RowsToPixelsConverter" DefaultRowHeight="40"/&gt;
/// </remarks>
public class RowsToPixelsConverter : IValueConverter
{
    /// <summary>
    /// Высота одной строки в пикселях (по умолчанию 35).
    /// Используется, если не удалось получить высоту строки из контрола.
    /// </summary>
    public double DefaultRowHeight { get; set; } = 25;

    /// <summary>
    /// Имя свойства, содержащего высоту строки (по умолчанию "RowHeight").
    /// Для DataGrid это "RowHeight", для ListView/ListBox - "ItemHeight".
    /// </summary>
    public string RowHeightPropertyName { get; set; } = "RowHeight";

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Получаем количество строк из ConverterParameter
        if (parameter == null) 
            return DependencyProperty.UnsetValue;
        var rows = parameter.ToString().ParseToInt();
        if (rows <= 0)
            return DependencyProperty.UnsetValue;
        
        // value должен быть ItemsControl (DataGrid, ListView, ListBox и т.д.)
        if (value is ItemsControl itemsControl)
        {
            // Получаем высоту одной строки
            double rowHeight = GetRowHeight(itemsControl);
            
            // Возвращаем высоту в пикселях
            return rows * rowHeight;
        }

        // Если value не ItemsControl
        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
    
    /// <summary>
    /// Получает высоту одной строки из контрола.
    /// </summary>
    private double GetRowHeight(ItemsControl control)
    {
        // 1. Пробуем получить высоту из указанного свойства (RowHeight, ItemHeight и т.д.)
        var heightProperty = control.GetType().GetProperty(RowHeightPropertyName);
        if (heightProperty != null)
        {
            double? value = heightProperty.GetValue(control) as double?;
            if (value.HasValue && value.Value > 0)
                return value.Value;
        }
        
        // 2. Пробуем получить высоту из стиля элемента (ItemContainerStyle)
        if (control.ItemContainerStyle != null)
        {
            foreach (var setterBase in control.ItemContainerStyle.Setters)
            {
                var setter = (Setter)setterBase;
                if (setter.Property.Name == "Height" && setter.Value is double height && height > 0)
                    return height;
                if (setter.Property.Name == "MinHeight" && setter.Value is double minHeight && minHeight > 0)
                    return minHeight;
            }
        }
        
        // 3. Пробуем получить высоту из шаблона элемента (ItemTemplate)
        // Создаём временный элемент для измерения
        // ReSharper disable once InvertIf
        if (control.ItemTemplate?.LoadContent() is FrameworkElement tempElement)
        {
            tempElement.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            if (tempElement.DesiredSize.Height > 0)
                return tempElement.DesiredSize.Height;
        }

        // 4. Значение по умолчанию
        return DefaultRowHeight;
    }
}