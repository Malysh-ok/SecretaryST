using Common.WpfModule.Components.Signals.Interfaces;

namespace Common.WpfModule.Components.Signals;

/// <summary>
/// Предназначен для передачи сигнала от ViewModel на View через слабую ссылку.   
/// </summary>
public class Signal<T> : SignalBase, ISignal<T>
{
    private Action<T>? _action;


    /// <inheritdoc />
    public void Subscribe(Action<T> action)
    {
        _action = action;
    }

    /// <inheritdoc />
    public void Raise(T param)
    {
        _action?.Invoke(param);
    }

    /// <inheritdoc />
    public void Clear()
    {
        _action = null;
    }
}