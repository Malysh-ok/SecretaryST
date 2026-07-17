using Common.BaseComponents.Components.Exceptions;
using ProblemDomain.ProblemExceptions;

namespace ProblemDomain.UseCases._Contracts;

/// <summary>
/// Обеспечивает получение сообщений по кодам ошибок и их применение к исключениям
/// для предметной области (3.ProblemDomain).
/// </summary>
public interface IProblemErrorMsgProvider
{
    /// <summary>
    /// Возвращает сообщение по коду ошибки с подстановкой аргументов.
    /// </summary>
    /// <param name="errCode">Код ошибки.</param>
    /// <param name="args">Аргументы для форматирования сообщения.</param>
    public string? GetMessage(string? errCode, params object[]? args);

    /// <summary>
    /// Создаёт исключение <see cref="ProblemException"/> с сообщением по коду ошибки.
    /// </summary>
    /// <param name="code">Код ошибки.</param>
    /// <param name="inner">Исключение, вызвавшее текущее исключение, или null.</param>
    /// <param name="args">Аргументы для форматирования сообщения.</param>
    /// <param name="excptnType">Тип исключения.</param>
    public ProblemException CreateException(string code, Exception? inner = null, 
        ExcptnTypeEnm excptnType = ExcptnTypeEnm.Error, params object[] args);
}