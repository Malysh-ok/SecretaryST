using CommunityToolkit.Mvvm.ComponentModel;
using Serilog;

namespace Presentation.ViewModels.MainView;

/// <summary>
/// ViewModel для специфичного меню "File" (для Backstage).
/// </summary>
// ReSharper disable once InconsistentNaming
public sealed class BackstageVM : ObservableRecipient, IDisposable
{
    private ILogger _logger;

    /// <summary>
    /// Конструктор.
    /// </summary>
    public BackstageVM(ILogger logger)
    {
        _logger = logger;
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
    }
    
    ~BackstageVM() => Dispose(false);
}