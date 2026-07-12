using Common.BaseComponents.Components.Exceptions;

// ReSharper disable InvalidXmlDocComment

namespace DataAccess.DataAccessExceptions;

/// <summary>
/// Исключения, возникающие в слое доступа к данным (4.DataAccess).
/// </summary>
public class DataAccessException : BaseException
{
    /// <summary>
    /// Код ошибки.
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public string? ErrCode {get; protected init;}

    /// <inheritdoc />
    protected DataAccessException(string? message = null, Exception? innerException = null,
        string? localLangName = null, string? localMessage = null)
        : base(GetRealMessage(message, localLangName, localMessage), innerException)
    {
    }

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="errCode">Код ошибки.</param>
    /// <param name="innerException">Исключение, вызвавшее текущее исключение, или null.</param>
    /// <param name="message">Сообщение об ошибке в текущей локализации, указывающее причину создания исключения.</param>
    private DataAccessException(string errCode, Exception? innerException = null, 
        string? message = null)
        : base(innerException: innerException, message: message)
    {
        ErrCode =  errCode;
    }

    /// <summary>
    /// Создает новый экземпляр исключения <see cref="DataAccessException" /> (фабричный метод).
    /// </summary>
    /// <inheritdoc cref="BaseException.Create"/>
    public new static DataAccessException Create(
        string? message = null, Exception? innerException = null,
        string? localLangName = null, string? localMessage = null)
            => new(message, innerException, localLangName, localMessage);
    
    /// <summary>
    /// Создает новый экземпляр исключения <see cref="DataAccessException" /> по коду ошибки (фабричный метод).
    /// </summary>
    /// <inheritdoc cref="DataAccessException(string, Exception, string)"/>
    public static DataAccessException CreateFromErrorCode(
        string errCode, Exception? innerException = null, string? message = null)
        => new(errCode, innerException, message);
}