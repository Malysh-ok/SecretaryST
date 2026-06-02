using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using System.Windows.Media;
using AppDomain.Setting.Services;
using AppDomain.UseCases.Services;
using Common.WpfModule.Ui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Presentation.ViewModels._Contracts;
using Presentation.ViewModels.Common;
using Serilog;

namespace Presentation.ViewModels.MainView;

/// <summary>
/// ViewModel для основного представления.
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed class MainVM : ObservableRecipient, IStatusBarDataProvider, IDisposable
{
    /// <summary>
    /// Логгер.
    /// </summary>
    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
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
    /// Данные для статус-бара.
    /// </summary>
    public StatusBarData StatusBarData { get; } = null!;

    // TODO: Временная команда (кнопка панели быстрого доступа)
    public ICommand PinkCommand { get; } = null!;

    /// <summary>
    /// Конструктор, запрещающий создания экземпляра без параметров.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    private MainVM()
    {
    }
    
    /// <summary>
    /// Конструктор.
    /// </summary>
    private MainVM(BackstageVM backstageVm, SettingVM settingVm, StatusBarData statusBarData, ILogger logger)
    {
        BackstageVM = backstageVm;
        SettingVM = settingVm;
        StatusBarData = statusBarData;
        _logger = logger;
        
        //TODO: Заменить название PinkCommand
        PinkCommand = new AsyncRelayCommand(OnPink);
    }

    /// <summary>
    /// Создание экземпляра класса (фабричный метод).
    /// </summary>
    /// <param name="view">Представление, реализующее ресурсы.</param>
    /// <param name="statusBarData">Данные для статус-бара.</param>
    /// <param name="logger">Логгер.</param>
    /// <param name="appSetting">Настройки приложения.</param>
    /// <param name="exceptionsProvider">Поставщик исключения.</param>
    /// <param name="competitionDataService">Сервис для работы с Данными о соревнованиях.</param>
    /// <param name="refereeService">Сервис для работы с Судьями.</param>
    public static MainVM Create(IViewWithResources view, 
        StatusBarData statusBarData,
        ILogger logger, 
        IExceptionsProvider exceptionsProvider,
        AppSettingService appSetting,
        CompetitionDataService competitionDataService,
        RefereeService refereeService
        )
    {
        // TODO: Устанавливаем цвет кистей статус-бара (возможно изменим)
        statusBarData.ReSetBrushes(Brushes.Azure, Brushes.Azure,
            // new SolidColorBrush(Color.FromRgb(0xE6, 0x5C, 0x00)));
            Brushes.LightSalmon);

        var mainVM = new MainVM(
            new BackstageVM(logger),
            new SettingVM(view, statusBarData, logger, exceptionsProvider,
                appSetting,
                competitionDataService,
                refereeService),
            statusBarData,
            logger
        );

        return mainVM;
    }
    
    /// <summary>
    /// TODO: OnPink - Временно
    /// </summary>
    private async Task OnPink()
    {
        // Пишем в статус-бар
        await StatusBarData.SetProgressAsync(50);
        await StatusBarData.SetTextAsync("OnPink" + 
            " Если нужно добавить всплывающие подсказки, иконки и кликабельность" + 
            " Если нужно добавить всплывающие подсказки, иконки и кликабельность" + 
            " Если нужно добавить всплывающие подсказки, иконки и кликабельность", 
                StatusBarData.StatusBarTextType.Info, 0, false);
        await Task.Delay(2000);
        await StatusBarData.SetProgressAsync(100);
        await Task.Delay(3000);
        await StatusBarData.SetProgressAsync(0);
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