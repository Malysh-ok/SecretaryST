namespace Common.WpfModule.Components.ObservableModels;

/// <summary>
/// Предоставляет возможность получить результат выполнения.
/// </summary>
/// <typeparam name="TResult"></typeparam>
public interface IResult<out TResult>
{
    /// <summary>
    /// Получить результат.
    /// </summary>
    /// <returns></returns>
    TResult GetResult();
}