using System.Windows.Input;
using System.Windows.Media;
using AppDomain.AppExceptions;
using AppDomain.AppUseCases._Contracts;
using AppDomain.AppUseCases.Services;
using Common.BaseComponents.Components.Exceptions;
using Common.WpfModule.Ui.Services;
using Common.WpfModule.Ui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Presentation.ViewModels._Contracts;
using ProblemDomain.UseCases.Services;
using Serilog;
// ReSharper disable InconsistentNaming

namespace Presentation.ViewModels.MainView;

/// <summary>
/// ViewModel для основного представления.
/// </summary>
public sealed class MainVM : ObservableRecipient, IDisposable
{
    /// <summary>
    /// Логгер.
    /// </summary>
    // ReSharper disable once NotAccessedField.Local
    private ILogger _logger = null!;

    /// <summary>
    /// Провайдер сообщений об ошибках предметной области приложения.
    /// </summary>
    private readonly IAppErrorMsgProvider _appErrorMsgProvider = null!;

    /// <summary>
    /// Модель представления Backstage.
    /// </summary>
    public BackstageVM BackstageVM { get; } = null!;

    /// <summary>
    /// Модель представления настроек.
    /// </summary>
    public SettingVM SettingVM { get; } = null!;

    /// <summary>
    /// Сервис статус-бара.
    /// </summary>
    public StatusBarService _statusBarService { get; } = null!;

    // TODO: Временная команда PinkCommand (кнопка панели быстрого доступа)
    public ICommand PinkCommand { get; } = null!;

    /// <summary>
    /// Конструктор, запрещающий создания экземпляра без параметров.
    /// </summary>
    // ReSharper disable once UnusedMember.Local
    private MainVM()
    {
    }
    
    /// <summary>
    /// Конструктор.
    /// </summary>
    private MainVM(ILogger logger, IAppErrorMsgProvider appErrorMsgProvider,
            StatusBarService statusBarService, BackstageVM backstageVm, SettingVM settingVm 
        )
    {
        BackstageVM = backstageVm;
        SettingVM = settingVm;
        _statusBarService = statusBarService;
        _logger = logger;
        _appErrorMsgProvider = appErrorMsgProvider;

        PinkCommand = new AsyncRelayCommand(OnPink);
    }

    /// <summary>
    /// Создание экземпляра класса (фабричный метод).
    /// </summary>
    /// <param name="view">Представление, реализующее ресурсы.</param>
    /// <param name="statusBarService">Сервис статус-бара.</param>
    /// <param name="logger">Логгер.</param>
    /// <param name="appSettingService">Настройки приложения.</param>
    /// <param name="exceptionsProvider">Поставщик исключения.</param>
    /// <param name="competitionDataService">Сервис для работы с соревнованиями.</param>
    /// <param name="refereeService">Сервис для работы с судьями.</param>
    /// <param name="sportEventService">Сервис для работы с видами программы.</param>
    /// <param name="appErrorMsgProvider">Провайдер сообщений об ошибках слоя приложения.</param>
    public static MainVM Create(IViewWithResources view, 
        ILogger logger, 
        IExceptionsProvider exceptionsProvider,
        IAppErrorMsgProvider appErrorMsgProvider,
        AppSettingService appSettingService,
        StatusBarService statusBarService,
        CompetitionDataService competitionDataService,
        RefereeService refereeService,
        SportEventService sportEventService)
    {
        // TODO: Устанавливаем цвет кистей статус-бара (возможно изменим)
        statusBarService.ReSetBrushes(Brushes.Azure, Brushes.Azure,
            // new SolidColorBrush(Color.FromRgb(0xE6, 0x5C, 0x00)));
            Brushes.LightSalmon);

        // Создаем ViewModel's. Последовательность создания важна!
        var settingVM = new SettingVM(view, logger, exceptionsProvider, appErrorMsgProvider, appSettingService,
            statusBarService, competitionDataService, refereeService, sportEventService);
        var backstageVM = new BackstageVM(logger, exceptionsProvider, appErrorMsgProvider, 
            appSettingService, statusBarService, competitionDataService);
        var mainVM = new MainVM(logger, appErrorMsgProvider, statusBarService, backstageVM, settingVM);

        return mainVM;
    }
    
    /// <summary>
    /// TODO: OnPink - Временно
    /// </summary>
    private async Task OnPink()
    {
        // Пишем в статус-бар
        await _statusBarService.SetProgressAsync(50);
        // await StatusBarService.SetTextAsync("OnPink" + 
        //     " Если нужно добавить всплывающие подсказки, иконки и кликабельность" + 
        //     " Если нужно добавить всплывающие подсказки, иконки и кликабельность" + 
        //     " Если нужно добавить всплывающие подсказки, иконки и кликабельность", 
        //         BaseException.ExcptnType.Info, 0, false);
        await _statusBarService.SetTextAsync("OnPink " + _appErrorMsgProvider.GetMessage(AppErrorCodes.UnknownError),
            BaseException.ExcptnType.Info, 0, false);

        await Task.Delay(2000);
        await _statusBarService.SetProgressAsync(100);
        await Task.Delay(3000);
        await _statusBarService.SetProgressAsync(0);
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        // ReSharper disable once InvertIf
        if (disposing)
        {
            BackstageVM.Dispose();
            SettingVM.Dispose();
        }
    }
    
    ~MainVM() => Dispose(false);
}