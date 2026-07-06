using System.Globalization;
using AppDomain.AppExceptions;
using AppDomain.Setting.Entities;
using Common.BaseExtensions;
using Common.BaseExtensions.Collections;
using Common.BaseComponents.Components;

// ReSharper disable MemberCanBePrivate.Global

namespace AppDomain.Setting.Services;

/// <summary>
/// Сервис локализации приложения.
/// </summary>
public class AppLocalizationService
{
    /// <summary>
    /// Название языка по умолчанию.
    /// </summary>
    private const string DefaultLangName = "en-US";
    
    /// <summary>
    /// Название русского языка.
    /// </summary>
    private const string RuLangName = "ru-RU";
    
    /// <summary>
    /// Делегат, получающий наименование из настроек.
    /// </summary>
    private readonly Func<string> _getLangDelegate;

    /// <summary>
    /// Делегат, устанавливающий наименование в настройках.
    /// </summary>
    private readonly Action<string> _setLangDelegate;

    /// <summary>
    /// Создание языка по умолчанию.
    /// </summary>
    private Lang CreateDefaultLang() => new(CultureInfo.GetCultureInfo(DefaultLangName));
    
    /// <summary>
    /// Индекс текущего языка в словаре <see cref="Languages"/>.
    /// </summary>
    private LangEnm _langNumber = 0;
    
    /// <summary>
    /// Список поддерживаемых приложением языков.
    /// </summary>
    public Dictionary<LangEnm, Lang> Languages { get; private set; }

    /// <summary>
    /// Конструктор, запрещающий создание экземпляра без параметров.
    /// </summary>
    // ReSharper disable once UnusedMember.Local
    private AppLocalizationService()
    {
        _getLangDelegate = null!;
        _setLangDelegate = null!;
        Languages = new Dictionary<LangEnm, Lang>();
    }

    /// <summary>
    /// Конструктор.
    /// </summary>
    public AppLocalizationService(Func<string> getLangDelegate, Action<string> setLangDelegate) : this()
    {
        _getLangDelegate = getLangDelegate;
        _setLangDelegate = setLangDelegate;

        // Текущая локализация
        var appCultureInfo = CreateDefaultLang().GetCultureInfo();
        
        Languages = new Dictionary<LangEnm, Lang>
        {
            { LangEnm.En, new Lang(CultureInfo.GetCultureInfo(DefaultLangName), appCultureInfo) },
            { LangEnm.Ru, new Lang(CultureInfo.GetCultureInfo(RuLangName), appCultureInfo) },
            { LangEnm.Fr, new Lang(CultureInfo.GetCultureInfo("fr-FR"), appCultureInfo) },
        };
    }

    /// <summary>
    /// Перевод наименований всех доступных языков приложения в соответствии с локализацией <paramref name="cultureInfo"/>.
    /// </summary>
    /// <remarks>
    /// По сути - локализация свойства <see cref="Lang.DisplayName"/> доступных языков в соответствии с заданной локализацией.
    /// </remarks>
    public void Translate(CultureInfo cultureInfo)
    {
        Languages.ForEach(item => item.Value.Translate(cultureInfo));
    }

    /// <summary>
    /// Получить язык по умолчанию.
    /// </summary>
    public Lang GetDefaultLang() => GetFromName(DefaultLangName) 
                                    ?? Languages.FirstOrDefault(x => x.Value.Name == DefaultLangName).Value;

    /// <summary>
    /// Получить текущий язык.
    /// </summary>
    public Lang? GetCurrentLang() => Languages.GetValueOrDefault(_langNumber);

    /// <summary>
    /// Получить текущий язык или язык по умолчанию.
    /// </summary>
    public Lang GetCurrentOrDefaultLang() => Languages.GetValueOrDefault(_langNumber, GetDefaultLang());

    /// <summary>
    /// Проверка валидности языка.
    /// </summary>
    public Result<bool> ValidateLang(Lang? lang)
    {
        return lang != null && Languages.KeyByValue(lang, out _)
            ? Result<bool>.Done(true)
            : Result<bool>.Fail(GetCurrentOrDefaultLang().GetCultureInfo().IsRu()
                ? "Данный язык отсутствует в списке доступных языков."
                : "This language is missing in the list of available languages.");
    }

    /// <summary>
    /// Проверка валидности языка по его имени.
    /// </summary>
    public Result<bool> ValidateLang(string? langName)
    {
        return GetFromName(langName) != null
            ? Result<bool>.Done(true)
            : Result<bool>.Fail(GetCurrentOrDefaultLang().GetCultureInfo().IsRu()
                ? "Данный язык отсутствует в списке доступных языков."
                : "This language is missing in the list of available languages.");
    }

    /// <summary>
    /// Получить язык по его имени.
    /// </summary>
    /// <remarks>
    /// Если язык не найден и isExceptionIfNotFound = true, генерируется исключение.
    /// </remarks>
    public Lang? GetFromName(string? langName, bool isExceptionIfNotFound = false)
    {
        var lang = Languages.Values.FirstOrDefault(x => x.Name == langName);

        if (lang is null && isExceptionIfNotFound)
            throw new AppException(GetCurrentOrDefaultLang().GetCultureInfo().IsRu()
                ? $"Локализация с именем {langName} не найдена."
                : "Localization with name {name} is not found.");

        return lang;
    }

    /// <summary>
    /// Получить язык по его имени или язык по умолчанию.
    /// </summary>
    public Lang GetFromNameOrDefault(string? langName)
    {
        return GetFromName(langName!) ?? GetDefaultLang();
    }

    /// <summary>
    /// Установить текущий язык.
    /// </summary>
    /// <remarks>
    /// Если язык <paramref name="lang"/> не найден, то текущий язык не изменяется.
    /// </remarks>
    public Lang SetCurrentLang(Lang? lang, bool isExceptionIfNotFound = false)
    {
        var langNumber = Languages.FirstOrDefault(x =>  lang != null && x.Value.Name == lang.Name).Key;
        switch (langNumber)
        {
            case 0 when isExceptionIfNotFound:
                throw new AppException(GetCurrentOrDefaultLang().GetCultureInfo().IsRu()
                    ? $"Локализация {nameof(lang)} не найдена."
                    : $"Localization {nameof(lang)} is not found.");
            case > 0:
                _langNumber = langNumber;
                SetLangToSetting(lang!.Name);        // сохраняем локализацию в настройках
                break;
        }

        return GetCurrentOrDefaultLang();
    }

    /// <summary>
    /// Установить текущий язык по его имени.
    /// </summary>
    /// <remarks>
    /// Если язык с именем <paramref name="langName"/> не найден, то текущий язык не изменяется.
    /// </remarks>
    public Lang SetCurrentLangFromName(string? langName, bool isExceptionIfNotFound = false)
    {
        var lang = GetFromName(langName, isExceptionIfNotFound);

        return lang != null
            ? SetCurrentLang(lang)
            : GetCurrentOrDefaultLang();
    }
    
    /// <summary>
    /// Получаем наименование языка из настроек.
    /// </summary>
    public string GetLangFromSetting() => _getLangDelegate();
    
    /// <summary>
    /// Сохраняем наименование языка в настройки.
    /// </summary>
    public void SetLangToSetting(string langName) => _setLangDelegate(langName);
}