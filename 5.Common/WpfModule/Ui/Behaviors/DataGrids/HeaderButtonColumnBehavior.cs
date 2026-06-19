using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using Common.BaseExtensions;
using Common.WpfModule.Ui.Controls;
using Microsoft.Xaml.Behaviors;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Common.WpfModule.Ui.Behaviors.DataGrids;

/// <summary>
/// Бихейвер (поведение) для DataGrid, добавляющее редактируемую колонку с кнопкой в заголовке.
/// Вся визуальная часть (иконка, размеры, отступы, выравнивание) задаётся в XAML-стилях
/// и передаётся в поведение через соответствующие свойства.
/// </summary>
/// <remarks>
/// Бихейвер не содержит жёстко заданных строк, размеров или визуальных элементов.
/// Оно лишь создаёт необходимые шаблоны и устанавливает привязки.<br/><br/>
/// Устарел. Используйте <see cref="DataGridBoundTemplateColumn"/> вместо данного бихейвера.
/// </remarks>
[Obsolete($"Используйте {nameof(DataGridBoundTemplateColumn)} вместо данного бихейвера.")]
public class HeaderButtonColumnBehavior : Behavior<DataGrid>
{
    /// <summary>
    /// Имя команды в ViewModel, которая будет вызвана при нажатии на кнопку в заголовке.
    /// Команда должна быть доступна через DataContext DataGrid.
    /// </summary>
    public string? CommandName { get; set; }

    /// <summary>
    /// Имя свойства элемента коллекции <see cref="DataGrid.ItemsSource"/>, к которому выполняется привязка.
    /// Используется для отображения значения (односторонняя привязка) и редактирования (двусторонняя).
    /// </summary>
    public string? BindingPath { get; set; }

    /// <summary>
    /// Текст, отображаемый в заголовке колонки (слева от кнопки).
    /// </summary>
    public string HeaderText { get; set; } = string.Empty;
    
    /// <summary>
    /// Содержимое кнопки.
    /// </summary>
    public string? ButtonContent { get; set; }
        
    /// <summary>
    /// Признак того, что текст расположен справа от кнопки.
    /// </summary>
    public bool IsTextOnRight { get; set; } = true;

    /// <summary>
    /// Стиль для кнопки действия.
    /// Определяет ширину, высоту, выравнивание и т.п.
    /// </summary>
    public Style? ButtonStyle { get; set; }

    /// <summary>
    /// Стиль для TextBlock в режиме отображения.
    /// Задаёт вертикальное выравнивание, отступы и прочее.
    /// </summary>
    public Style? CellTextStyle { get; set; }

    /// <summary>
    /// Стиль для TextBox в режиме редактирования.
    /// Обычно задаёт BorderThickness="0", Background="Transparent", выравнивание.
    /// </summary>
    public Style? CellEditStyle { get; set; }

    /// <summary>
    /// Логическая ширина колонки. По умолчанию - "*".
    /// </summary>
    public DataGridLength ColumnWidth { get; set; } = new DataGridLength(1, DataGridLengthUnitType.Star);

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.Loaded += OnLoaded;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.Loaded -= OnLoaded;
        base.OnDetaching();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Находим команду в DataContext DataGrid по её имени.
        ICommand? command = null;
        var dataContext = AssociatedObject.DataContext;
        if (dataContext != null && !CommandName.IsNullOrWhiteSpace())
        {
            var property = dataContext.GetType().GetProperty(CommandName!);
            if (property != null)
                command = property.GetValue(dataContext) as ICommand;
        }

        // Создаём колонку с шаблонами, которые строятся программно.
        var column = new DataGridTemplateColumn
        {
            // Header = HeaderText,
            HeaderTemplate = CreateHeaderTemplate(command),
            CellTemplate = CreateCellTemplate(),
            CellEditingTemplate = CreateCellEditingTemplate(),
            Width = ColumnWidth,
        };

        AssociatedObject.Columns.Add(column);
    }

    /// <summary>
    /// Создаёт шаблон заголовка: кнопка + текст.
    /// </summary>
    private DataTemplate CreateHeaderTemplate(ICommand? command)
    {
        var template = new DataTemplate();

        // Контейнер – горизонтальный StackPanel, выровненный по центру по вертикали.
        var stackPanel = new FrameworkElementFactory(typeof(StackPanel));
        stackPanel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
        stackPanel.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);

        // Кнопка действия. Её внешний вид полностью определяется переданным стилем.
        var button = new FrameworkElementFactory(typeof(Button));
        if (ButtonContent != null)
            button.SetValue(ContentControl.ContentProperty, ButtonContent);
        if (ButtonStyle != null)
            button.SetValue(FrameworkElement.StyleProperty, ButtonStyle);
        if (command != null)
            button.SetBinding(ButtonBase.CommandProperty, new Binding(".") { Source = command });

        // Текст заголовка.
        FrameworkElementFactory? textBlock = null;
        if (!HeaderText.IsNullOrWhiteSpace())
        {
            textBlock = new FrameworkElementFactory(typeof(TextBlock));
            textBlock.SetValue(TextBlock.TextProperty, HeaderText);
            textBlock.SetValue(FrameworkElement.MarginProperty, new Thickness(8, 0, 8, 0));
            textBlock.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
        }

        if (IsTextOnRight)
        {
            stackPanel.AppendChild(button);
            if (textBlock != null) stackPanel.AppendChild(textBlock);
        }
        else
        {
            if (textBlock != null) stackPanel.AppendChild(textBlock);
            stackPanel.AppendChild(button);
        }
        template.VisualTree = stackPanel;
        
        return template;
    }

    /// <summary>
    /// Создаёт шаблон для отображения ячейки (TextBlock с односторонней привязкой).
    /// </summary>
    private DataTemplate CreateCellTemplate()
    {
        var template = new DataTemplate();
        var textBlock = new FrameworkElementFactory(typeof(TextBlock));
        if (CellTextStyle != null)
            textBlock.SetValue(FrameworkElement.StyleProperty, CellTextStyle);
        
        // Binding, используется указанное имя свойства.
        if (!BindingPath.IsNullOrWhiteSpace())
            textBlock.SetBinding(TextBlock.TextProperty, new Binding(BindingPath));
        
        template.VisualTree = textBlock;
        return template;
    }

    /// <summary>
    /// Создаёт шаблон для редактирования ячейки (TextBox с двухсторонней привязкой).
    /// </summary>
    private DataTemplate CreateCellEditingTemplate()
    {
        var template = new DataTemplate();
        var textBox = new FrameworkElementFactory(typeof(TextBox));
        if (CellEditStyle != null)
            textBox.SetValue(FrameworkElement.StyleProperty, CellEditStyle);
        
        // Binding, используется указанное имя свойства.
        if (! BindingPath.IsNullOrWhiteSpace())
        {
            var binding = new Binding(BindingPath)
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            textBox.SetBinding(TextBox.TextProperty, binding);
        }

        template.VisualTree = textBox;
        return template;
    }
}