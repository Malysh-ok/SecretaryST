using Common.WpfModule.Components.ObservableModels;

namespace Common.WpfModule.Components.Signals.Interfaces;

/// <summary>
/// Предназначен для передачи сигнала с данными от ViewModel на View.   
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ISignal<T>: IClearable
{
    /// <summary>
    /// Подписка на сигнал.
    /// </summary>
    /// <param name="action"></param>
    void Subscribe(Action<T> action);
        
    /// <summary>
    /// Посигналить.
    /// </summary>
    /// <param name="param"></param>
    void Raise(T param);
}