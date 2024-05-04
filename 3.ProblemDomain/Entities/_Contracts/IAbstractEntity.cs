namespace ProblemDomain.Entities._Contracts;

/// <summary>
/// Интерфейс абстрактной базовой сущности.
/// </summary>
public interface IAbstractEntity
{
    /// <summary>
    /// Идентификатор.
    /// </summary>
    public int Id { get; set; }
}