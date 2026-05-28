namespace ProblemDomain.Entities._Contracts;

/// <summary>
/// Интерфейс сущности, содержащей номер.
/// </summary>
public interface INumberedEntity
{
    /// <summary>
    /// Номер.
    /// </summary>
    public int Number { get; set; }    
}