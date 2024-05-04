using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Presentation.ViewModels.MainView;

/// <summary>
/// ViewModel для специфичного меню "File" (для Backstage).
/// </summary>
// ReSharper disable once InconsistentNaming
public class BackstageVM : ObservableRecipient, IDisposable
{
    #region [---------- НЕ публичные члены ----------]

    

    #endregion


    /// <summary>
    /// Конструктор.
    /// </summary>
    public BackstageVM()
    {
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
    }
    
    ~BackstageVM() => Dispose(false);
}