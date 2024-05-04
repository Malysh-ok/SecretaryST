using System.Diagnostics.CodeAnalysis;
using Common.BaseComponents.Components.Exceptions;

namespace DataAccess.Repositories.Exceptions;

/// <summary>
/// Исключения, возникающие при работе с БД.
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class DbException : BaseException
{
    /// <inheritdoc />
    // ReSharper disable once MemberCanBeProtected.Global
    public DbException(string? message = null, Exception? innerException = null,
        string? localLangName = null, string? localMessage = null)
        : base(GetRealMessage(message, localLangName, localMessage), innerException)
    {
    }

    /// <summary>
    /// Создает новый экземпляр исключения <see cref="DbException" /> (фабричный метод).
    /// </summary>
    /// <inheritdoc cref="BaseException.CreateException"/>
    public new static DbException CreateException(
            string? message = null, Exception? innerException = null,
            string? localLangName = null, string? localMessage = null)
        => new(message, innerException, localLangName, localMessage);
}