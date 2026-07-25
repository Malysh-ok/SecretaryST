namespace ProblemDomain.Entities._Contracts;

/// <summary>
/// Интерфейс абстрактной базовой сущности.
/// </summary>
public interface IAbstractEntity
{
    /// <summary>
    /// Идентификатор.
    /// </summary>
    object Id { get; }
    
    /// <summary>
    /// Возвращает true, если сущность еще не сохранена в репозитории (Id равен значению по умолчанию).
    /// </summary>
    public bool IsNew { get; }
}