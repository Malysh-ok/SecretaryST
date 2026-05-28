using System.Windows.Media;
using AppDomain.Setting.Services;
using AppDomain.UseCases.Services;
using Common.WpfModule.Ui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using Presentation.ViewModels._Contracts;
using Presentation.ViewModels.Common;
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
    private ILogger _logger = null!;
    
    /// <summary>
    /// Модель представления Backstage.
    /// </summary>
    public BackstageVM BackstageVM { get; } = null!;

    /// <summary>
    /// Модель представления настроек.
    /// </summary>
    public SettingVM SettingVM { get; } = null!;

    
    private StatusBarData _statusBarData = new StatusBarData();
    /// <summary>
    /// Данные для статус-бара.
    /// </summary>
    public StatusBarData StatusBarData
    {
        get => _statusBarData;
        set => SetProperty(ref _statusBarData, value);
    }

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
    private MainVM(BackstageVM backstageVm, SettingVM settingVm, ILogger logger)
    {
        BackstageVM = backstageVm;
        SettingVM = settingVm;
        _logger = logger;
    }

    /// <summary>
    /// Создание экземпляра класса (фабричный метод).
    /// </summary>
    /// <param name="view">Представление, реализующее ресурсы.</param>
    /// <param name="logger">Логгер.</param>
    /// <param name="appSetting">Настройки приложения.</param>
    /// <param name="exceptionsProvider">Поставщик исключения.</param>
    /// <param name="competitionDataService">Сервис для работы с Данными о соревнованиях.</param>
    /// <param name="refereeService">Сервис для работы с Судьями.</param>
    public static MainVM Create(IViewWithResources view, ILogger logger, IExceptionsProvider exceptionsProvider,
        AppSettingService appSetting,
        CompetitionDataService competitionDataService,
        RefereeService refereeService
        )
    {
        // TODO: Устанавливаем цвет кистей статус-бара (возможно изменим)
        StatusBarData.ReSet(Brushes.Azure, Brushes.Azure, Brushes.LightSalmon);

        return new MainVM(
            new BackstageVM(logger),
            new SettingVM(view, logger, exceptionsProvider,
                appSetting,
                competitionDataService,
                refereeService),
            logger
        );
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            BackstageVM.Dispose();
            SettingVM.Dispose();
        }
    }
    
    ~MainVM() => Dispose(false);
}