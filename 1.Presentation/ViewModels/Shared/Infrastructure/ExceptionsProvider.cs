using Presentation.ViewModels.Shared.Infrastructure._Contracts;

namespace Presentation.ViewModels.Shared.Infrastructure;

/// <summary>
/// Поставщик исключения.
/// </summary>
public class ExceptionsProvider : IExceptionsProvider
{
    
    /// <inheritdoc />
    public Exception? Exception { get; set; }
    
    /// <inheritdoc />
    public bool IsFatal { get; set; } = false;
}