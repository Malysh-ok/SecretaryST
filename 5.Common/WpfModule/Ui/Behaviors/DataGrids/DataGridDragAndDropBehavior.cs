using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Common.WpfModule.Extensions;
using Common.WpfModule.Ui.Behaviors.DragAndDrop;
using Microsoft.Xaml.Behaviors;

namespace Common.WpfModule.Ui.Behaviors.DataGrids;

/// <summary>
/// Бихейвер, реализующий перемещение строк (Drag-and-Drop) в DataGrid.
/// Позволяет пользователю перетаскивать строки мышью для изменения их порядка.
/// </summary>
public class DataGridDragAndDropBehavior : Behavior<DataGrid>, IDragAndDropBehavior
{
    /// <summary>
    /// Коллекция элементов (строк), с которой работает бихейвер.
    /// Обычно это ItemsSource DataGrid.
    /// </summary>
    public IList? Items
    {
        get => (IList)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }
    /// <inheritdoc cref="Items"/>
    public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(
        nameof(Items), typeof(IList), typeof(DataGridDragAndDropBehavior), 
        new PropertyMetadata(default(IList)));
    
    /// <summary>
    /// Команда, выполняемая после успешного перемещения строки.
    /// Удобно для обновления нумерации или пересчёта зависимых данных.
    /// </summary>
    public ICommand? AfterItemMovedCommand
    {
        get => (ICommand)GetValue(AfterItemMovedCommandProperty);
        set => SetValue(AfterItemMovedCommandProperty, value);
    }
    /// <inheritdoc cref="AfterItemMovedCommand"/>
    public static readonly DependencyProperty AfterItemMovedCommandProperty = 
        DependencyProperty.Register(nameof(AfterItemMovedCommand), typeof(ICommand), 
            typeof(DataGridDragAndDropBehavior));

    #region Fields

    private bool _mouseDown;           // Флаг: зажата ли левая кнопка мыши на элементе DataGrid
    private bool _isDragging;          // Флаг: выполняется ли в данный момент перетаскивание
    private Point _startPosition;      // Позиция мыши в момент нажатия (в экранных координатах)
    private bool _isMovePerformed;     // Флаг: было ли совершено перемещение (используется для вызова команды)

    #endregion Fields

    #region Properties

    /// <inheritdoc />
    public bool IsAllowDrop { get; set; }  // Разрешён ли сброс в текущей позиции (управляет курсором)

    /// <summary>
    /// Расстояние от края DataGrid до начала автопрокрутки (в пикселях).
    /// При перетаскивании вблизи верхней или нижней границы начинается скроллинг.
    /// </summary>
    public double Tolerance { get; set; } = 20;

    /// <summary>
    /// Шаг автопрокрутки (в пикселях) при приближении к границе.
    /// </summary>
    public double Offset { get; set; } = 5;

    #endregion Properties

    #region Methods

    protected override void OnAttached()
    {
        base.OnAttached();
        // Подписка на события мыши и drag-drop
        AssociatedObject.PreviewMouseDown += OnPreviewMouseDown;
        AssociatedObject.PreviewMouseUp += OnPreviewMouseUp;
        AssociatedObject.DragEnter += OnDragMoving;
        AssociatedObject.DragLeave += OnDragMoving;
        AssociatedObject.MouseMove += OnMouseMove;
        AssociatedObject.DragOver += OnDragOver;
        AssociatedObject.Drop += Drop;
        AssociatedObject.GiveFeedback += OnGiveFeedback;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        // Отписка от событий
        AssociatedObject.PreviewMouseDown -= OnPreviewMouseDown;
        AssociatedObject.PreviewMouseUp -= OnPreviewMouseUp;
        AssociatedObject.DragEnter -= OnDragMoving;
        AssociatedObject.DragLeave -= OnDragMoving;
        AssociatedObject.MouseMove -= OnMouseMove;
        AssociatedObject.DragOver -= OnDragOver;
        AssociatedObject.Drop -= Drop;
        AssociatedObject.GiveFeedback -= OnGiveFeedback;
    }
    
    /// <summary>
    /// Обработчик завершения перетаскивания (Drop).
    /// Выполняет перемещение элемента в коллекции и обновляет UI.
    /// </summary>
    private void Drop(object sender, DragEventArgs e)
    {
        if (Items != null && Items.Count > 0)
        {
            // Получаем перетаскиваемый элемент по его типу (первый элемент коллекции)
            var droppedItem = e.Data.GetData(Items[0]!.GetType());

            if (e.OriginalSource is DependencyObject dependencyObject)
            {
                // Находим родительский ContentControl (строка DataGrid)
                var contentControl = dependencyObject.TryFindParent<ContentControl>();

                if (droppedItem != null)
                {
                    var destinationItem = contentControl?.DataContext;
                    // Проверяем, что целевой элемент существует, того же типа и не равен источнику
                    if (destinationItem != null 
                        && destinationItem.GetType() == droppedItem.GetType() 
                        && !droppedItem.Equals(destinationItem))
                    {
                        var destIndex = Items.IndexOf(destinationItem);
                        Items.Remove(droppedItem);
                        Items.Insert(destIndex, droppedItem);
                        
                        _isMovePerformed = true; // перемещение выполнено
                        
                        var view = CollectionViewSource.GetDefaultView(Items);
                        view.Refresh(); // обновляем UI
                    }
                }
            }
        }
        
        // Выполняем команду, если перемещение было
        if (_isMovePerformed)
        {
            AfterItemMovedCommand?.Execute(null);
        }
    }

    #endregion Methods

    #region Handlers

    /// <summary>
    /// Обработчик GiveFeedback – определяет вид курсора во время перетаскивания.
    /// Если сброс запрещён, показывает курсор "Нет".
    /// </summary>
    private void OnGiveFeedback(object sender, GiveFeedbackEventArgs e)
    {
        if (!IsAllowDrop)
        {
            e.UseDefaultCursors = false;
            Mouse.SetCursor(Cursors.No);
        }
        else
        {
            e.UseDefaultCursors = true;
        }
        e.Handled = true;
    }

    /// <summary>
    /// Обработчик DragOver – обеспечивает автопрокрутку при перетаскивании к краям DataGrid.
    /// </summary>
    private void OnDragOver(object sender, DragEventArgs e)
    {
        var control = sender as ItemsControl;
        var scrollViewer = control?.FindVisualChild<ScrollViewer>();
        if (control != null && scrollViewer != null)
        {
            var verticalPos = e.GetPosition(control).Y;
            if (verticalPos < Tolerance)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - Offset);
            }
            if (verticalPos > control.ActualHeight - Tolerance)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + Offset);
            }
        }
    }

    /// <summary>
    /// Обработчик DragEnter/DragLeave – определяет, можно ли сбросить элемент в текущую позицию.
    /// Анализирует тип элемента под курсором и устанавливает IsAllowDrop.
    /// </summary>
    private void OnDragMoving(object sender, DragEventArgs e)
    {
        if (Items != null && Items.Count > 0)
        {
            var itemType = Items[0]!.GetType();
        
            var droppedItem = e.Data.GetData(itemType);
        
            if (droppedItem != null)
            {
                if (e.OriginalSource is FrameworkElement element)
                {
                    // Находим строку DataGrid под мышью
                    var row = element.TryFindParent<DataGridRow>();
                    var destinationItem = row?.Item;
                
                    if (destinationItem != null && destinationItem.GetType() == droppedItem.GetType())
                    {
                        // Разрешаем сброс, если элементы разные
                        if (!droppedItem.Equals(destinationItem))
                        {
                            IsAllowDrop = true;
                            return;
                        }
                    }
                }
            }
        }
        IsAllowDrop = false;
    }
    
    /// <summary>
    /// Проверяет, превысило ли перемещение мыши порог, чтобы начать перетаскивание.
    /// </summary>
    private bool IsDragStart(Point position)
    {
        if (_mouseDown)
        {
            return Math.Abs(position.X - _startPosition.X) > SystemParameters.MinimumHorizontalDragDistance ||
                   Math.Abs(position.Y - _startPosition.Y) > SystemParameters.MinimumVerticalDragDistance;
        }
        return false;
    }

    /// <summary>
    /// Обработчик PreviewMouseDown – запоминает начальную позицию мыши.
    /// Игнорирует нажатие на полосе прокрутки.
    /// </summary>
    private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is DependencyObject original)
        {
            var scroll = original.TryFindParent<ScrollBar>();
            if (scroll != null)
            {
                _mouseDown = false;
                return;
            }
        }
        _startPosition = e.GetPosition(null);
        _mouseDown = true;
    }

    private void OnPreviewMouseUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
    {
        _mouseDown = false;
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        MouseMove(sender, e);
    }

    // ReSharper disable once UnusedParameter.Local
    private void MouseMove(object sender, MouseEventArgs e)
    {
        // Если кнопка зажата, перемещение превысило порог и перетаскивание ещё не начато
        if (e.LeftButton == MouseButtonState.Pressed && !_isDragging && IsDragStart(e.GetPosition(null)))
        {
            _isDragging = true;

            var result = VisualTreeHelper.HitTest(AssociatedObject, e.GetPosition(AssociatedObject));
            var element = result?.VisualHit;
            if (element is FrameworkElement frameworkElement)
            {
                if (frameworkElement.DataContext != null)
                {
                    // Инициируем операцию drag-drop
                    DragDrop.DoDragDrop(AssociatedObject, frameworkElement.DataContext, DragDropEffects.Move);
                }
            }

            _isDragging = false;
        }
    }
    
    #endregion Handlers
}