using AppDomain.Setting.Entities;
using AppDomain.Setting.Services;
using Common.WpfModule.Ui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Presentation.ViewModels.Common;

namespace Presentation.ViewModels;

/// <summary>
/// ViewModel для представления с настройками.
/// </summary>
// ReSharper disable once InconsistentNaming
public class OtherSettingVM : ObservableRecipient, IRecipient<LocalizationMessage>
{
    private readonly AppSettingService _appSetting;

    private readonly IViewWithResources _view;

    private readonly LocalizationHelper _localizationHelper;

    // TODO: Временно
    public Lang DisplayMsg
    {
        get;
        private set => SetProperty(ref field, value);
    } = null!;

    /// <summary>
    /// Конструктор.
    /// </summary>
    public OtherSettingVM(IViewWithResources view, AppSettingService appSetting)
    {
        _view = view;
        _appSetting = appSetting;
        _localizationHelper = new LocalizationHelper(appSetting);

        // Подписываемся на получение сообщений
        Messenger.Register(this);

        var lang = appSetting.AppLocalization.GetCurrentOrDefaultLang();
        // В первый раз сразу вызываем "получатель" сообщений
        Receive(new LocalizationMessage(lang, lang));
    }
    
    /// <summary>
    /// Получаем сообщение с экземпляром <see cref="LocalizationMessage"/>.
    /// </summary>
    public void Receive(LocalizationMessage message)
    {
        DisplayMsg = message.Lang;
        
        // Локализуем представление
        _ = Task.Run(() =>
        {
            if (! _localizationHelper.LocalizeView(_view, message.Lang))
            {
                DisplayMsg = message.OldLang;
            }
        });

    }
}