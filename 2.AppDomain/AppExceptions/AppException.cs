namespace AppDomain.AppExceptions;

/// <summary>
/// Исключения приложения.
/// </summary>
public class AppException : Exception
{
    /// <summary>
    /// Конструктор.
    /// </summary>
    public AppException() : base()
    {
    }
        
    /// <summary>
    /// Конструктор.
    /// </summary>
    public AppException(string message) : base(message)
    {
    }
        
    /// <summary>
    /// Конструктор.
    /// </summary>
    public AppException(string message, Exception innerException) : base(message, innerException)
    {
    }
}