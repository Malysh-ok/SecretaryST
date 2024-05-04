namespace ProblemDomain.Entities._Contracts;

/// <summary>
/// Интерфейс сущности, содержащей описание.
/// </summary>
public interface IEntityWithDescription
{
    /// <summary>
    /// Описание.
    /// </summary>
    public string? Description { get; set; }    
}