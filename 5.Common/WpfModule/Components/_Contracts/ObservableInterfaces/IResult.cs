namespace Common.WpfModule.Components._Contracts.ObservableInterfaces;

/// <summary>
/// Предоставляет возможность получить результат выполнения.
/// </summary>
/// <typeparam name="TResult">Тип результата.</typeparam>
public interface IResult<out TResult>
{
    /// <summary>
    /// Получить результат.
    /// </summary>
    TResult GetResult();
}