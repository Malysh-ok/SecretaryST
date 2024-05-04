namespace ProblemDomain.Entities._Contracts;

/// <summary>
/// Интерфейс сущности, содержащей наименование.
/// </summary>
public interface IEntityWithName
{
    /// <summary>
    /// Наименование.
    /// </summary>
    public string Name { get; set; }    
}