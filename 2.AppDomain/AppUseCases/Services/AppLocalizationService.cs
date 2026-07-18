using System.Globalization;
using AppDomain.AppEntities;
using AppDomain.AppExceptions;
using AppDomain.AppUseCases._Contracts;
using Common.BaseComponents.Components;
using Common.BaseComponents.Components.Exceptions;
using Common.BaseExtensions;
using Common.BaseExtensions.Collections;

// ReSharper disable MemberCanBePrivate.Global

namespace AppDomain.AppUseCases.Services;

/// <summary>
/// Сервис локализации приложения.
/// </summary>
public class AppLocalizationService
{
    /// <summary>
    /// Ключ текущего языка в словаре <see cref="Languages"/>.
    /// </summary>
    private LangEnm _currentLangKey;

    /// <summary>
    /// Провайдер сообщений об ошибках предметной области приложения.
    /// </summary>
    private readonly IAppErrorMsgProvider _appErrorMsgProvider;

    /// <summary>
    /// Делегат, получающий наименование языка из настроек.
    /// </summary>
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly Func<string?> _getLangDelegate;

    /// <summary>
    /// Делегат, сохраняющий наименование языка в настройках.
    /// </summary>
    private readonly Action<string> _setLangDelegate;
    
    /// <summary>
    /// Переводит отображаемые имена всех языков в соответствии с указанной культурой.
    /// </summary>
    private void Translate(CultureInfo targetCulture)
    {
        Languages.ForEach(item => item.Value.Translate(targetCulture));
    }

    /// <summary>
    /// Получение исключения "Язык = null".
    /// </summary>
    private AppException GetLangIsNullEx()
    {
        var ex =  _appErrorMsgProvider.CreateException(
            AppErrorCodes.LanguageIsNull);
        
        return ex;
    }

    /// <summary>
    /// Получение исключения "Язык не найден".
    /// </summary>
    /// <param name="langName">Наименование языка.</param>
    private AppException GetLangNotFoundEx(string langName)
    {
        var ex =  _appErrorMsgProvider.CreateException(
            AppErrorCodes.LanguageNotFound, args: langName);
        
        return ex;
    }
    
    /// <summary>
    /// Название языка по умолчанию.
    /// </summary>
    public const string DefaultLangName = "en-US";
    
    /// <summary>
    /// Название русского языка.
    /// </summary>
    public const string RuLangName = "ru-RU";

    /// <summary>
    /// Словарь поддерживаемых языков (ключ – идентификатор языка).
    /// </summary>
    public Dictionary<LangEnm, Lang> Languages { get; }

    /// <summary>
    /// Язык по умолчанию.
    /// </summary>
    public Lang DefaultLang => GetLangFromName(DefaultLangName).Value!;

    /// <summary>
    /// Текущий язык.
    /// </summary>
    public Lang CurrentLang => Languages.GetValueOrDefault(_currentLangKey)!;

    /// <summary>
    /// Исключение, возникшее при инициализации сервиса.
    /// Если инициализация прошла успешно, значение равно <c>null</c>.
    /// </summary>
    public AppException? InitializationException { get; private set; }
    
    /// <summary>
    /// Флаг, проверяющий, была ли инициализация сервиса успешной.
    /// </summary>
    public bool IsInitializedSuccessfully => InitializationException == null;
    
    
    /// <summary>
    /// Конструктор.
    /// </summary>
    public AppLocalizationService(IAppErrorMsgProvider appErrorMsgProvider, 
        Func<string?> getLangDelegate, Action<string> setLangDelegate)
    {
        _appErrorMsgProvider = appErrorMsgProvider;
        _getLangDelegate = getLangDelegate;
        _setLangDelegate = setLangDelegate;

        // Инициализация словаря языков
        var defaultCulture = CultureInfo.GetCultureInfo(DefaultLangName);
        Languages = new Dictionary<LangEnm, Lang>
        {
            { LangEnm.En, new Lang(CultureInfo.GetCultureInfo(DefaultLangName), defaultCulture) },
            { LangEnm.Ru, new Lang(CultureInfo.GetCultureInfo(RuLangName), defaultCulture) },
            { LangEnm.Fr, new Lang(CultureInfo.GetCultureInfo("fr-FR"), defaultCulture) },
        };
        
        // Безопасная инициализация: читаем из настроек,
        // если не получилось – ставим дефолтный язык и устанавливаем InitializationException
        var initialLangName = _getLangDelegate();
        var initialLangResult = GetLangFromName(initialLangName);
        Lang initialLang;
        if (initialLangResult)
        {
            initialLang = initialLangResult.Value!;
        }
        else
        {
            initialLang = DefaultLang;
            
            // Устанавливаем InitializationException и меняем его тип на предупреждение
            InitializationException = initialLangResult.Excptn as AppException;
            InitializationException!.ResetExcptnType(ExcptnTypeEnm.Warning);
        }
        _currentLangKey = Languages.First(kvp => kvp.Value.Equals(initialLang)).Key;
        Translate(initialLang.GetCultureInfo());
    }
    
    /// <summary>
    /// Проверяет, поддерживается ли переданный язык.
    /// </summary>
    /// <remarks>
    /// Проверяется наличие языка в списке доступных языков.
    /// </remarks>
    public Result<bool> ValidateLang(Lang? lang)
    {
        if (lang == null)
            return Result<bool>.Fail(GetLangIsNullEx());

        return Languages.ContainsValue(lang)
            ? Result<bool>.Done(true)
            : Result<bool>.Fail(GetLangNotFoundEx(lang.Name));
    }

    /// <summary>
    /// Возвращает язык по его имени.
    /// </summary>
    public Result<Lang> GetLangFromName(string? langName)
    {
        if (langName.IsNullOrEmpty())
            return Result<Lang>.Fail(GetLangIsNullEx());
        
        var lang = Languages.Values.FirstOrDefault(l => l.Name == langName);

        return lang is null 
            ? Result<Lang>.Fail(GetLangNotFoundEx(langName!)) 
            : Result<Lang>.Done(lang);
    }

    /// <summary>
    /// Получает язык по его имени или, если такой язык не найден, язык по умолчанию.
    /// </summary>
    public Lang GetLangFromNameOrDefault(string? langName)
    {
        var langResult = GetLangFromName(langName);
        return langResult
            ? langResult.Value!
            : DefaultLang;
    }

    /// <summary>
    /// Устанавливает текущий язык.
    /// </summary>
    /// <remarks>
    /// <para>Если язык <paramref name="lang"/> не найден, то текущий язык не изменяется.</para>
    /// </remarks>
    public Result<bool> SetCurrentLang(Lang? lang)
    {
        if (lang == null)
            return Result<bool>.Fail(GetLangIsNullEx());

        var key = Languages.FirstOrDefault(kvp => kvp.Value.Equals(lang)).Key;
        if (key == 0)
            return Result<bool>.Fail(GetLangNotFoundEx(lang.Name));

        _currentLangKey = key;
        _setLangDelegate(lang.Name);
        Translate(lang.GetCultureInfo());
        
        return Result<bool>.Done(true);
    }
}