namespace ProblemDomain.Entities._Contracts;

/// <summary>
/// Интерфейс сущности, содержащей наименование.
/// </summary>
public interface INamedEntity
{
    /// <summary>
    /// Наименование.
    /// </summary>
    public string Name { get; set; }    
}