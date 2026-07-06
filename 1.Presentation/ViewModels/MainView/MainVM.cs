using System.Windows.Input;
using System.Windows.Media;
using AppDomain.Phrases;
using AppDomain.Setting.Services;
using AppDomain.UseCases.Services;
using Common.BaseComponents.Components.Exceptions;
using Common.WpfModule.Ui.Services;
using Common.WpfModule.Ui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Presentation.ViewModels._Contracts;
using Serilog;
// ReSharper disable InconsistentNaming

namespace Presentation.ViewModels.MainView;

/// <summary>
/// ViewModel для основного представления.
/// </summary>
public sealed class MainVM : ObservableRecipient, IStatusBarDataProvider, IDisposable
{
    /// <summary>
    /// Логгер.
    /// </summary>
    // ReSharper disable once NotAccessedField.Local
    private ILogger _logger = null!;

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
    public StatusBarService StatusBarService { get; } = null!;

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
    private MainVM(BackstageVM backstageVm, SettingVM settingVm, StatusBarService statusBarService, ILogger logger)
    {
        BackstageVM = backstageVm;
        SettingVM = settingVm;
        StatusBarService = statusBarService;
        _logger = logger;
        
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
    /// <param name="competitionDataService">Сервис для работы с Данными о соревнованиях.</param>
    /// <param name="refereeService">Сервис для работы с Судьями.</param>
    /// <param name="sportEventService">Сервис для работы с Видами программы</param>
    public static MainVM Create(IViewWithResources view, 
        StatusBarService statusBarService,
        ILogger logger, 
        IExceptionsProvider exceptionsProvider,
        AppSettingService appSettingService,
        CompetitionDataService competitionDataService,
        RefereeService refereeService,
        SportEventService sportEventService)
    {
        // TODO: Устанавливаем цвет кистей статус-бара (возможно изменим)
        statusBarService.ReSetBrushes(Brushes.Azure, Brushes.Azure,
            // new SolidColorBrush(Color.FromRgb(0xE6, 0x5C, 0x00)));
            Brushes.LightSalmon);

        // Создаем ViewModel's. Последовательность создания важна!
        var settingVM = new SettingVM(view, statusBarService, logger, exceptionsProvider, appSettingService,
            competitionDataService, refereeService, sportEventService);
        var backstageVM = new BackstageVM(statusBarService, logger, exceptionsProvider, appSettingService, 
            competitionDataService);
        var mainVM = new MainVM(backstageVM, settingVM, statusBarService, logger);

        return mainVM;
    }
    
    /// <summary>
    /// TODO: OnPink - Временно
    /// </summary>
    private async Task OnPink()
    {
        // Пишем в статус-бар
        await StatusBarService.SetProgressAsync(50);
        // await StatusBarService.SetTextAsync("OnPink" + 
        //     " Если нужно добавить всплывающие подсказки, иконки и кликабельность" + 
        //     " Если нужно добавить всплывающие подсказки, иконки и кликабельность" + 
        //     " Если нужно добавить всплывающие подсказки, иконки и кликабельность", 
        //         BaseException.ExcptnType.Info, 0, false);
        await StatusBarService.SetTextAsync("OnPink " + AppPhrases.UnknownError, 
            BaseException.ExcptnType.Info, 0, false);

        await Task.Delay(2000);
        await StatusBarService.SetProgressAsync(100);
        await Task.Delay(3000);
        await StatusBarService.SetProgressAsync(0);
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