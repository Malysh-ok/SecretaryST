using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Common.BaseExtensions;
using Common.WpfModule.Extensions;

namespace Common.WpfModule.Ui.Converters;

/// <summary>
/// Универсальный конвертер для получения индекса элемента в ItemsControl.
/// Поддерживает DataGrid, ListView, ListBox, ComboBox и другие контролы с ItemsSource.
/// </summary>
/// <remarks>
/// Использование в XAML:<br/>
/// 
/// 1. Для DataGrid:<br/>
///    <Setter Property="Text" Value="{Binding RelativeSource={RelativeSource AncestorType=DataGridRow}, 
///                                            Converter={StaticResource RowIndexConverter}}"/>.<br/>
/// 
/// 2. Для ListView/ListBox:<br/>
///    <Setter Property="Text" Value="{Binding RelativeSource={RelativeSource AncestorType=ListViewItem}, 
///                                            Converter={StaticResource RowIndexConverter}}"/>.<br/>
/// 
/// 3. С параметром (начальное значение):<br/>
///    ConverterParameter=0 - нумерация с 0<br/>
///    ConverterParameter=1 - нумерация с 1 (по умолчанию).<br/>
/// </remarks>
public class RowIndexConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Смещение (по умолчанию 1)
        var offset = parameter?.ToString().ParseToInt(1) ?? 1;
        
        var index = GetIndex(value);
        
        if (index.HasValue)
            return index.Value + offset;
        
        return 0;
    }
    
    /// <summary>
    /// Получение индекса.
    /// </summary>
    private int? GetIndex(object? value)
    {
        // 1. DataGridRow
        if (value is DataGridRow dataGridRow)
        {
            return dataGridRow.GetIndex();
        }
        
        // 2. ListViewItem, ListBoxItem, ComboBoxItem - через ItemContainerGenerator
        if (value is FrameworkElement element)
        {
            var itemsControl = element.TryFindParent<ItemsControl>();

            {
                // Получаем индекс через ItemContainerGenerator
                var index = itemsControl?.ItemContainerGenerator.IndexFromContainer(element);
                if (index >= 0)
                    return index;
            }
        }
        
        // 3. Если value сам является индексом
        if (value is int intValue && intValue >= 0)
            return intValue;
        
        return null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}