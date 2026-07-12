using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using AppDomain.AppUseCases._Contracts;
using AppDomain.AppUseCases.Services;
using Common.BaseExtensions;
using Common.BaseExtensions.ValueTypes;
using Common.WpfModule.Ui.Services;
using Common.WpfModule.Ui.Views;
using CommunityToolkit.Mvvm.Messaging;
using Presentation.ViewModels._Contracts;
using Presentation.ViewModels.Common.Messages;
using Presentation.ViewModels.MainView;
using ProblemDomain.UseCases.Services;
using Serilog;

namespace Presentation.Shell.Views;

/// <summary>
/// Основное представление (окно).
/// </summary>
public partial class MainView : IViewWithResources, IRecipient<OpenAppSettingMessage>
{
    /// <summary>
    /// Последнее активное дочернее окно.
    /// </summary>
    private Window? _lastActiveChild;
    
    /// <summary>
    /// Флаг, указывающий, что при следующей активации главного окна нужно
    /// принудительно активировать дочернее окно.
    /// Устанавливается при открытии дочернего окна и сбрасывается после активации.
    /// Это предотвращает перехват фокуса при каждом клике на главное окно.
    /// </summary>
    private bool _shouldActivateChild;

    private readonly ILogger _logger;
    private readonly StatusBarService _statusBarService;
    private readonly AppSettingService _appSetting;
    private readonly IAppErrorMsgProvider _appErrorMsgProvider;

    /// <summary>
    /// Представление (окно) настроек приложения.
    /// </summary>
    private static AppSettingView? _appSettingView;


    /// <summary>
    /// Конструктор.
    /// </summary>
    public MainView(StatusBarService statusBarService,
        ILogger logger, 
        IExceptionsProvider exceptionsProvider,
        IAppErrorMsgProvider appErrorMsgProvider,
        AppSettingService appSetting,
        CompetitionDataService competitionDataService,
        RefereeService refereeService,
        SportEventService  sportEventService)

    {
        _logger = logger;
        _statusBarService = statusBarService;
        _appSetting = appSetting;
        _appErrorMsgProvider = appErrorMsgProvider;

        InitializeComponent();
        
        // Получаем главную модель представления
        var mainVm = MainVM.Create(this, logger, exceptionsProvider, _appErrorMsgProvider,
            appSetting, statusBarService, competitionDataService, refereeService, sportEventService);
        
        // Привязываем данные главной модели представления
        DataContext = mainVm;

        // Подписываемся на сообщение
        WeakReferenceMessenger.Default.Register<MainView, OpenAppSettingMessage>(this, (r, m) =>
            r.Receive(m));
        
        // REMARK: Костыль - при закрытии Backstage фокус возвращается в главное окно,
        // и нам нужно перехватить это событие, чтобы активировать дочернее окно.
        this.Activated += OnMainWindowActivated;
    }
    
    public void Receive(OpenAppSettingMessage message)
    {
        // Если окно уже открыто — активируем его
        if (_appSettingView is { IsVisible: true })
        {
            _appSettingView.Activate();
            return;
        }

        // Закрываем старое окно (если есть)
        _appSettingView?.Close();

        // Создаём новое окно, передаём ViewModel
        _appSettingView = new AppSettingView(_logger, _appErrorMsgProvider, _appSetting, _statusBarService);
        
        // Вызываем универсальный метод открытия окна
        OpenChildWindow(_appSettingView,
            message.DataContext ?? _appSettingView.DataContext);
    }
    
    /// <summary>
    /// Универсальный метод открытия любого дочернего окна
    /// </summary>
    public void OpenChildWindow(Window childWindow, object? dataContext = null)
    {
        childWindow.Owner = this;
        childWindow.DataContext = dataContext;

        // Подписываемся на активацию дочернего окна
        childWindow.Activated += OnChildWindowActivated;
        // При закрытии отписываемся
        childWindow.Closed += OnChildWindowClosed;

        childWindow.Show();
        
        // Устанавливаем флаг — нужно активировать дочернее окно при следующей активации главного
        _shouldActivateChild = true;
    }
    
    /// <summary>
    /// Перехватывает активацию главного окна (после закрытия Backstage или переключения).
    /// Если флаг _shouldActivateChild установлен, активирует последнее дочернее окно и сбрасывает флаг.
    /// </summary>
    private void OnMainWindowActivated(object? sender, EventArgs e)
    {
        // Если есть последнее активное дочернее окно, оно видимо, но не активно — активируем его
        if (_shouldActivateChild && _lastActiveChild != null && _lastActiveChild.IsVisible && !_lastActiveChild.IsActive)
        {
            _shouldActivateChild = false; 
            
            // Активируем дочернее окно
            _lastActiveChild.Activate();
            _lastActiveChild.Focus();
              
        }
    }

    private void OnChildWindowActivated(object? sender, EventArgs e)
    {
        // Запоминаем последнее активное дочернее окно
        _lastActiveChild = sender as Window;
    }

    private void OnChildWindowClosed(object? sender, EventArgs e)
    {
        if (sender is Window closingWindow)
        {
            // Если закрывается окно, которое было последним активным — обнуляем ссылку
            if (_lastActiveChild == closingWindow)
                _lastActiveChild = null;

            // Отписываемся от событий
            closingWindow.Activated -= OnChildWindowActivated;
            closingWindow.Closed -= OnChildWindowClosed;
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
        if (_appSetting.GetConfigItem("MainViewLeft").NullToEmpty().TryParseDouble(out var doubleResult))
            Left = doubleResult;
        if (_appSetting.GetConfigItem("MainViewTop").NullToEmpty().TryParseDouble(out doubleResult))
            Top = doubleResult;
        if (_appSetting.GetConfigItem("MainViewWidth").NullToEmpty().TryParseDouble(out doubleResult))
            Width = doubleResult;
        if (_appSetting.GetConfigItem("MainViewHeight").NullToEmpty().TryParseDouble(out doubleResult))
            Height = doubleResult;
        WindowState = _appSetting.GetConfigItem("MainViewState").NullToEmpty().ToEnum<WindowState>();

        // TODO: Не срабатывает восстановление значений контролов. Придумать другой способ.
        if (_appSetting.GetConfigItem(nameof(CmboxRestrictedDisciplineGroup)).NullToEmpty().TryParseInt(out var intResult))
            CmboxRestrictedDisciplineGroup.SelectedIndex = intResult;
    }

    /// <summary>
    /// Сохранение данных представления (окна) в настройках.
    /// </summary>
    private void SaveViewData()
    {
        _appSetting.SetConfigItem("MainViewLeft", Left.ToString(CultureInfo.InvariantCulture));
        _appSetting.SetConfigItem("MainViewTop", Top.ToString(CultureInfo.InvariantCulture));
        _appSetting.SetConfigItem("MainViewWidth", Width.ToString(CultureInfo.InvariantCulture));
        _appSetting.SetConfigItem("MainViewHeight", Height.ToString(CultureInfo.InvariantCulture));
        _appSetting.SetConfigItem("MainViewState", WindowState.ToString());

        _appSetting.SetConfigItem(nameof(CmboxRestrictedDisciplineGroup), CmboxRestrictedDisciplineGroup.SelectedIndex.ToString());
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