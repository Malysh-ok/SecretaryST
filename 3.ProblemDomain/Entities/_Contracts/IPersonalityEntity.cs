namespace ProblemDomain.Entities._Contracts;

/// <summary>
/// Интерфейс сущности личности (человека).
/// </summary>
public interface IPersonalityEntity : IEntityWithName
{
    /// <summary>
    /// Имя.
    /// </summary>
    public string FirstName { get; set; }
    
    /// <summary>
    /// Фамилия.
    /// </summary>
    public string LastName { get; set; }

    /// <summary>
    /// Отчество.
    /// </summary>
    public string? Patronymic { get; set; }
}