using Common.WpfModule.Components.Signals.Interfaces;

namespace Common.WpfModule.Components.Signals;

/// <inheritdoc cref="ISignal" />
public class Signal : SignalBase, ISignal
{
    private Action? _action;
        
    public void Subscribe(Action action)
    {
        _action = action ?? throw new ArgumentNullException(nameof(action), @"Не задано сигнальное действие");
    }

    /// <inheritdoc />
    public void Raise()
    {
        _action?.Invoke();
    }

    /// <inheritdoc />
    public void Clear()
    {
        _action = null;
    }
}