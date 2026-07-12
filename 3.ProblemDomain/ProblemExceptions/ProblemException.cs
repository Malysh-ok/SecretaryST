using Common.BaseComponents.Components.Exceptions;

namespace ProblemDomain.ProblemExceptions;

/// <summary>
/// Исключение для предметной области (3.ProblemDomain).
/// </summary>
public class ProblemException : BaseException
{
    /// <summary>
    /// Код ошибки.
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public string? ErrCode {get; private set;}
        
    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="errCode">Код ошибки.</param>
    /// <param name="innerException">Исключение, вызвавшее текущее исключение, или null.</param>
    /// <param name="message">Сообщение об ошибке в текущей локализации, указывающее причину создания исключения.</param>
    private ProblemException(string? errCode = null, Exception? innerException = null, 
        string? message = null)
        : base(innerException: innerException, message: message)
    {
        ErrCode = errCode;
    }
    
    /// <summary>
    /// Создает новый экземпляр исключения <see cref="ProblemException" /> по коду ошибки (фабричный метод).
    /// </summary>
    /// <inheritdoc cref="ProblemException(string, Exception, string)"/>
    public static ProblemException CreateFromErrorCode(
        string errCode, Exception? innerException = null, string? message = null)
        => new(errCode, innerException, message);
}