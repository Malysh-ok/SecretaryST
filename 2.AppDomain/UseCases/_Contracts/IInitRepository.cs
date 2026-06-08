using Common.BaseComponents.Components;

namespace AppDomain.UseCases._Contracts;

/// <summary>
/// Интерфейс, реализующий свойства/методы, необходимые для инициализации.
/// </summary>
public interface IInitRepository
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