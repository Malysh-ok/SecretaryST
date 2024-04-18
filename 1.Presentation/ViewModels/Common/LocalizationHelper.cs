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

    /// <summary>
    /// Получаем наименование ресурса локализации.
    /// </summary>
    /// <param name="lang">Локализация, для которой получаем наименование.</param>
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
    /// Локализация представления (окна).
    /// </summary>
    /// <remarks>
    /// Если ресурс с локализацией не найден - представление остается в прежней локализации.
    /// </remarks>
    public async Task<bool> LocalizeView(IViewWithResources view, Lang localisation)
    {
        return await Task.Run<bool>(() =>
        {
            // Используем Dispatcher, чтобы получить UI-поток
            var result = Application.Current.Dispatcher.Invoke((Func<bool>)(() =>
            {
                // Получаем ResourceDictionary локализации из App
                var regex = new Regex(@"lang\..*xaml", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                var appResources = Application.Current.Resources.MergedDictionaries.FirstOrDefault(r =>
                    regex.IsMatch(r.Source.OriginalString));

                // Если у приложения отсутствует ресурс с локализацией - выходим
                if (appResources?.Source == null)
                    return false;
                
                try
                {
                    //Создаем ResourceDictionary для нового языкового стандарта
                    var viewResources = new ResourceDictionary
                    {
                        Source = new Uri(
                            regex.Replace(appResources.Source.OriginalString, 
                                GetLangResourceName(localisation)),
                            UriKind.Relative)
                    };

                    // Удаляем текущий ResourceDictionary локализации, добавляем новый
                    view.Resources.MergedDictionaries.Remove(appResources);
                    view.Resources.MergedDictionaries.Add(viewResources);

                    return true;
                }
                catch
                {
                    return false;   // если ошибка - выходим
                }
            }));
            
            return result;
        });
    }
}