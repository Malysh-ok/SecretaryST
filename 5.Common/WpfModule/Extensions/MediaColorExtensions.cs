using System.Diagnostics.CodeAnalysis;
using Common.BaseComponents.Components.Colors;
using DrawingColor = System.Drawing.Color;
using MediaColor = System.Windows.Media.Color;

namespace Common.WpfModule.Extensions
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class MediaColorExtensions
    {
        /// <summary>
        /// System.Drawing.Color => System.Windows.Media.Color.
        /// </summary>
        public static MediaColor ToMediaColor(this DrawingColor drawingColor)
        {
            return MediaColor.FromArgb(drawingColor.A, drawingColor.R, drawingColor.G, drawingColor.B);
        }
        
        /// <summary>
        /// System.Windows.Media.Color => System.Drawing.Color.
        /// </summary>
        public static DrawingColor ToDrawingColor(this MediaColor mediaColor)
        {
            return DrawingColor.FromArgb(mediaColor.A, mediaColor.R, mediaColor.G, mediaColor.B);
        }

        /// <summary>
        /// Получение цвета из строки.
        /// </summary>
        /// <remarks>
        /// В качестве строки colorStr может выступать как название цвета,
        /// так и html-код цвета (в формате #AARRGGBB или #RRGGBB).<br/>
        /// Если признак isCheckOnlyKnownColors = true,
        /// то colorStr должна совпадать долько с известным (предопределенным) цветом.
        /// </remarks>
        /// <returns>
        /// Экземпляр типа MediaColor.<br/>
        /// В случае, если цвет с данным именем отсутствует, или имя цвета = "", или неправильный формат html-кода цвета,
        /// возвращается цвет colorIfError.
        /// </returns>
        /// <param name="colorStr">Строка, идентифицирующая цвет.</param>
        /// <param name="colorIfError">Возвращаемый цвет при ошибке.</param>
        /// <param name="isCheckOnlyKnownColors">Признак проверки только на известные цвета.</param>
        public static MediaColor StrToMediaColor(this string colorStr, 
            MediaColor colorIfError, bool isCheckOnlyKnownColors = true)
        {
            var drawingColorIfError = colorIfError.ToDrawingColor();
            var drawingColor = colorStr.StrToDrawingColor(drawingColorIfError, isCheckOnlyKnownColors);
            return drawingColor.ToMediaColor();
        }
        
        /// <summary>
        /// Получение цвета из строки.
        /// </summary>
        /// <remarks>
        /// В качестве строки colorStr может выступать как название цвета,
        /// так и html-код цвета (в формате #AARRGGBB или #RRGGBB).<br/>
        /// Если признак isCheckOnlyKnownColors = true,
        /// то colorStr должна совпадать долько с известным (предопределенным) цветом.
        /// </remarks>
        /// <returns>
        /// Экземпляр типа MediaColor.<br/>
        /// В случае, если цвет с данным именем отсутствует, или имя цвета = "", или неправильный формат html-кода цвета,
        /// возвращается "пустой" цвет.
        /// </returns>
        /// <param name="colorStr">Строка, идентифицирующая цвет.</param>
        /// <param name="isCheckOnlyKnownColors">Признак проверки только на известные цвета.</param>
        public static MediaColor StrToMediaColor(this string colorStr, bool isCheckOnlyKnownColors = true)
        {
            return StrToMediaColor(colorStr, new MediaColor(), isCheckOnlyKnownColors);
        }

        /// <summary>
        /// Преобразуем стандартный цвет (MediaColor) в HSB-цвет.
        /// </summary>
        public static HsbColor MediaColorToHsbColor(this MediaColor color)
        {
            var drawingColor = color.ToDrawingColor();
            return drawingColor.DrawingColorToHsbColor();
        }

        /// <summary>
        /// Преобразуем HSB-цвет в стандартный цвет (MediaColor).
        /// </summary>
        public static MediaColor HsbColorToMediaColor(this HsbColor hsbColor)
        {
            var drawingColor = hsbColor.HsbColorToDrawingColor();
            return drawingColor.ToMediaColor();
        }

    }
}