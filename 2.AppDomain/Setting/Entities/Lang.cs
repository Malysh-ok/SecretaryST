using System.Globalization;
using Common.BaseExtensions;

// ReSharper disable MemberCanBePrivate.Global

namespace AppDomain.Setting.Entities;

/// <summary>
/// Язык.
/// </summary>
public class Lang : IEquatable<Lang>, ICloneable
{
    /// <summary>
    /// Название языка в формате "en-EN", "ru-RU".
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Название языка в формате "en", "ru"...
    /// </summary>
    public string ShortName { get; }

    /// <summary>
    /// Естественное название языка: "Русский", "English"...
    /// </summary>
    public string DisplayName { get; private set; }

    /// <summary>
    /// Получить локализацию, соответствующую языку.
    /// </summary>
    public CultureInfo GetCultureInfo() 
        => CultureInfo.GetCultureInfo(Name);

    /// <summary>
    /// Конструктор, запрещающий создание объекта без параметров.
    /// </summary>
    // ReSharper disable once UnusedMember.Local
    private Lang()
    {
        Name = null!;
        ShortName = null!;
        DisplayName = null!;
    }
    
    /// <summary>
    /// Конструктор.
    /// </summary>
    private Lang(Lang lang)
    {
        Name = lang.Name;
        ShortName = lang.ShortName;
        DisplayName = lang.DisplayName;
    }

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="cultureInfo"><see cref="CultureInfo"/>, в соответствии с которой устанавливаем локализацию.</param>
    /// <param name="currentCultureInfo"><see cref="CultureInfo"/>, в соответствии с которой локализуем
    /// свойство <see cref="ShortName"/>.</param>
    public Lang(CultureInfo cultureInfo, CultureInfo? currentCultureInfo = null)
    {
        Name = cultureInfo.Name;
        ShortName = cultureInfo.TwoLetterISOLanguageName;
        
        var currentUiCulture = CultureInfo.CurrentUICulture;

        if (currentCultureInfo != null)
        {
            CultureInfo.CurrentUICulture = currentCultureInfo;
        }

        DisplayName = cultureInfo.Parent.DisplayName.ToCamelCase();
        CultureInfo.CurrentUICulture = currentUiCulture;
    }

    /// <summary>
    /// Перевод языка в соответствии с локализацией <paramref name="cultureInfo"/>.
    /// </summary>
    /// <remarks>
    /// По сути - локализуем свойство <see cref="DisplayName"/> в соответствии с заданной локализацией.
    /// </remarks>
    public Lang Translate(CultureInfo cultureInfo)
    {
        var currentUiCulture = CultureInfo.CurrentUICulture;
        
        CultureInfo.CurrentUICulture = cultureInfo;
        DisplayName = GetCultureInfo().Parent.DisplayName.ToCamelCase();

        CultureInfo.CurrentUICulture = currentUiCulture;

        return this;
    }

    
    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType()) return false;

        return Equals((Lang)obj);
    }

    
    /// <inheritdoc />
    public bool Equals(Lang? other)
    {
        return string.Equals(Name, other?.Name, StringComparison.OrdinalIgnoreCase)
               && string.Equals(ShortName, other?.ShortName, StringComparison.OrdinalIgnoreCase)
               && string.Equals(DisplayName, other?.DisplayName, StringComparison.OrdinalIgnoreCase);
    }

    
    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(Name, ShortName);
    }

    /// <summary>
    /// Клонирование языка.
    /// </summary>
    public Lang Clone()
        => new(this);
    
    
    /// <inheritdoc />
    object ICloneable.Clone() {
        return Clone();
    }


    /// <inheritdoc />
    public override string ToString() 
        => DisplayName;
}