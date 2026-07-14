namespace Presentation.ViewModels.Shared.Infrastructure._Contracts;

/// <summary>
/// Интерфейс поставщика исключения.
/// </summary>
public interface IExceptionsProvider
{
    /// <summary>
    /// Исключение.
    /// </summary>
    public Exception? Exception { get; set; }

    /// <summary>
    /// Признак того, что исключение фатально.
    /// </summary>
    public bool IsFatal { get; set; }
}