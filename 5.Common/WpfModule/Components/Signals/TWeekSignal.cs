using Common.WpfModule.Components.Signals.Interfaces;

namespace Common.WpfModule.Components.Signals
{
    /// <summary>
    /// Предназначен для передачи сигнала от ViewModel на View через слабую ссылку.   
    /// </summary>
    public class WeekSignal<T> : SignalBase, ISignal<T>
    {
        private WeakReference<Action<T>>? _action;

        /// <inheritdoc />
        public void Subscribe(Action<T> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action), @"Не задано сигнальное действие");
            
            _action = new WeakReference<Action<T>>(action);
        }

        /// <inheritdoc />
        public void Raise(T param)
        {
            if (_action == null)
                return;
            
            if (_action.TryGetTarget(out var action))
                action.Invoke(param);
        }
        
        /// <inheritdoc />
        public void Clear()
        {
            _action = null;
        }
    }
}