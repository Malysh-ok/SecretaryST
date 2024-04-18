using System.Globalization;

// ReSharper disable MemberCanBePrivate.Global

namespace Common.BaseExtensions;

public static class CultureInfoExtensions
{
    /// <summary>
    /// Получить двухбуквенное наименование языкового стандарта ("en", "ru", и т.п.).
    /// </summary>
    public static string ShortName(this CultureInfo culture) => culture.TwoLetterISOLanguageName;

    /// <summary>
    /// По сути - естественное наименование языка языкового стандарта ("Английский", "Русский", и т.п.)
    /// </summary>
    public static string ShortDisplayName(this CultureInfo culture) => culture.Parent.DisplayName.ToCamelCase();
    
    /// <summary>
    /// Получить признак того, что язык языкового стандарта является русским.
    /// </summary>
    public static bool IsRu(this CultureInfo culture) => culture.ShortName() == "ru";
    
    /// <summary>
    /// Получить признак того, что язык языкового стандарта является английским.
    /// </summary>
    public static bool IsEn(this CultureInfo culture) => culture.ShortName() == "en";
    
    /// <summary>
    /// Получить признак того, что языковой стандарт с заданным именем существует.
    /// </summary>
    public static bool IsExists(this string name)
    {
        try
        {
            _ = CultureInfo.GetCultureInfo(name);

            return true;
        }
        catch
        {
            return false;
        }
    }
}