namespace AppDomain.UseCases._Contracts;

public interface IInitRepository
{
    /// <summary>
    /// Упрощенная проверка наличия библиотечных сущностей.
    /// </summary>
    public Task<bool> IsExistLibrary();
    /// <summary>
    /// Полное пересоздание репозитория (БД).
    /// </summary>
    public Task RebuildRepository();
}