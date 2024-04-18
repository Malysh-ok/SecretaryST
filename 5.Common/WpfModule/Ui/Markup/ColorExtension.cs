using System.Diagnostics.CodeAnalysis;
using System.Windows.Markup;
using System.Windows.Media;
using Common.BaseComponents.Components.Colors;
using Common.WpfModule.Extensions;

namespace Common.WpfModule.Ui.Markup;

/// <summary>
/// Расширение разметки для цвета.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class ColorExtension: MarkupExtension
{
    /// <summary>
    /// Базовый цвет.
    /// </summary>
    public Color BaseColor { get; set; }

    /// <summary>
    /// Приращение прозрачности базового цвета (от -255 до 255).
    /// </summary>
    public int DeltaAlpha { get; set; } = 0;

    /// <summary>
    /// Приращение цветового тона базовому цвету (от -350° до 360°).
    /// </summary>
    public float DeltaHue { get; set; } = 0;

    /// <summary>
    /// Приращение насыщенности базовому цвету (от -1 до 1).
    /// </summary>
    public float DeltaSaturation { get; set; } = 0;

    /// <summary>
    /// Приращение яркости базовому цвету в долях единицы (от -1 до 1).
    /// </summary>
    public float DeltaBrightness { get; set; } = 0;

    /// <inheritdoc />
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var hsbColor = BaseColor.MediaColorToHsbColor();
        hsbColor = hsbColor.AdjustHsbColor(DeltaAlpha, DeltaHue, DeltaSaturation, DeltaBrightness);
        return hsbColor.HsbColorToMediaColor();
    }
}