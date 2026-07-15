using System.Windows.Input;
using Common.WpfModule.Ui.Views._Contracts;

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