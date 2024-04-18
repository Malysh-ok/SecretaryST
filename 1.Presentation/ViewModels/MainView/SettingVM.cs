using System.Collections.ObjectModel;
using AppDomain.Setting.Entities;
using AppDomain.Setting.Services;
using Common.BaseExtensions.Collections;
using Common.WpfModule.Ui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Presentation.ViewModels.Common;

namespace Presentation.ViewModels.MainView;

/// <summary>
/// ViewModel для меню ленты "Настройки".
/// </summary>
// ReSharper disable once InconsistentNaming
public class SettingVM : ObservableRecipient, IRecipient<LocalizationMessage>
{
    private readonly AppSettingService _appSetting;
    
    private readonly IViewWithResources _view;

    private readonly LocalizationHelper _localizationHelper;

    /// <summary>
    /// Список доступных языков.
    /// </summary>
    public ObservableCollection<Lang> Languages { get; private set; }

    private Lang? _currLang;
    /// <summary>
    /// Текущая локализация.
    /// </summary>
    public Lang? CurrLang
    {
        get => _currLang;
        set
        {
            var oldLang = _currLang?.Clone() ?? _appSetting.AppLocalization.GetDefaultLang();
            if (SetProperty(ref _currLang, value))
            {
                // Оповещаем все представления (окна) приложения о смене локализации
                Messenger.Send(new LocalizationMessage(value ?? _appSetting.AppLocalization.GetDefaultLang(), oldLang ));
            }
        }
    }
    
    /// <summary>
    /// Конструктор.
    /// </summary>
    public SettingVM(IViewWithResources view, AppSettingService appSetting)
    {
        _view = view;
        _appSetting = appSetting;
        var localization = appSetting.AppLocalization;
        
        // Установка русской локализации
        Languages = new ObservableCollection<Lang>(localization.Languages.Values);
        var langName = localization.GetLangFromSetting();
        _currLang = localization.SetCurrentLangFromName(langName);
        _localizationHelper = new LocalizationHelper(appSetting);

        // Подписываемся на получение сообщений
        Messenger.Register(this);
        
        // Посылаем сообщение о смене локализации
        Messenger.Send( new LocalizationMessage(_currLang, localization.GetDefaultLang()));
    }
    
    /// <summary>
    /// Получаем сообщение с экземпляром <see cref="LocalizationMessage"/>.
    /// </summary>
    public async void Receive(LocalizationMessage message)
    {
        var localization = _appSetting.AppLocalization;
        
        var lang = message.Lang;            // устанавливаемый язык
        var oldLang = message.OldLang;      // предыдущий язык
        
        // Перевод всех доступных языков приложения в соответствии с устанавливаемым языком
        localization.Translate(localization.SetCurrentLang(lang).GetCultureInfo());

        if (!await _localizationHelper.LocalizeView(_view, lang))
        {
            // Если локализовать не получилось - переводим доступные языки обратно,
            // в соответствии с предыдущим языком
            localization.Translate(localization.SetCurrentLang(oldLang).GetCultureInfo());
        }
        var currLang = localization.GetCurrentOrDefaultLang();
        
        // Изменяем свойства, используемые для биндинга
        SetProperty(ref _currLang!, null);      // отвязываем привязку свойства к событию изменения значения 
        Languages.Clear();
        localization.Languages.ForEach(item => Languages.Add(item.Value));
        SetProperty(ref _currLang, currLang, nameof(CurrLang));     // вновь привязываем
    }
}