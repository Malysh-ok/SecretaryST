using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Threading;
using AppDomain.AppEntities;
using AppDomain.AppExceptions;
using AppDomain.AppUseCases._Contracts;
using AppDomain.AppUseCases.Services;
using Common.WpfModule.Ui.Services;
using Common.WpfModule.Ui.Views._Contracts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Presentation.ViewModels.Shared.Infrastructure;
using Presentation.ViewModels.Shared.Messages;
using Serilog;

namespace Presentation.ViewModels.AppSetting;

/// <summary>
/// ViewModel для представления с настройками приложения.
/// </summary>
// ReSharper disable once InconsistentNaming
public class AppSettingVM : ObservableRecipient, IRecipient<LocalizationMessage>
{
    private readonly IViewWithResources _view;
    private readonly AppSettingsService _appSettingsService;
    private readonly IAppErrorMsgProvider _appErrorMsgProvider;
    private readonly AppLocalizationService _appLocalizationService;
    private readonly ViewLocalizationService _viewLocalizationService;
    private readonly ViewModelHelper _viewModelHelper;
    
    /// <summary>
    /// Подавляет отправку сообщения <see cref="LocalizationMessage"/> при установке <see cref="CurrLang"/>.
    /// Используется в <see cref="Receive"/> для предотвращения повторной отправки того же сообщения и циклического вызова.
    /// </summary>
    private bool _suppressSend;

    /// <summary>
    /// Коллекция доступных языков.
    /// </summary>
    public ObservableCollection<Lang> Languages
    {
        get;
        private set => SetProperty(ref field, value);
    } = [];

    /// <summary>
    /// Текущий язык локализация.
    /// </summary>
    public Lang? CurrLang
    {
        get;
        set
        {
            var oldLang = field;
            if (SetProperty(ref field, value) && ! _suppressSend)
            {
                // Оповещаем все представления (окна) приложения о смене локализации
                Messenger.Send(new LocalizationMessage(value ?? _appLocalizationService.DefaultLang, oldLang));
            }
        }
    }

    // TODO: Временно - визуализация текущего языка
    public Lang? DisplayMsg
    {
        get;
        private set => SetProperty(ref field, value);
    } = null!;

    /// <summary>
    /// Конструктор.
    /// </summary>
    public AppSettingVM(
        IViewWithResources view,
        ILogger logger,
        IAppErrorMsgProvider appErrorMsgProvider,
        AppSettingsService appSettingsService,
        StatusBarService statusBarService)
    {
        _view = view;
        _appErrorMsgProvider = appErrorMsgProvider;
        _appSettingsService = appSettingsService;
        _appLocalizationService = appSettingsService.AppLocalization;
        _viewLocalizationService = new ViewLocalizationService(appSettingsService);
        _viewModelHelper = new ViewModelHelper(logger, appErrorMsgProvider, statusBarService);

        // Подписываемся на получение сообщений
        Messenger.Register(this);
        
        // Получаем язык локализации из настроек и отправляем начальное сообщение
        var initialLang = _appLocalizationService.CurrentLang;
        Messenger.Send(new LocalizationMessage(initialLang, null));
    }
    
    /// <summary>
    /// Получаем сообщение с экземпляром <see cref="LocalizationMessage"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="Receive"/> извне прийти не может.
    /// </remarks>
    public async void Receive(LocalizationMessage message)
    {
        try
        {
            // Устанавливаем текущий язык
            var langResult = _appLocalizationService.SetCurrentLang(message.Lang);
            if (langResult)
            {
                // Локализация представления асинхронно в UI-потоке, но без блокировки
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    if (! _viewLocalizationService.LocalizeView(_view, message.Lang))
                    {
                        // Если локализовать не получилось - восстанавливаем текущий язык
                        _appLocalizationService.SetCurrentLang(message.OldLang);
                    }
                }, DispatcherPriority.Normal);
            }
            else
            {
                // Пишем в статус-бар и лог об ошибке
                var exception = _appErrorMsgProvider.CreateException(AppErrorCodes.LocalizingError,
                    langResult.Excptn, args: this.GetType().Name);
                _viewModelHelper.HandleException(exception, this.ToString(), nameof(Receive));
            }

            // После завершения локализации устанавливаем язык культуры
            var currLang = _appLocalizationService.CurrentLang;
            CultureInfo.CurrentUICulture = currLang.GetCultureInfo();
            CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture;
            
            // Визуализируем язык
            DisplayMsg = currLang;
        
            // Пересоздаем коллекцию языков
            Languages = new ObservableCollection<Lang>(_appLocalizationService.Languages.Values);
            
            // Устанавливаем язык локализации с подавлением отправки сообщения
            _suppressSend = true;
            CurrLang = currLang;
            _suppressSend = false;
        }
        catch (Exception ex)
        {
            // Пишем в статус-бар и лог об ошибке
            var exception = _appErrorMsgProvider.CreateException(AppErrorCodes.LocalizingError, ex, args: this.GetType().Name);
            _viewModelHelper.HandleException(exception, this.ToString(), nameof(Receive));
        }
    }
}