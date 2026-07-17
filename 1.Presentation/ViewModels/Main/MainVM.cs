using System.Windows.Input;
using System.Windows.Media;
using AppDomain.AppExceptions;
using AppDomain.AppUseCases._Contracts;
using Common.BaseComponents.Components.Exceptions;
using Common.WpfModule.Ui.Services;
using Common.WpfModule.Ui.Views._Contracts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

// ReSharper disable InconsistentNaming

namespace Presentation.ViewModels.Main;

/// <summary>
/// ViewModel для основного представления.
/// </summary>
public sealed class MainVM : ObservableRecipient, IDisposable
{
    /// <summary>
    /// Провайдер сообщений об ошибках предметной области приложения.
    /// </summary>
    private readonly IAppErrorMsgProvider _appErrorMsgProvider = null!;

    /// <summary>
    /// Фабрика создания представления <see cref="SettingVM"/>.
    /// </summary>
    private readonly Func<IViewWithResources, SettingVM> _settingVmFactory = null!;
    
    /// <summary>
    /// Фабрика создания представления <see cref="BackstageVM"/>.
    /// </summary>
    private readonly Func<BackstageVM> _backstageVmFactory = null!;

    /// <summary>
    /// Модель представления настроек.
    /// </summary>
    public SettingVM SettingVM
    {
        get;
        set => SetProperty(ref field, value);
    } = null!;

    /// <summary>
    /// Модель представления Backstage.
    /// </summary>
    public BackstageVM BackstageVM
    {
        get;
        set => SetProperty(ref field, value);
    } = null!;

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
    public MainVM(
        Func<IViewWithResources, SettingVM> settingVmFactory,
        Func<BackstageVM> backstageVmFactory, 
        IAppErrorMsgProvider appErrorMsgProvider,
        StatusBarService statusBarService)
    {
        _settingVmFactory = settingVmFactory;
        _backstageVmFactory = backstageVmFactory;
        _appErrorMsgProvider = appErrorMsgProvider;
        _statusBarService = statusBarService;

        PinkCommand = new AsyncRelayCommand(OnPink);
        
        // TODO: Устанавливаем цвет кистей статус-бара (возможно изменим)
        _statusBarService.ReSetBrushes(Brushes.Azure, Brushes.Azure,
            // new SolidColorBrush(Color.FromRgb(0xE6, 0x5C, 0x00)));
            Brushes.LightSalmon);
    }

    /// <summary>
    /// Инициализирует главную модель представления, создавая дочерние ViewModel.
    /// </summary>
    /// <remarks>
    /// Метод должен быть вызван после создания экземпляра <see cref="MainVM"/>
    /// и до отображения главного окна.
    /// </remarks>
    /// <param name="view">Главное представление, реализующее <see cref="IViewWithResources"/>,
    /// которое передаётся дочерним ViewModel для локализации.</param>
    public void Initialize(IViewWithResources view)
    {
        // Последовательность создания важна!
        SettingVM = _settingVmFactory(view);
        BackstageVM = _backstageVmFactory();
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
            ExcptnTypeEnm.Info, 0, false);

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