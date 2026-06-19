using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Common.WpfModule.Ui.Markups;

/// <summary>
/// Расширение разметки для автоматического добавления отступов между столбцами Grid.
/// </summary>
/// <remarks>
/// Использование в XAML:
/// &lt;Grid Style="{local:GridColumnSpacing 10}"&gt;
///     &lt;Grid.ColumnDefinitions&gt;
///         &lt;ColumnDefinition Width="Auto"/&gt;
///         &lt;ColumnDefinition Width="*"/&gt;
///     &lt;/Grid.ColumnDefinitions&gt;
///     &lt;TextBlock Grid.Column="0" Text="Метка:"/&gt;
///     &lt;TextBox Grid.Column="1"/&gt;
/// &lt;/Grid&gt;
/// </remarks>
public class GridColumnSpacingExtension : MarkupExtension
{
    private readonly double _spacing;

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="spacing">Отступ между столбцами в пикселях (добавляется справа от элемента, кроме последнего столбца)</param>
    public GridColumnSpacingExtension(double spacing)
    {
        _spacing = spacing;
    }

    /// <summary>
    /// Возвращает Style для Grid, который при загрузке добавит отступы всем дочерним элементам.
    /// </summary>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        // Создаём стиль для Grid
        var style = new Style(typeof(Grid));
        
        // Подписываемся на событие Loaded через EventSetter
        // Когда Grid загрузится, будет вызван метод OnGridLoaded
        style.Setters.Add(new EventSetter(FrameworkElement.LoadedEvent, new RoutedEventHandler(OnGridLoaded)));
        
        return style;
    }

    /// <summary>
    /// Обработчик события Loaded для Grid.
    /// </summary>
    private void OnGridLoaded(object sender, RoutedEventArgs e)
    {
        var grid = (Grid)sender;
        var children = grid.Children;
        
        // Определяем индекс последней колонки
        var lastColumnIndex = grid.ColumnDefinitions.Count - 1;

        // Проходим по всем дочерним элементам Grid
        for (var i = 0; i < children.Count; i++)
        {
            var child = children[i] as FrameworkElement;
            if (child == null) continue;

            // Получаем колонку и сколько колонок занимает элемент
            var column = Grid.GetColumn(child);
            var columnSpan = Grid.GetColumnSpan(child);
            
            // Вычисляем последнюю колонку, которую занимает элемент
            var lastOccupiedColumn = column + columnSpan - 1;
            
            // Добавляем отступ справа только если элемент НЕ заканчивается в последней колонке
            // ReSharper disable once InvertIf
            if (lastOccupiedColumn != lastColumnIndex)
            {
                var margin = child.Margin;
                child.Margin = new Thickness(
                    margin.Left,        // левый отступ сохраняется
                    margin.Top,         // верхний отступ сохраняется
                    _spacing,      // правый отступ = заданному расстоянию
                    margin.Bottom       // нижний отступ сохраняется
                );
            }
        }
    }
}