using System.Windows.Input;
using AppDomain.AppUseCases._Contracts;
using AppDomain.AppUseCases.Services;
using Common.WpfModule.Ui.Services;
using Common.WpfModule.Ui.Views;
using Common.WpfModule.Ui.Views._Contracts;
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
    public AppSettingView()
    {
        InitializeComponent();
    }
}