using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using Common.BaseExtensions;
using Common.WpfModule.Ui.Converters;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Common.WpfModule.Ui.Markups;

/// <summary>
/// MarkupExtension для модификации существующего Thickness (Margin/Padding).
/// Позволяет переопределить только указанные стороны, сохраняя остальные без изменений.
/// </summary>
/// <remarks>
/// Используется в XAML для задания Margin или Padding с частичной заменой.
/// Например, чтобы увеличить левый отступ, но сохранить верхний/нижний/правый из текущего значения.
/// </remarks>
public class ThicknessModifyExtension : MarkupExtension
{
    /// <summary>
    /// Новое значение для левого отступа. Если null – остаётся без изменений.
    /// </summary>
    public double? Left { get; set; }

    /// <summary>
    /// Новое значение для верхнего отступа. Если null – остаётся без изменений.
    /// </summary>
    public double? Top { get; set; }

    /// <summary>
    /// Новое значение для правого отступа. Если null – остаётся без изменений.
    /// </summary>
    public double? Right { get; set; }

    /// <summary>
    /// Новое значение для нижнего отступа. Если null – остаётся без изменений.
    /// </summary>
    public double? Bottom { get; set; }

    /// <summary>
    /// Конструктор без параметров (обязателен для использования в XAML).
    /// </summary>
    public ThicknessModifyExtension() { }

    /// <summary>
    /// Возвращает Binding с конвертером, который подставит текущее значение Thickness
    /// и заменит только указанные стороны.
    /// </summary>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var path = string.Empty;
        
        // Пытаемся определить целевое свойство
        if (serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget target)
        {
            // Проверяем, является ли целевое свойство DependencyProperty или PropertyInfo
            if (target.TargetProperty is DependencyProperty dp)
            {
                // Если свойство имеет тип Thickness, используем его имя
                if (dp.PropertyType == typeof(Thickness))
                    path = dp.Name;
            }
            else if (target.TargetProperty is PropertyInfo propInfo)
            {
                // Если свойство имеет тип Thickness, используем его имя
                if (propInfo.PropertyType == typeof(Thickness))
                    path = propInfo.Name;
            }
        }
        
        // Если свойство не Thickness — возвращаем обычный Binding без конвертера
        if (path.IsNullOrEmpty())
        {
            return new Binding
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.Self),
                Mode = BindingMode.OneWay
            };
        }
        
        // Создаём конвертер с переданными параметрами
        var converter = new ThicknessModifyConverter
        {
            Left = Left,
            Top = Top,
            Right = Right,
            Bottom = Bottom
        };

        // Создаём Binding к самому элементу (RelativeSource.Self),
        // чтобы получить текущее значение Thickness и передать его в конвертер.
        var binding = new Binding
        {
            RelativeSource = new RelativeSource(RelativeSourceMode.Self),
            Path = new PropertyPath(path),
            Converter = converter,
            Mode = BindingMode.OneWay
        };

        // Возвращаем Binding, который будет применён к свойству (Margin или Padding).
        // return binding.ProvideValue(serviceProvider);
        return binding;
    }
}