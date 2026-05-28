using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using Common.BaseExtensions;

namespace Common.WpfModule.Ui.Converters;

/// <summary>
/// Преобразует языковой тег в виде строки (в формате "ru-RU", "en-US" и т.п.) в соответствующий ему <see cref="XmlLanguage"/>.
/// </summary>
[ValueConversion(typeof(string), typeof(XmlLanguage))]
public class StrToXmlLangConverter: IValueConverter
{
    /// <inheritdoc />
    public object Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
    {
        var languageTag = value as string;
        
        if (languageTag.IsNullOrWhiteSpace())
        {
            languageTag = culture?.Name;
        }
        
        try
        {
            return languageTag.IsNullOrWhiteSpace()
                ? Binding.DoNothing
                : XmlLanguage.GetLanguage(languageTag!);
        }
        catch
        {
            return Binding.DoNothing;
        }
    }
    
    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is XmlLanguage xmlLanguage
            ? xmlLanguage.IetfLanguageTag
            : Binding.DoNothing;
    }  
}