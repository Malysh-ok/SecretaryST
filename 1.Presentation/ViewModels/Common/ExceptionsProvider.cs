using Presentation.ViewModels._Contracts;

namespace Presentation.ViewModels.Common;

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