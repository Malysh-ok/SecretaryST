using System.Windows.Input;
using AppDomain.Setting.Services;
using Common.WpfModule.Ui.Views;
using Presentation.ViewModels;
using Serilog;

namespace Presentation.Shell.Views;

/// <summary>
/// Представление (окно) с настройками приложения.
/// </summary>
public partial class AppSettingView : IViewWithResources
{
    /// <summary>
    /// Закрыть представление (окно).
    /// </summary>
    private void CloseView(object sender, ExecutedRoutedEventArgs executedRoutedEventArgs)
    {
        Close();
    }

    /// <summary>
    /// Конструктор.
    /// </summary>
    public AppSettingView(ILogger logger, AppSettingService appSetting)
    {
        InitializeComponent();

        DataContext = new AppSettingVM(this, logger, appSetting);
    }
}