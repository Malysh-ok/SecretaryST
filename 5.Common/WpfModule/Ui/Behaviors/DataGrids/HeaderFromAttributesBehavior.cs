using System.ComponentModel;
using System.Windows.Controls;
using Common.BaseComponents.Components.Attributes;
using Microsoft.Xaml.Behaviors;

namespace Common.WpfModule.Ui.Behaviors.DataGrids;

/// <summary>
/// Бихейвер. Использует атрибуты "ColumnName" в качестве названия столбцов при их автогенерации в DataGrid.
/// </summary>
public class HeaderFromAttributesBehavior : Behavior<DataGrid>
{
    /// <inheritdoc />
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.AutoGeneratingColumn += Grid_AutoGeneratingColumn;
    }
    
    /// <inheritdoc />
    protected override void OnDetaching()
    {
        base.OnAttached();
        AssociatedObject.AutoGeneratingColumn -= Grid_AutoGeneratingColumn;
    }

    /// <summary>
    /// Обработчик события автогенерации столбцов.
    /// </summary>
    private static void Grid_AutoGeneratingColumn(object? sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        var desc = e.PropertyDescriptor as PropertyDescriptor;
        if(desc?.Attributes[typeof(ColumnNameAttribute)] is ColumnNameAttribute att)
        {
            e.Column.Header = att.Name;
        }
    }
    
    /*
    private void ScrollViewerLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        var property = AssociatedObject.GetType().GetProperty("ScrollInfo", BindingFlags.NonPublic | BindingFlags.Instance);
        property.SetValue(AssociatedObject, new ScrollInfoAdapter((IScrollInfo)property.GetValue(AssociatedObject)));
    }
    
        private static void Grid_Changed(object sender, RoutedEventArgs routedEventArgs)
    {
        var grid = (DataGrid)sender!;
        foreach (var column in grid.Columns)
        {
            // column.MinWidth = column.ActualWidth;
            column.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
            // column.Width = DataGridLength.Auto;
        }
    }

    */
}