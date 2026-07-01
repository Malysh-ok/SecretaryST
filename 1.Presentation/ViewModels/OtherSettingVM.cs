using System.Windows;
using System.Windows.Threading;
using AppDomain.AppExceptions;
using AppDomain.Phrases;
using AppDomain.Setting.Entities;
using AppDomain.Setting.Services;
using Common.BaseExtensions;
using Common.WpfModule.Ui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Presentation.ViewModels.Common;
using Presentation.ViewModels.Common.Messages;
using Serilog;

namespace Presentation.ViewModels;

/// <summary>
/// ViewModel для представления с настройками.
/// </summary>
// ReSharper disable once InconsistentNaming
public class OtherSettingVM : ObservableRecipient, IRecipient<LocalizationMessage>
{
    private readonly IViewWithResources _view;
    private readonly ILogger _logger;
    private readonly AppSettingService _appSetting;

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
    public OtherSettingVM(IViewWithResources view, 
        ILogger logger, 
        AppSettingService appSetting)
    {
        _view = view;
        _logger = logger;
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
    public async void Receive(LocalizationMessage message)
    {
        try
        {
            DisplayMsg = message.Lang;
        
            // Локализация представления асинхронно в UI-потоке, но без блокировки
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (! _localizationHelper.LocalizeView(_view, message.Lang))
                {
                    // Если локализовать не получилось - восстанавливаем предыдущий язык
                    DisplayMsg = message.OldLang;
                }
            }, DispatcherPriority.Background);
        }
        catch (Exception ex)
        {
            // Пишем в и лог об ошибке
            var exception = new AppException(AppPhrases.LocalizingError.Format(nameof(OtherSettingVM)), ex);
            _logger.Error(exception,
                "{class}.{method}.",
                typeof(OtherSettingVM), nameof(Receive));
        }
    }
}