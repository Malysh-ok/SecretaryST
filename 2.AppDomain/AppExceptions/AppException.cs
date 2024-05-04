using Common.BaseComponents.Components.Exceptions;

namespace AppDomain.AppExceptions;

/// <summary>
/// Класс исключения приложения.
/// </summary>
public class AppException : BaseException
{
    /// <inheritdoc />
    public AppException(string? message = null, Exception? innerException = null,
        string? localLangName = null, string? localMessage = null)
        : base(GetRealMessage(message, localLangName, localMessage), innerException)
    {
    }

    /// <summary>
    /// Создает новый экземпляр исключения <see cref="AppException" /> (фабричный метод).
    /// </summary>
    /// <inheritdoc cref="BaseException.CreateException"/>
    public new static AppException CreateException(
        string? message = null, Exception? innerException = null,
        string? localLangName = null, string? localMessage = null)
        => new(message, innerException, localLangName, localMessage);
}