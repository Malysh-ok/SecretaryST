using Common.Phrases;

namespace DataAccess.Repositories.Exceptions;

/// <summary>
/// Фатальное исключение, возникающие при работе с БД.
/// </summary>

public class DbFatalException : DbException
{
    /// <inheritdoc />
    public DbFatalException(string? message = null, Exception? innerException = null,
        string? localLangName = null, string? localMessage = null)
        : base(ConvertMessage(typeof(DbPhrases), message ?? DbPhrases.FatalDbError, localLangName),
            innerException,
            localLangName,
            localMessage)
    {
    }


    /// <summary>
    /// Создает новый экземпляр исключения <see cref="DbFatalException" /> (фабричный метод).
    /// </summary>
    /// <inheritdoc cref="DbException.CreateException"/>
    public new static DbFatalException CreateException(
        string? message = null, Exception? innerException = null,
        string? localLangName = null, string? localMessage = null)
        => new(message, innerException, localLangName, localMessage);
}
