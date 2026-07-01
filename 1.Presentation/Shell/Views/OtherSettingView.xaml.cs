using System.Windows.Input;
using AppDomain.Setting.Services;
using Common.WpfModule.Ui.Views;
using Presentation.ViewModels;
using Serilog;

namespace Presentation.Shell.Views;

/// <summary>
/// Представление (окно) с настройками.
/// </summary>
public partial class OtherSettingView : IViewWithResources
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
    public OtherSettingView(ILogger logger, AppSettingService appSetting)
    {
        InitializeComponent();

        DataContext = new OtherSettingVM(this, logger, appSetting);
    }
}