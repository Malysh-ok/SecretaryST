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
using Presentation.ViewModels.AppSetting;
using Presentation.ViewModels.Main;
using Presentation.ViewModels.Shared.Messages;

namespace Presentation.Shell.Views;

/// <summary>
/// Основное представление (окно).
/// </summary>
public partial class MainView : IViewWithResources, IRecipient<OpenAppSettingMessage>
{
    private readonly IViewService _viewService;
    private readonly AppSettingsService _appSettingsService;

    /// <summary>
    /// Конструктор.
    /// </summary>
    public MainView(IViewService viewService, AppSettingsService appSettingsService)
    {
        _viewService = viewService;
        _viewService.Initialize(this);
        _appSettingsService = appSettingsService;

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
        if (_appSettingsService.GetConfigItem("MainViewLeft").NullToEmpty().TryParseDouble(out var doubleResult))
            Left = doubleResult;
        if (_appSettingsService.GetConfigItem("MainViewTop").NullToEmpty().TryParseDouble(out doubleResult))
            Top = doubleResult;
        if (_appSettingsService.GetConfigItem("MainViewWidth").NullToEmpty().TryParseDouble(out doubleResult))
            Width = doubleResult;
        if (_appSettingsService.GetConfigItem("MainViewHeight").NullToEmpty().TryParseDouble(out doubleResult))
            Height = doubleResult;
        WindowState = _appSettingsService.GetConfigItem("MainViewState").NullToEmpty().ToEnum<WindowState>();

        // TODO: Не срабатывает восстановление значений контролов. Придумать другой способ.
        if (_appSettingsService.GetConfigItem(nameof(CmboxRestrictedDisciplineGroup)).NullToEmpty().TryParseInt(out var intResult))
            CmboxRestrictedDisciplineGroup.SelectedIndex = intResult;
    }

    /// <summary>
    /// Сохранение данных представления (окна) в настройках.
    /// </summary>
    private void SaveViewData()
    {
        _appSettingsService.SetConfigItem("MainViewLeft", Left.ToString(CultureInfo.InvariantCulture));
        _appSettingsService.SetConfigItem("MainViewTop", Top.ToString(CultureInfo.InvariantCulture));
        _appSettingsService.SetConfigItem("MainViewWidth", Width.ToString(CultureInfo.InvariantCulture));
        _appSettingsService.SetConfigItem("MainViewHeight", Height.ToString(CultureInfo.InvariantCulture));
        _appSettingsService.SetConfigItem("MainViewState", WindowState.ToString());

        _appSettingsService.SetConfigItem(nameof(CmboxRestrictedDisciplineGroup), CmboxRestrictedDisciplineGroup.SelectedIndex.ToString());
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