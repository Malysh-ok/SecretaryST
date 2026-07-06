using System.Text.RegularExpressions;
using System.Windows;
using AppDomain.Setting.Entities;
using AppDomain.Setting.Services;
using Common.WpfModule.Ui.Views;

namespace Presentation.ViewModels.Common;

/// <summary>
/// Помощник локализации представлений.
/// </summary>
public class LocalizationHelper
{
    // Настройки приложения
    private readonly AppSettingService _appSetting;

    // Базовый  для поиска языкового словаря ресурсов
    private Regex _regex = new Regex(@"lang\..*xaml", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Получаем наименование ресурса локализации.
    /// </summary>
    /// <param name="lang">Язык, для которого получаем наименование.</param>
    private string GetLangResourceName(Lang lang)
    {
        var delim = lang.ShortName == _appSetting.AppLocalization.GetDefaultLang().ShortName
            ? string.Empty
            : $".{lang.ShortName}";
        return $"lang{delim}.xaml";
    }

    /// <summary>
    /// Конструктор.
    /// </summary>
    public LocalizationHelper(AppSettingService appSetting)
    {
        _appSetting = appSetting;
    }

    /// <summary>
    /// Локализует представление (окно), заменяя словарь ResourceDictionary языка.
    /// </summary>
    /// <param name="view">Представление, которое нужно локализовать.</param>
    /// <param name="lang">Целевой язык.</param>
    /// <returns>true - локализация успешна, false - ошибка.</returns>
    /// <remarks>
    /// Если словарь с локализацией не найден - представление остается в прежней локализации.
    /// </remarks>
    public bool LocalizeView(IViewWithResources view, Lang lang)
    {
        // Получаем словарь локализации из App
        var appResDic = Application.Current.Resources.MergedDictionaries.FirstOrDefault(r =>
            _regex.IsMatch(r.Source.OriginalString));

        // Если у приложения отсутствует словарь с локализацией - выходим
        if (appResDic?.Source == null)
            return false;

        try
        {
            // Создаем новый словарь с локализацией для целевого языка
            var newResDic = new ResourceDictionary
            {
                Source = new Uri(
                    _regex.Replace(appResDic.Source.OriginalString, 
                        GetLangResourceName(lang)), UriKind.Relative)
            };

            // Находим и удаляем все языковые словари в представлении
            var toRemoveResDicList = view.Resources.MergedDictionaries
                               .Where(r => r.Source != null && _regex.IsMatch(r.Source.OriginalString))
                               .ToList();
            toRemoveResDicList.ForEach(d => view.Resources.MergedDictionaries.Remove(d));
            
            // Добавляем новый словарь
            view.Resources.MergedDictionaries.Add(newResDic);

            return true;
        }
        catch
        {
            return false; // если ошибка - выходим
        }
    }

    /// <summary>
    /// Получение строки в текущей локализации из словаря <see cref="ResourceDictionary"/> представления.
    /// </summary>
    /// <param name="view">Представление, откуда получаем строку.</param>
    /// <param name="key">Ключ в ресурсе.</param>
    /// <param name="defaultValue">Значение, получаемое при неудачном поиске в словаре.</param>
    public string GetLocalizedString(IViewWithResources view, string key, string defaultValue = "")
    {            
        // Получаем словарь текущей локализации из представления
        var viewResDic = view.Resources.MergedDictionaries.FirstOrDefault(r =>
            _regex.IsMatch(r.Source.OriginalString));
        
        // Получаем локализованную строку, если она есть 
        var result = defaultValue;
        if (viewResDic != null && viewResDic.Contains(key))
            result = (viewResDic[key] as string);

        return result ?? defaultValue;
    }
}