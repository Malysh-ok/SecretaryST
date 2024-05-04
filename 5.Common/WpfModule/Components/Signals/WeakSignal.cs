using Common.WpfModule.Components.Signals.Interfaces;

namespace Common.WpfModule.Components.Signals;

/// <summary>
/// Предназначен для передачи сигнала от ViewModel на View через слабую ссылку.   
/// </summary>
public class WeakSignal : SignalBase, ISignal
{
    private WeakReference<Action>? _action;

    /// <inheritdoc />
    public void Subscribe(Action action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action), @"Не задано сигнальное действие");
            
        _action = new WeakReference<Action>(action);
    }

    /// <inheritdoc />
    public void Raise()
    {
        if (_action == null)
            return;
            
        if (_action.TryGetTarget(out var action))
            action.Invoke();
    }

    /// <inheritdoc />
    public void Clear()
    {
        _action = null;
    }
}