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
/// Бихейвер. Сортирует список элементов в DataGrid.
/// </summary>
public class DataGridSortDragAndDropBehavior : Behavior<DataGrid>, IDragAndDropBehavior
{
    public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(
        nameof(Items), typeof(ICollection), typeof(DataGridSortDragAndDropBehavior), 
        new PropertyMetadata(default(IList)));

    public IList? Items
    {
        get => (IList)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    #region Fields

    private bool _mouseDown;
    private double _tolerance = 20;
    private double _offset = 5;
    private bool _isDragging;
    private Point _startPosition;

    #endregion Fields

    #region Properties

    /// <inheritdoc />
    public bool IsAllowDrop { get; set; }

    public double Tolerance
    {
        get => _tolerance;
        set => _tolerance = value;
    }

    public double Offset
    {
        get => _offset;
        set => _offset = _tolerance;
    }

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
                        var view = CollectionViewSource.GetDefaultView(Items);
                        view.Refresh();
                    }
                }
            }
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
            var droppedItem = e.Data.GetData(Items[0]!.GetType());
            if (droppedItem != null)
            {
                if (e.OriginalSource is FrameworkElement element)
                {
                    var contentControl = element.TryFindParent<ContentControl>();
                    var destinationItem = contentControl?.DataContext;
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

    private void MouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed && !_isDragging && IsDragStart(e.GetPosition(null)))
        {
            _isDragging = true;

            var result = VisualTreeHelper.HitTest(AssociatedObject, e.GetPosition(AssociatedObject));
            if (result != null)
            {
                var element = result.VisualHit;
                if (element is FrameworkElement frameworkElement)
                {
                    if (frameworkElement.DataContext != null)
                    {
                        DragDrop.DoDragDrop(AssociatedObject, frameworkElement.DataContext, DragDropEffects.Move);
                        //| DragDropEffects.Copy);
                    }
                }
            }


            _isDragging = false;
        }
    }
    #endregion Handlers
}