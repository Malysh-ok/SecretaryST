using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using AppDomain.AppUseCases.Services;
using Common.BaseExtensions;
using Common.BaseExtensions.ValueTypes;
using Common.WpfModule.Ui.Services._Contracts;
using Common.WpfModule.Ui.Views._Contracts;
using CommunityToolkit.Mvvm.Messaging;
using Presentation.ViewModels;
using Presentation.ViewModels.AppSetting;
using Presentation.ViewModels.Main;
using Presentation.ViewModels.Shared.Messages;

namespace Presentation.Shell.Infrastructure.Views;

/// <summary>
/// Основное представление (окно).
/// </summary>
public partial class MainView : IViewWithResources, IRecipient<OpenAppSettingMessage>
{
    private readonly IViewService _viewService;
    private readonly AppSettingService _appSettingService;

    /// <summary>
    /// Конструктор.
    /// </summary>
    public MainView(IViewService viewService, AppSettingService appSettingService)
    {
        _viewService = viewService;
        _viewService.Initialize(this);
        _appSettingService = appSettingService;

        InitializeComponent();

        // Подписываемся на сообщение
        WeakReferenceMessenger.Default.Register<MainView, OpenAppSettingMessage>(this, (r, m) =>
            r.Receive(m));
    }
    
    /// <summary>
    /// Получаем сообщение с экземпляром <see cref="OpenAppSettingMessage"/>.
    /// </summary>
    public void Receive(OpenAppSettingMessage message)
    {
        // Открываем Представление настроек
        // Если в сообщении передан конкретный DataContext, используем его
        if (message.DataContext != null)
        {
            _viewService.ShowView<AppSettingView>(message.DataContext);
        }
        else
        {
            // Иначе создаём ViewModel через DI (с передачей главного окна как IViewWithResources)
            _viewService.ShowView<AppSettingView, AppSettingVM>();
        }
    }
    
    /// <summary>
    /// Выход из приложения.
    /// </summary>
    private void ExitApp(object sender, ExecutedRoutedEventArgs executedRoutedEventArgs)
    {
        Application.Current.Shutdown();
    }
        
    /// <summary>
    /// Загрузка данных представления (окна) из настроек.
    /// </summary>
    private void LoadViewData()
    {
        if (_appSettingService.GetConfigItem("MainViewLeft").NullToEmpty().TryParseDouble(out var doubleResult))
            Left = doubleResult;
        if (_appSettingService.GetConfigItem("MainViewTop").NullToEmpty().TryParseDouble(out doubleResult))
            Top = doubleResult;
        if (_appSettingService.GetConfigItem("MainViewWidth").NullToEmpty().TryParseDouble(out doubleResult))
            Width = doubleResult;
        if (_appSettingService.GetConfigItem("MainViewHeight").NullToEmpty().TryParseDouble(out doubleResult))
            Height = doubleResult;
        WindowState = _appSettingService.GetConfigItem("MainViewState").NullToEmpty().ToEnum<WindowState>();

        // TODO: Не срабатывает восстановление значений контролов. Придумать другой способ.
        if (_appSettingService.GetConfigItem(nameof(CmboxRestrictedDisciplineGroup)).NullToEmpty().TryParseInt(out var intResult))
            CmboxRestrictedDisciplineGroup.SelectedIndex = intResult;
    }

    /// <summary>
    /// Сохранение данных представления (окна) в настройках.
    /// </summary>
    private void SaveViewData()
    {
        _appSettingService.SetConfigItem("MainViewLeft", Left.ToString(CultureInfo.InvariantCulture));
        _appSettingService.SetConfigItem("MainViewTop", Top.ToString(CultureInfo.InvariantCulture));
        _appSettingService.SetConfigItem("MainViewWidth", Width.ToString(CultureInfo.InvariantCulture));
        _appSettingService.SetConfigItem("MainViewHeight", Height.ToString(CultureInfo.InvariantCulture));
        _appSettingService.SetConfigItem("MainViewState", WindowState.ToString());

        _appSettingService.SetConfigItem(nameof(CmboxRestrictedDisciplineGroup), CmboxRestrictedDisciplineGroup.SelectedIndex.ToString());
    }

    /// <summary>
    /// Обработчик события инициализации представления (окна).
    /// </summary>
    protected override void OnInitialized (EventArgs e)
    {
        LoadViewData();
        
        base.OnInitialized(e);
    }
    
    /// <summary>
    /// Обработчик события выхода из приложения.
    /// </summary>
    protected override void OnClosed(EventArgs e)
    {
        SaveViewData();
        
        ((MainVM)DataContext).Dispose();
        
        base.OnClosed(e);
    }
}