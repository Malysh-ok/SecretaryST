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
/// Бихейвер. Реализует перемещение строк (drag and drop) в DataGrid.
/// </summary>
public class DataGridDragAndDropBehavior : Behavior<DataGrid>, IDragAndDropBehavior
{
    // Список строк DataGrid
    public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(
        nameof(Items), typeof(IList), typeof(DataGridDragAndDropBehavior), 
        new PropertyMetadata(default(IList)));
    
    // Команда, выполняемая после перемещения строки
    public static readonly DependencyProperty AfterItemMovedCommandProperty = 
        DependencyProperty.Register(nameof(AfterItemMovedCommand), typeof(ICommand), 
            typeof(DataGridDragAndDropBehavior));

    public IList? Items
    {
        get => (IList)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }
    
    public ICommand? AfterItemMovedCommand
    {
        get => (ICommand)GetValue(AfterItemMovedCommandProperty);
        set => SetValue(AfterItemMovedCommandProperty, value);
    }

    #region Fields

    private bool _mouseDown;
    private bool _isDragging;
    private Point _startPosition;
    private bool _isMovePerformed;

    #endregion Fields

    #region Properties

    /// <inheritdoc />
    public bool IsAllowDrop { get; set; }

    public double Tolerance { get; set; } = 20;

    public double Offset { get; set; } = 5;

    #endregion Properties

    #region Methods

    protected override void OnAttached()
    {
        base.OnAttached();
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
        AssociatedObject.PreviewMouseDown -= OnPreviewMouseDown;
        AssociatedObject.PreviewMouseUp -= OnPreviewMouseUp;
        AssociatedObject.DragEnter -= OnDragMoving;
        AssociatedObject.DragLeave -= OnDragMoving;
        AssociatedObject.MouseMove -= OnMouseMove;
        AssociatedObject.DragOver -= OnDragOver;
        AssociatedObject.Drop -= Drop;
        AssociatedObject.GiveFeedback -= OnGiveFeedback;
    }
    
    private void Drop(object sender, DragEventArgs e)
    {
        if (Items != null && Items.Count > 0)
        {
            var droppedItem = e.Data.GetData(Items[0]!.GetType());

            if (e.OriginalSource is DependencyObject dependencyObject)
            {
                var contentControl = dependencyObject.TryFindParent<ContentControl>();

                if (droppedItem != null)
                {
                    var destinationItem = contentControl?.DataContext;
                    if (destinationItem != null 
                        && destinationItem.GetType() == droppedItem.GetType() 
                        && !droppedItem.Equals(destinationItem))
                    {
                        var destIndex = Items.IndexOf(destinationItem);
                        Items.Remove(droppedItem);
                        Items.Insert(destIndex, droppedItem);
                        
                        _isMovePerformed = true; // перемещение выполнено
                        
                        var view = CollectionViewSource.GetDefaultView(Items);
                        view.Refresh();
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
                    var row = element.TryFindParent<DataGridRow>();
                    var destinationItem = row?.Item;
                
                    if (destinationItem != null && destinationItem.GetType() == droppedItem.GetType())
                    {
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
    
    private bool IsDragStart(Point position)
    {
        if (_mouseDown)
        {
            return Math.Abs(position.X - _startPosition.X) > SystemParameters.MinimumHorizontalDragDistance ||
                   Math.Abs(position.Y - _startPosition.Y) > SystemParameters.MinimumVerticalDragDistance;
        }
        return false;
    }

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
        if (e.LeftButton == MouseButtonState.Pressed && !_isDragging && IsDragStart(e.GetPosition(null)))
        {
            _isDragging = true;

            var result = VisualTreeHelper.HitTest(AssociatedObject, e.GetPosition(AssociatedObject));
            var element = result?.VisualHit;
            if (element is FrameworkElement frameworkElement)
            {
                if (frameworkElement.DataContext != null)
                {
                    DragDrop.DoDragDrop(AssociatedObject, frameworkElement.DataContext, DragDropEffects.Move);
                }
            }

            _isDragging = false;
        }
    }
    
    #endregion Handlers
}