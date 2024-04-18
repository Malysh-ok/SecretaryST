using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using AppDomain.Setting.Services;
using Common.BaseExtensions;
using Common.BaseExtensions.ValueTypes;
using Common.WpfModule.Ui.Views;
using CommunityToolkit.Mvvm.Input;
using Presentation.ViewModels.MainView;

namespace Presentation.Shell.Views;

/// <summary>
/// Основное представление (окно).
/// </summary>
public partial class MainView : IViewWithResources
{
    // Настройки приложения
    private static AppSettingService _appSetting = null!;   // на момент обращения поле уже будет != null
    
    /// <summary>
    /// Представление (окно) дополнительных настроек.
    /// </summary>
    private static OtherSettingView? _otherSettingView;

    
    /// <summary>
    /// Создание команды показа представления дополнительных настроек.
    /// </summary>
    // TODO: Попытаться сделать НЕ статическую команду
    public static ICommand ShowOtherSettingViewCommand { get; } = new RelayCommand(() =>
    {
        if (_otherSettingView is { IsVisible: true })
        {
            // Если окно доп. настоек отображено
            _otherSettingView.Activate();
            return;
        }

        // Удаляем старое и создаем новое окно доп. настроек
        _otherSettingView?.Close();
        _otherSettingView = new OtherSettingView(_appSetting)
        {
            Owner = Application.Current.MainWindow
        };
        _otherSettingView.Show();
    });

    /// <summary>
    /// Конструктор.
    /// </summary>
    public MainView(AppSettingService appSetting)
    {
        _appSetting = appSetting;
        
        InitializeComponent();
            
        // Привязываем данные главной модели представления
        DataContext = new MainVM(
            new BackstageVM(),
            new SettingVM(this, _appSetting)
        );
        
        // TODO: Пример CommandBinding
        // var dialogCommandBinding =
        //      new CommandBinding(ShowOtherSettingViewCommand, ExecuteShowDialogCommand, CanExecuteShowDialogCommand);
        // CommandBindings.Add(dialogCommandBinding);
    }
        
    /// <summary>
    /// Выход из приложения.
    /// </summary>
    private void ExitApp(object sender, ExecutedRoutedEventArgs executedRoutedEventArgs)
    {
        Application.Current.Shutdown();
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
    }
    
    /// <summary>
    /// Загрузка данных представления (окна) из настроек.
    /// </summary>
    private void LoadViewData()
    {
        if (_appSetting.GetConfigItem("MainViewLeft").NullToEmpty().TryParseDouble(out var result))
            Left = result;
        if (_appSetting.GetConfigItem("MainViewTop").NullToEmpty().TryParseDouble(out result))
            Top = result;
        if (_appSetting.GetConfigItem("MainViewWidth").NullToEmpty().TryParseDouble(out result))
            Width = result;
        if (_appSetting.GetConfigItem("MainViewHeight").NullToEmpty().TryParseDouble(out result))
            Height = result;
        WindowState = _appSetting.GetConfigItem("MainViewState").NullToEmpty().ToEnum<WindowState>();
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
    protected override async void OnClosed(EventArgs e)
    {
        SaveViewData();
        
        // _appSetting.SaveConfig().ConfigureAwait(false);
        await _appSetting.SaveConfig();
        
        base.OnClosed(e);
    }
}