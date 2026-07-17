using Common.BaseComponents.Components.Exceptions;
// ReSharper disable InvalidXmlDocComment

namespace AppDomain.AppExceptions;

/// <summary>
/// Исключение для предметной области приложения (2.AppDomain).
/// </summary>
public class AppException : BaseException
{
    /// <summary>
    /// Код ошибки.
    /// </summary>
    public string? ErrCode {get; private set;}

    /// <inheritdoc />
    private AppException(string? message = null, Exception? innerException = null,
        string? localLangName = null, string? localMessage = null, ExcptnTypeEnm excptnType = ExcptnTypeEnm.Error)
        : base(message, innerException, localLangName, localMessage, excptnType)
    {
    }

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="errCode">Код ошибки.</param>
    /// <param name="innerException">Исключение, вызвавшее текущее исключение, или null.</param>
    /// <param name="message">Сообщение об ошибке в текущей локализации, указывающее причину создания исключения.</param>
    /// <param name="excptnType">Тип исключения.</param>
    private AppException(string errCode, Exception? innerException = null, 
        string? message = null, ExcptnTypeEnm excptnType = ExcptnTypeEnm.Error)
        : base(innerException: innerException, message: message,  excptnType: excptnType)
    {
        ErrCode =  errCode;
    }

    /// <summary>
    /// Создает новый экземпляр исключения <see cref="AppException" /> (фабричный метод).
    /// </summary>
    /// <inheritdoc cref="BaseException.Create"/>
    public new static AppException Create(
        string? message = null, Exception? innerException = null,
        string? localLangName = null, string? localMessage = null, 
        ExcptnTypeEnm excptnType = ExcptnTypeEnm.Error)
            => new(message, innerException, localLangName, localMessage, excptnType);
    
    /// <summary>
    /// Создает новый экземпляр исключения <see cref="AppException" /> по коду ошибки (фабричный метод).
    /// </summary>
    /// <inheritdoc cref="AppException(string, Exception, string, ExcptnTypeEnm)"/>
    public static AppException CreateFromErrorCode(string errCode, Exception? innerException = null, 
        string? message = null, ExcptnTypeEnm excptnType = ExcptnTypeEnm.Error)
            => new(errCode, innerException, message, excptnType);
}