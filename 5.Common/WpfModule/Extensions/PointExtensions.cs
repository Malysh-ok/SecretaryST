using System.Windows;
using Common.BaseExtensions.ValueTypes;

namespace Common.WpfModule.Extensions;

public static class PointExtensions
{
    /// <summary>
    /// Покоординатная сумма точек
    /// </summary>
    /// <param name="point1">Первая точка</param>
    /// <param name="point2">Вторая точка</param>
    /// <returns>Ограниченное значение.</returns>
    public static Point Plus(this Point point1, Point point2)
    {
        return new Point(point1.X + point2.X, point1.Y + point2.Y);
    }

    /// <summary>
    /// Покоординатная разность точек
    /// </summary>
    /// <param name="point1">Первая точка (Уменьшаемое)</param>
    /// <param name="point2">Вторая точка (Вычитаемое)</param>
    /// <returns>Разностть.</returns>
    public static Point Minus(this Point point1, Point point2)
    {
        return new Point(point1.X - point2.X, point1.Y - point2.Y);
    }
    /// <summary>
    /// Деление вектора, представленного в виде точки
    /// </summary>
    /// <param name="point">Tочка (Делимое)</param>
    /// <param name="divider">Делитель</param>
    /// <returns>Ограниченное значение (Разностть).</returns>
    public static Point Divide(this Point point, double divider)
    {
        return new Point(point.X / divider, point.Y / divider);
    }

    /// <summary>
    /// Покоординатная сумма точек
    /// </summary>
    /// <param name="points">Все точки, которые нужно суммировать</param>
    /// <returns>Ограниченное значение.</returns>
    public static Point SummPoint(this IEnumerable<Point> points)
    {
        double x = 0, y = 0;
        foreach (var point in points)
        {
            x += point.X;
            y += point.Y;
        }
        return new Point(x, y);
    }

    /// <summary>
    /// Проверка точки на начало координат (0;0)
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public static bool IsZero(this Point point)
    {
        return (Math.Abs(point.X) + Math.Abs(point.Y)).IsEquals(0, 0.0001);
    }


    /// <summary>
    /// Метод для ограничения точки
    /// </summary>
    /// <param name="point"></param>
    /// <param name="minX"></param>
    /// <param name="minY"></param>
    /// <param name="maxX"></param>
    /// <param name="maxY"></param>
    /// <returns></returns>
    public static Point Bound(this Point point, double minX = -1, double minY = -1, double maxX = 1, double maxY = 1)
    {
        return new Point(point.X.Bound(minX, maxX), point.Y.Bound(minY, maxY));
    }

    /// <summary>
    /// Обновить значение точки и добавить в коллекцию. 
    /// </summary>
    /// <param name="points"></param>
    /// <param name="point"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public static void AddPoint(this IList<Point> points, ref Point point, double x, double y)
    {
        point.X = x;
        point.Y = y;
        points.Add(point);
    }
}