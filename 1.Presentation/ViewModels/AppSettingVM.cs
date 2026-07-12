using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Threading;
using AppDomain.AppAssets.Strings;
using AppDomain.AppEntities;
using AppDomain.AppExceptions;
using AppDomain.AppUseCases._Contracts;
using AppDomain.AppUseCases.Services;
using Common.WpfModule.Ui.Services;
using Common.WpfModule.Ui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Presentation.ViewModels.Common;
using Presentation.ViewModels.Common.Messages;
using Serilog;

namespace Presentation.ViewModels;

/// <summary>
/// ViewModel для представления с настройками приложения.
/// </summary>
// ReSharper disable once InconsistentNaming
public class AppSettingVM : ObservableRecipient, IRecipient<LocalizationMessage>
{
    private readonly IViewWithResources _view;
    private readonly ILogger _logger;
    private readonly StatusBarService _statusBarService;
    private readonly AppSettingService _appSettingService;
    private readonly IAppErrorMsgProvider _appErrorMsgProvider;
    private readonly LocalizationHelper _localizationHelper;
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
                Messenger.Send(new LocalizationMessage(value ?? _appSettingService.AppLocalization.GetDefaultLang(), oldLang));
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
        AppSettingService appSettingService,
        StatusBarService statusBarService)
    {
        _view = view;
        _logger = logger;
        _appErrorMsgProvider = appErrorMsgProvider;
        _appSettingService = appSettingService;
        _statusBarService = statusBarService;
        _localizationHelper = new LocalizationHelper(appSettingService);
        _viewModelHelper = new ViewModelHelper(logger, appErrorMsgProvider, statusBarService);

        // Подписываемся на получение сообщений
        Messenger.Register(this);
        
        // Отправляем начальное сообщение
        var localization = appSettingService.AppLocalization;
        var initialLang = localization.GetCurrentOrDefaultLang();
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
            // Визуализируем язык
            DisplayMsg = message.Lang;
            
            var localization = _appSettingService.AppLocalization;
            var lang = message.Lang;       // устанавливаемый язык
            var oldLang = message.OldLang; // предыдущий язык
        
            // Перевод наименований всех доступных языков приложения в соответствии с устанавливаемым языком
            localization.Translate(localization.SetCurrentLang(lang).GetCultureInfo());
        
            // Локализация представления асинхронно в UI-потоке, но без блокировки
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (! _localizationHelper.LocalizeView(_view, message.Lang))
                {
                    // Если локализовать не получилось - визуализируем предыдущий язык
                    DisplayMsg = message.OldLang;
                    
                    // Если локализовать не получилось - переводим доступные языки обратно,
                    // в соответствии с предыдущим языком
                    localization.Translate(localization.SetCurrentLang(oldLang).GetCultureInfo());
                }
            }, DispatcherPriority.Background);
            
            // После завершения локализации обновляем культуру, UI, и фразы приложения
            var currLang = localization.GetCurrentOrDefaultLang();
            CultureInfo.CurrentUICulture = currLang.GetCultureInfo();
            CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture;
            AppPhrases.Culture = CultureInfo.CurrentUICulture;
        
            // Пересоздаем коллекцию языков
            Languages = new ObservableCollection<Lang>(localization.Languages.Values);
            // Устанавливаем язык локализации с подавлением отправки сообщения
            _suppressSend = true;
            CurrLang = currLang;
            _suppressSend = false;
        }
        catch (Exception ex)
        {
            // Пишем в и лог об ошибке
            var exception =
                _appErrorMsgProvider.CreateException(AppErrorCodes.LocalizingError, ex, this.GetType().Name);
            _viewModelHelper.HandleException(exception, this.ToString(), nameof(Receive));
        }
    }
}