using System.Windows.Input;

namespace Common.WpfModule.Components.Commands;

public class RelayCommand : ICommand
{
    private readonly Func<object, bool>? _canExecute;
    private readonly Action<object>? _onExecute;
 
    /// <summary>
    /// Событие извещающее об изменении состояния команды.
    /// </summary>
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
 
    /// <summary>
    /// Конструктор команды.
    /// </summary>
    /// <param name="execute">Выполняемый метод команды.</param>
    /// <param name="canExecute">Метод разрешающий выполнение команды.</param>
    public RelayCommand(Action<object>? execute, Func<object, bool>? canExecute = null)
    {
        _onExecute = execute;
        _canExecute = canExecute;
    }
 
    /// <summary>
    /// Вызов разрешающего метода команды.
    /// </summary>
    /// <param name="parameter">Параметр команды.</param>
    /// <returns>True - если выполнение команды разрешено.</returns>
    public bool CanExecute(object? parameter) 
        => parameter != null && (_canExecute == null || _canExecute.Invoke(parameter));
 
    /// <summary>
    /// Вызов выполняющего метода команды.
    /// </summary>
    /// <param name="parameter">Параметр команды.</param>
    public void Execute(object? parameter)
    {
        if (parameter != null) _onExecute?.Invoke(parameter);
    }
}