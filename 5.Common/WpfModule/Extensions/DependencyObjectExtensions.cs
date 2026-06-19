using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Common.BaseExtensions;

namespace Common.WpfModule.Extensions;

/// <summary>
/// Набор вспомогательных методов для работы с DependencyObject (визуальное и логическое дерево WPF).
/// </summary>
public static class DependencyObjectExtensions
{
    /// <summary>
    /// Рекурсивно ищет первого потомка указанного типа в визуальном дереве (обход в глубину).
    /// </summary>
    /// <typeparam name="T">Тип искомого дочернего элемента.</typeparam>
    /// <param name="depObj">Родительский элемент, с которого начинается поиск.</param>
    /// <returns>Первый найденный элемент типа T, либо null, если ничего не найдено.</returns>
    public static T? FindVisualChild<T>(this DependencyObject depObj) where T : DependencyObject
    {
        // Сначала проверяем непосредственных детей (обход в ширину на первом уровне)
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

    /// <summary>
    /// Возвращает всех потомков указанного типа в визуальном дереве (итератор).
    /// </summary>
    /// <typeparam name="T">Тип искомых дочерних элементов.</typeparam>
    /// <param name="depObj">Родительский элемент, с которого начинается поиск (может быть null).</param>
    /// <returns>Последовательность элементов типа T.</returns>
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

    /// <summary>
    /// Принудительно обновляет источник привязки для указанного свойства зависимостей.
    /// </summary>
    /// <param name="dependencyObject">Элемент, у которого есть привязка.</param>
    /// <param name="property">Свойство, для которого нужно обновить источник.</param>
    public static void UpdateSource(this DependencyObject dependencyObject, DependencyProperty property)
    {
        var be = BindingOperations.GetBindingExpression(dependencyObject, property);
        be?.UpdateSource();
    }

    /// <summary>
    /// Принудительно обновляет целевое значение (UI) для указанного свойства зависимостей.
    /// </summary>
    /// <param name="dependencyObject">Элемент, у которого есть привязка.</param>
    /// <param name="property">Свойство, для которого нужно обновить цель.</param>
    public static void UpdateTarget(this DependencyObject dependencyObject, DependencyProperty property)
    {
        var be = BindingOperations.GetBindingExpression(dependencyObject, property);
        be?.UpdateTarget();
    }

    /// <summary>
    /// Очищает все привязки для указанного элемента.
    /// </summary>
    /// <param name="dependencyObject">Элемент, у которого нужно удалить привязки.</param>
    public static void ClearAllBindings(this DependencyObject dependencyObject)
    {
        BindingOperations.ClearAllBindings(dependencyObject);
    }

    /// <summary>
    /// Вычисляет центральную точку текущего элемента относительно родительского элемента,
    /// у которого DataContext имеет заданный тип, с возможностью смещения.
    /// </summary>
    /// <typeparam name="TElement">Тип текущего элемента (должен быть FrameworkElement).</typeparam>
    /// <typeparam name="TParentContext">Тип DataContext искомого родительского элемента.</typeparam>
    /// <param name="dependencyObject">Текущий элемент.</param>
    /// <param name="offsetX">Смещение по X (вычитается из итоговой координаты).</param>
    /// <param name="offsetY">Смещение по Y.</param>
    /// <returns>Точка (X,Y) относительно найденного родителя.</returns>
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
    /// Находит первого родителя указанного типа в визуальном/логическом дереве.
    /// </summary>
    /// <typeparam name="T">Тип искомого родительского элемента.</typeparam>
    /// <param name="child">Дочерний элемент, с которого начинается поиск.</param>
    /// <returns>Найденный родитель или null, если ничего не найдено.</returns>
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
    /// Находит первого родителя указанного типа с заданным именем.
    /// </summary>
    /// <typeparam name="T">Тип искомого родительского элемента.</typeparam>
    /// <param name="child">Дочерний элемент, с которого начинается поиск.</param>
    /// <param name="parentName">Имя родительского элемента (свойство Name).</param>
    /// <returns>Найденный родитель или null, если ничего не найдено.</returns>
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
    /// Ищет среди родителей элемент типа TElement, у которого DataContext имеет тип TDataContext.
    /// </summary>
    /// <typeparam name="TElement">Тип искомого родительского элемента (должен быть FrameworkElement).</typeparam>
    /// <typeparam name="TDataContext">Тип DataContext, который должен быть у родителя.</typeparam>
    /// <param name="childElement">Дочерний элемент, с которого начинается поиск.</param>
    /// <returns>Найденный элемент или null.</returns>
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
    /// Возвращает DataContext родителя, который имеет указанный тип.
    /// </summary>
    /// <typeparam name="TDataContext">Тип искомого DataContext.</typeparam>
    /// <param name="childElement">Дочерний элемент, с которого начинается поиск.</param>
    /// <returns>DataContext найденного родителя или null.</returns>
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
    /// Универсальный метод получения родительского элемента (работает с визуальным и логическим деревом).
    /// Альтернатива VisualTreeHelper.GetParent, поддерживающая ContentElement и FrameworkElement.
    /// </summary>
    /// <param name="child">Дочерний элемент.</param>
    /// <returns>Родительский элемент или null.</returns>
    public static DependencyObject? GetParentObject(this DependencyObject? child)
    {
        if (child == null) return null;

        // Обрабатываем ContentElement отдельно (логическое дерево)
        if (child is ContentElement contentElement)
        {
            var parent = ContentOperations.GetParent(contentElement);
            if (parent != null) return parent;

            var fce = contentElement as FrameworkContentElement;
            return fce?.Parent;
        }

        // Для FrameworkElement используем свойство Parent (логическое дерево)
        if (child is FrameworkElement frameworkElement)
        {
            var parent = frameworkElement.Parent;
            if (parent != null) return parent;
        }

        // Для Window проверяем Owner
        if (child is Window window)
        {
            var parent = window.Owner;
            if (parent != null) return parent;
        }

        // Во всех остальных случаях используем VisualTreeHelper
        return VisualTreeHelper.GetParent(child);
    }

    /// <summary>
    /// Находит элемент указанного типа в точке относительно reference.
    /// </summary>
    /// <typeparam name="T">Тип искомого элемента.</typeparam>
    /// <param name="reference">Элемент, относительно которого задана точка.</param>
    /// <param name="point">Точка в координатах reference.</param>
    /// <returns>Найденный элемент или null.</returns>
    public static T? TryFindFromPoint<T>(this UIElement reference, Point point) where T : DependencyObject
    {
        if (reference.InputHitTest(point) is DependencyObject element)
            return element as T ?? TryFindParent<T>(element); 

        return null;
    }

    /// <summary>
    /// Находит элемент указанного типа и имени в заданной точке.
    /// </summary>
    /// <typeparam name="T">Тип искомого элемента.</typeparam>
    /// <param name="reference">Элемент, относительно которого задана точка.</param>
    /// <param name="point">Точка в координатах reference.</param>
    /// <param name="elementName">Имя элемента, которое должен иметь найденный элемент или его родитель.</param>
    /// <returns>Найденный элемент или null.</returns>
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