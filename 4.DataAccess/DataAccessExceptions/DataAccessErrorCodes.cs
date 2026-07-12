namespace DataAccess.DataAccessExceptions;

/// <summary>
/// Содержит строковые коды ошибок слоя доступа к данным (4.DataAccess).
/// </summary>
/// <remarks>
/// Коды используются для идентификации типа ошибки и последующей локализации сообщения.
/// Коды должны быть уникальными в пределах слоя.
/// </remarks>
public class DataAccessErrorCodes
{
    public const string DbConnectionError = nameof(DbConnectionError);
    public const string DbCreateError = nameof(DbCreateError);
    public const string DbDoesNotExistError = nameof(DbDoesNotExistError);
    public const string DbRecreated = nameof(DbRecreated);
    public const string EntityAlreadyExistsError = nameof(EntityAlreadyExistsError);
    public const string MissingEntitiesError = nameof(MissingEntitiesError);
    public const string NotSupportedByDbProviderError = nameof(NotSupportedByDbProviderError);
    public const string UnknownError = nameof(UnknownError);
    public const string UpdateDeletedEntityError = nameof(UpdateDeletedEntityError);
}