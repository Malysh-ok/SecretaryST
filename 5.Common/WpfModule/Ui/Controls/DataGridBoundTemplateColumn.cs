using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Common.WpfModule.Extensions;
using Common.WpfModule.Ui.Converters;
using Common.WpfModule.Ui.DependencyObjects;

namespace Common.WpfModule.Ui.Controls;

/// <summary>
/// Колонка <see cref="DataGrid"/> с динамической привязкой к указанному свойству (<see cref="BindingPath"/>)
/// и возможностью разместить произвольное количество кнопок в заголовке.
/// Кнопки определяются через коллекцию <see cref="ButtonInfo"/>, их команды программно
/// привязываются после загрузки элемента в визуальное дерево.
/// </summary>
/// <remarks>
/// REMARK: Устарел. Используйте <see cref="DataGridColumnHeaderWithButtons"/> вместо данного класса.
/// </remarks>
[Obsolete($"Используйте {nameof(DataGridColumnHeaderWithButtons)} вместо данного класса.")]
public class DataGridBoundTemplateColumn : DataGridTemplateColumn
{
    #region ---------- Привязка к данным ----------
        
    /// <summary>
    /// Имя свойства элемента коллекции <see cref="DataGrid.ItemsSource"/>, к которому выполняется привязка.
    /// Используется для отображения значения (односторонняя привязка) и редактирования (двусторонняя).
    /// </summary>
    public string BindingPath
    {
        get => (string)GetValue(BindingPathProperty);
        set => SetValue(BindingPathProperty, value);
    }
    public static readonly DependencyProperty BindingPathProperty =
        DependencyProperty.Register(nameof(BindingPath), typeof(string), typeof(DataGridBoundTemplateColumn),
            new PropertyMetadata(null));

    #endregion

    #region ---------- Стили для содержимого ячеек ----------

    /// <summary>
    /// Стиль для TextBlock, используемого в режиме отображения ячейки.
    /// </summary>
    public Style? CellTextStyle
    {
        get => (Style)GetValue(CellTextStyleProperty);
        set => SetValue(CellTextStyleProperty, value);
    }
    public static readonly DependencyProperty CellTextStyleProperty =
        DependencyProperty.Register(nameof(CellTextStyle), typeof(Style), typeof(DataGridBoundTemplateColumn),
            new PropertyMetadata(null));

    /// <summary>
    /// Стиль для TextBox, используемого в режиме редактирования ячейки.
    /// </summary>
    public Style? CellEditStyle
    {
        get => (Style)GetValue(CellEditStyleProperty);
        set => SetValue(CellEditStyleProperty, value);
    }
    public static readonly DependencyProperty CellEditStyleProperty =
        DependencyProperty.Register(nameof(CellEditStyle), typeof(Style), typeof(DataGridBoundTemplateColumn),
            new PropertyMetadata(null));

    #endregion

    #region ---------- Стиль кнопок в заголовке ----------

    /// <summary>
    /// Стиль, определяющий внешний вид кнопок в заголовке (размеры, отступы, оформление).
    /// </summary>
    public Style? HeaderButtonStyle
    {
        get => (Style)GetValue(HeaderButtonStyleProperty);
        set => SetValue(HeaderButtonStyleProperty, value);
    }
    public static readonly DependencyProperty HeaderButtonStyleProperty =
        DependencyProperty.Register(nameof(HeaderButtonStyle), typeof(Style), typeof(DataGridBoundTemplateColumn),
            new PropertyMetadata(null));

    #endregion

    #region ---------- Расположение текста заголовка ----------

    /// <summary>
    /// Определяет, отображать ли текст заголовка справа от кнопок (true) или слева (false).
    /// По умолчанию true – текст справа.
    /// </summary>
    public bool IsTextOnRight
    {
        get => (bool)GetValue(IsTextOnRightProperty);
        set => SetValue(IsTextOnRightProperty, value);
    }
    public static readonly DependencyProperty IsTextOnRightProperty =
        DependencyProperty.Register(nameof(IsTextOnRight), typeof(bool), typeof(DataGridBoundTemplateColumn),
            new PropertyMetadata(true, OnIsTextOnRightChanged));

    private static void OnIsTextOnRightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var column = (DataGridBoundTemplateColumn)d;
        column.UpdateHeaderTemplate();
    }
    
    #endregion

    #region ---------- Коллекция кнопок ----------

    /// <summary>
    /// Коллекция кнопок, отображаемых в заголовке. Каждая кнопка задаётся через ButtonInfo,
    /// который содержит имя команды, содержимое, подсказку и видимость.
    /// </summary>
    public ObservableCollection<ButtonInfo>? HeaderButtons
    {
        get => (ObservableCollection<ButtonInfo>)GetValue(HeaderButtonsProperty);
        set => SetValue(HeaderButtonsProperty, value);
    }
    public static readonly DependencyProperty HeaderButtonsProperty =
        DependencyProperty.Register(nameof(HeaderButtons), typeof(ObservableCollection<ButtonInfo>), typeof(DataGridBoundTemplateColumn),
            new PropertyMetadata(null, OnHeaderButtonsChanged));

    private static void OnHeaderButtonsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var column = (DataGridBoundTemplateColumn)d;
        if (e.OldValue is INotifyCollectionChanged oldColl)
            oldColl.CollectionChanged -= column.OnButtonsCollectionChanged;
        if (e.NewValue is INotifyCollectionChanged newColl)
            newColl.CollectionChanged += column.OnButtonsCollectionChanged;
        column.UpdateHeaderTemplate();
    }

    private void OnButtonsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateHeaderTemplate();
    }
    
    #endregion

    
    /// <summary>
    /// Конструктор, инициализирующий коллекцию кнопок по умолчанию и подписывающийся на её изменения.
    /// </summary>
    public DataGridBoundTemplateColumn()
    {
        HeaderButtons = [];
        ((INotifyCollectionChanged)HeaderButtons).CollectionChanged += OnButtonsCollectionChanged;
    }

    #region ---------- Построение шаблона заголовка ----------

    /// <summary>
    /// Создаёт шаблон заголовка, содержащий кнопки из коллекции HeaderButtons и текст заголовка.
    /// Порядок следования (кнопки слева/справа) управляется свойством IsTextOnRight.
    /// Команды кнопок программно устанавливаются в обработчике Loaded каждой кнопки,
    /// чтобы обеспечить доступ к DataContext DataGrid и извлечь ICommand по имени.
    /// </summary>
    private void UpdateHeaderTemplate()
    {
        var template = new DataTemplate();
        var stackFactory = new FrameworkElementFactory(typeof(StackPanel));
        stackFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
        stackFactory.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);

        // Создаём элементы кнопок на основе HeaderButtons, но добавляем их в нужном порядке
        // Вспомогательный список фабрик кнопок
        var buttonFactories = new List<FrameworkElementFactory>();

        foreach (var btnInfo in HeaderButtons!)
        {
            var buttonFactory = new FrameworkElementFactory(typeof(Button));
            if (HeaderButtonStyle != null)
                buttonFactory.SetValue(FrameworkElement.StyleProperty, HeaderButtonStyle);
            else
            {
                // Минимальные размеры, если стиль не задан
                buttonFactory.SetValue(FrameworkElement.WidthProperty, 16.0);
                buttonFactory.SetValue(FrameworkElement.HeightProperty, 16.0);
                buttonFactory.SetValue(FrameworkElement.MarginProperty, new Thickness(0, 0, 0, 0));
            }

            // Обработка содержимого: если это DrawingImage или BitmapImage, создаём Image и привязываем Source
            if (btnInfo.Content is DrawingImage or BitmapImage)
            {
                // Создаём шаблон для отображения DrawingImage через Image
                var contentTemplate = new DataTemplate();
                var imageFactory = new FrameworkElementFactory(typeof(Image));
                imageFactory.SetBinding(Image.SourceProperty, new Binding(nameof(ButtonInfo.Content))
                {
                    Source = btnInfo
                });
                
                // Создаём конвертер 
                var converter = new MinValueConverter { Ratio = 0.6666667 }; // 16/24 от меньшей стороны

                var multiBinding = new MultiBinding
                {
                    Converter = converter
                };
                multiBinding.Bindings.Add(new Binding("ActualWidth")
                {
                    RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Button), 1) 
                });
                multiBinding.Bindings.Add(new Binding("ActualHeight")
                {
                    RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Button), 1) 
                });
                
                // Один multiBinding для ширины и высоты
                imageFactory.SetBinding(FrameworkElement.WidthProperty, multiBinding);
                imageFactory.SetBinding(FrameworkElement.HeightProperty, multiBinding);
                
                imageFactory.SetValue(Image.StretchProperty, Stretch.Uniform);
                imageFactory.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
                imageFactory.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                contentTemplate.VisualTree = imageFactory;
                buttonFactory.SetValue(ContentControl.ContentTemplateProperty, contentTemplate);
            }
            else
            {
                buttonFactory.SetBinding(ContentControl.ContentProperty, new Binding(nameof(ButtonInfo.Content))
                {
                    Source = btnInfo
                });
            }
            
            // Привязка подсказки и видимости
            buttonFactory.SetBinding(FrameworkElement.ToolTipProperty, new Binding(nameof(ButtonInfo.ToolTip))
            {
                Source = btnInfo
            });
            buttonFactory.SetBinding(UIElement.VisibilityProperty, new Binding(nameof(ButtonInfo.IsVisible))
            {
                Source = btnInfo,
                Converter = new BooleanToVisibilityConverter()
            });

            // Захватываем имя команды в локальную переменную (замыкание)
            var commandName = btnInfo.CommandName;
            // Добавляем обработчик Loaded, который установит команду программно
            buttonFactory.AddHandler(FrameworkElement.LoadedEvent, new RoutedEventHandler((s, _) =>
            {
                var btn = (Button)s;
                var dataGrid = btn.TryFindParent<DataGrid>();
                if (dataGrid != null && !string.IsNullOrEmpty(commandName))
                {
                    var command = dataGrid.DataContext?.GetType().GetProperty(commandName)?.GetValue(dataGrid.DataContext) as ICommand;
                    btn.Command = command;
                }
            }));

            buttonFactories.Add(buttonFactory);
        }

        // Фабрика для текста заголовка (используем свойство Header колонки)
        var textBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
        textBlockFactory.SetBinding(TextBlock.TextProperty, new Binding(nameof(Header)) { Source = this });
        textBlockFactory.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
        // Отступ зависит от положения текста
        textBlockFactory.SetValue(FrameworkElement.MarginProperty,
            IsTextOnRight 
                ? new Thickness(6, 0, 0, 0) 
                : new Thickness(0, 0, 6, 0));

        // Формируем порядок добавления элементов в зависимости от IsTextOnRight
        if (IsTextOnRight)
        {
            // Сначала все кнопки, потом текст
            foreach (var btnFactory in buttonFactories)
                stackFactory.AppendChild(btnFactory);
            stackFactory.AppendChild(textBlockFactory);
        }
        else
        {
            // Сначала текст, потом все кнопки
            stackFactory.AppendChild(textBlockFactory);
            foreach (var btnFactory in buttonFactories)
                stackFactory.AppendChild(btnFactory);
        }

        template.VisualTree = stackFactory;
        HeaderTemplate = template;
    }

    #endregion

    #region ---------- Генерация ячеек для отображения и редактирования ----------

    /// <summary>
    /// Создаёт элемент для режима отображения (не редактирования).
    /// Используется TextBlock с односторонней привязкой.
    /// </summary>
    protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
    {
        var textBlock = new TextBlock();
        if (CellTextStyle != null)
            textBlock.Style = CellTextStyle;
        if (!string.IsNullOrEmpty(BindingPath))
            textBlock.SetBinding(TextBlock.TextProperty, new Binding(BindingPath));
        return textBlock;
    }

    /// <summary>
    /// Создаёт элемент для режима редактирования.
    /// Используется TextBox с двусторонней привязкой.
    /// </summary>
    protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
    {
        var textBox = new TextBox();
        if (CellEditStyle != null)
            textBox.Style = CellEditStyle;
        if (!string.IsNullOrEmpty(BindingPath))
        {
            var binding = new Binding(BindingPath)
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            textBox.SetBinding(TextBox.TextProperty, binding);
        }
        return textBox;
    }
    
    #endregion
}

