using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Common.BaseExtensions;

namespace Common.WpfModule.Extensions
{
    // TODO: закомментировать DependencyObjectExtensions
    public static class DependencyObjectExtensions
    {
        public static T? FindVisualChild<T>(this DependencyObject depObj) where T : DependencyObject
        {
            // Search immediate children first (breadth-first)
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                if (child is T visualChild)
                    return visualChild;
                var childOfChild = FindVisualChild<T>(child);

                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }

        public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject? depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0, count = VisualTreeHelper.GetChildrenCount(depObj); i < count; i++)
                {
                    var child = VisualTreeHelper.GetChild(depObj, i);
                    if (child is T children)
                    {
                        yield return children;
                    }

                    foreach (var childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        public static void UpdateSource(this DependencyObject dependencyObject, DependencyProperty property)
        {
            var be = BindingOperations.GetBindingExpression(dependencyObject, property);
            be?.UpdateSource();
        }

        public static void UpdateTarget(this DependencyObject dependencyObject, DependencyProperty property)
        {
            var be = BindingOperations.GetBindingExpression(dependencyObject, property);
            be?.UpdateTarget();
        }

        public static void ClearAllBindings(this DependencyObject dependencyObject)
        {
            BindingOperations.ClearAllBindings(dependencyObject);
        }

        /// <summary>
        /// Получает центральную точку элемента относительно предка указанного типа с нужными отступами
        /// </summary>
        /// <typeparam name="TElement"></typeparam>
        /// <typeparam name="TParentContext"></typeparam>
        /// <param name="dependencyObject"></param>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        /// <returns></returns>
        public static Point GetCenterPositionOnParent<TElement, TParentContext>(this DependencyObject dependencyObject,
            double offsetX = 0.0, double offsetY = 0.0) where TElement : FrameworkElement
        {
            double x = 0.0, y = 0.0;
            if (dependencyObject is FrameworkElement frameworkElement)
            {
                var findedElement = frameworkElement.GetParentByDataContextType<TElement, TParentContext>();
                if (findedElement != null)
                {
                    if (PresentationSource.FromVisual(frameworkElement) != null)
                    {
                        var point = frameworkElement.PointToScreen(new Point(frameworkElement.ActualWidth / 2.0, frameworkElement.ActualHeight / 2.0));
                        var pointOnParent = findedElement.PointFromScreen(point);
                        x = pointOnParent.X - offsetX;
                        y = pointOnParent.Y - offsetY;
                    }
                }
            }
            return new Point(x, y);
        }

        /// <summary>
        /// Finds a parent of a given item on the visual tree.
        /// </summary>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="child">A direct or indirect child of the
        /// queried item.</param>
        /// <returns>The first parent item that matches the submitted
        /// type parameter. If not matching item can be found, a null
        /// reference is being returned.</returns>
        public static T? TryFindParent<T>(this DependencyObject? child) where T : DependencyObject
        {
            DependencyObject? parentObject;
            T? castedParentObject;
            do
            {
                parentObject = GetParentObject(child);

                if (parentObject == null)
                    return null;

                child = parentObject;
            } while ((castedParentObject = parentObject as T) == null);

            return castedParentObject;
        }

        /// <summary>
        /// Finds a parent of a given item on the visual tree.
        /// </summary>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="child">A direct or indirect child of the
        /// queried item.</param>
        /// <param name="parentName">Specified element name</param>
        /// <returns>The first parent item that matches the submitted
        /// type parameter. If not matching item can be found, a null
        /// reference is being returned.</returns>
        public static T? TryFindParent<T>(this DependencyObject? child, string parentName) where T : DependencyObject
        {
            DependencyObject? parentObject;
            T? castedParentObject;
            var equalName = false;
            do
            {
                parentObject = GetParentObject(child);
                
                switch (parentObject)
                {
                    case null:
                        return null;
                    case FrameworkElement frameworkElement:
                        equalName = frameworkElement.Name.IsEquals(parentName);
                        break;
                }

                child = parentObject;
            } while ((castedParentObject = parentObject as T) == null && !equalName);

            return castedParentObject;
        }

        /// <summary>
        /// Найти предка конкретного типа, по типу его датаконтекста
        /// </summary>
        /// <typeparam name="TElement">Тип искомого предка</typeparam>
        /// <typeparam name="TDataContext">Тип искомого датаконтекста</typeparam>
        /// <param name="childElement"></param>
        /// <returns>Если не найден вернут null</returns>
        public static FrameworkElement? GetParentByDataContextType<TElement, TDataContext>(this FrameworkElement? childElement)
            where TElement : FrameworkElement
        {
            var findedElement = childElement;
            while (findedElement != null && !(findedElement.DataContext is TDataContext))
            {
                findedElement = findedElement.TryFindParent<TElement>();
            }
            return findedElement;
        }

        /// <summary>
        /// Найти датаконтекст по его типу
        /// </summary>
        /// <typeparam name="TDataContext">Тип искомого датаконтекста</typeparam>
        /// <param name="childElement"></param>
        /// <returns>Если не найден вернут null</returns>
        public static TDataContext? GetParentDataContextByType<TDataContext>(this FrameworkElement childElement)
        {
            var findedElement = childElement;
            while (findedElement != null && !(findedElement.DataContext is TDataContext))
            {
                findedElement = (FrameworkElement)findedElement.Parent;
            }
            
            return (TDataContext?)findedElement?.DataContext;
        }

        /// <summary>
        /// This method is an alternative to WPF's
        /// <see cref="VisualTreeHelper.GetParent"/> method, which also
        /// supports content elements. Keep in mind that for content element,
        /// this method falls back to the logical tree of the element!
        /// </summary>
        /// <param name="child">The item to be processed.</param>
        /// <returns>The submitted item's parent, if available. Otherwise
        /// null.</returns>
        public static DependencyObject? GetParentObject(this DependencyObject? child)
        {
            if (child == null) return null;

            //handle content elements separately
            if (child is ContentElement contentElement)
            {
                var parent = ContentOperations.GetParent(contentElement);
                if (parent != null) return parent;

                var fce = contentElement as FrameworkContentElement;
                return fce?.Parent;
            }

            //also try searching for parent in framework elements (such as DockPanel, etc)
            if (child is FrameworkElement frameworkElement)
            {
                var parent = frameworkElement.Parent;
                if (parent != null) return parent;
            }

            if (child is Window window)
            {
                var parent = window.Owner;
                if (parent != null) return parent;
            }

            //if it's not a ContentElement/FrameworkElement, rely on VisualTreeHelper
            return VisualTreeHelper.GetParent(child);
        }

        /// <summary>
        /// Tries to locate a given item within the visual tree,
        /// starting with the dependency object at a given position. 
        /// </summary>
        /// <typeparam name="T">The type of the element to be found
        /// on the visual tree of the element at the given location.</typeparam>
        /// <param name="reference">The main element which is used to perform
        /// hit testing.</param>
        /// <param name="point">The position to be evaluated on the origin.</param>
        public static T? TryFindFromPoint<T>(this UIElement reference, Point point) where T : DependencyObject
        {
            if (reference.InputHitTest(point) is DependencyObject element)
                return element as T ?? TryFindParent<T>(element); 

            return null;
        }

        /// <summary>
        /// Tries to locate a given item within the visual tree,
        /// starting with the dependency object at a given position. 
        /// </summary>
        /// <typeparam name="T">The type of the element to be found
        /// on the visual tree of the element at the given location.</typeparam>
        /// <param name="reference">The main element which is used to perform
        /// hit testing.</param>
        /// <param name="point">The position to be evaluated on the origin.</param>
        /// <param name="elementName">Specified element name</param>
        public static T? TryFindFromPoint<T>(this UIElement reference, Point point, string elementName) where T : DependencyObject
        {
            if (reference.InputHitTest(point) is DependencyObject element and FrameworkElement frameworkElement)
            {
                if (frameworkElement.Name.IsEquals(elementName))
                {
                    if (element is T dependencyObject)
                        return dependencyObject;
                }

                return TryFindParent<T>(element, elementName);
            }

            return null;
        }
    }
}
