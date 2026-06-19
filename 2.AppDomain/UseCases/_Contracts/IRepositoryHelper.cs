using Common.BaseComponents.Components;

namespace AppDomain.UseCases._Contracts;

/// <summary>
/// Интерфейс, реализующий дополнительные свойства/методы для репозитория.
/// </summary>
public interface IRepositoryHelper
{
    /// <summary>
    /// Упрощенная проверка наличия библиотечных сущностей.
    /// </summary>
    public Task<bool> IsExistLibrary();
    
    /// <summary>
    /// Полное пересоздание репозитория (БД).
    /// </summary>
    public Task<Result<bool>> RebuildRepository();
}