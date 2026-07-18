using AppDomain.AppExceptions;
using Common.BaseComponents.Components.Exceptions;

namespace AppDomain.AppUseCases._Contracts;

/// <summary>
/// Обеспечивает получение сообщений по кодам ошибок и их применение к исключениям
/// для предметной области приложения (2.AppDomain).
/// </summary>
public interface IAppErrorMsgProvider
{
    /// <summary>
    /// Возвращает сообщение по коду ошибки с подстановкой аргументов.
    /// </summary>
    /// <param name="errCode">Код ошибки.</param>
    /// <param name="args">Аргументы для форматирования сообщения.</param>
    public string? GetMessage(string? errCode, params object[]? args);

    /// <summary>
    /// Создаёт исключение <see cref="AppException"/> с сообщением по коду ошибки.
    /// </summary>
    /// <param name="code">Код ошибки.</param>
    /// <param name="inner">Исключение, вызвавшее текущее исключение, или null.</param>
    /// <param name="args">Аргументы для форматирования сообщения.</param>
    /// <param name="excptnType">Тип исключения.</param>
    public AppException CreateException(string code, Exception? inner = null, 
        ExcptnTypeEnm excptnType = ExcptnTypeEnm.Error, params object[] args);

    /// <summary>
    /// Создаёт <see cref="AppException"/> с сообщением по коду ошибки.
    /// </summary>
    /// <param name="inner">Исключение, вызвавшее текущее исключение, или null.</param>
    public AppException CreateFatalException(Exception? inner = null);
}