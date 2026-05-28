namespace ProblemDomain.Entities._Contracts;

/// <summary>
/// Интерфейс сущности, содержащей описание.
/// </summary>
public interface IDescriptionedEntity
{
    /// <summary>
    /// Описание.
    /// </summary>
    public string? Description { get; set; }    
}