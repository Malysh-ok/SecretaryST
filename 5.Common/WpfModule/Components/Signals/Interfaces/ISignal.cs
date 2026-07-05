using Common.WpfModule.Components._Contracts.ObservableInterfaces;

namespace Common.WpfModule.Components.Signals.Interfaces;

/// <summary>
/// Предназначен для передачи сигнала от ViewModel на View.   
/// </summary>
public interface ISignal : IClearable
{
    /// <summary>
    /// Подписка на сигнал.
    /// </summary>
    /// <param name="action"></param>
    void Subscribe(Action action);
        
    /// <summary>
    /// Посигналить.
    /// </summary>
    void Raise();
}