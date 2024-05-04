using System.Diagnostics.CodeAnalysis;

namespace ProblemDomain.Entities._Contracts;

/// <summary>
/// Абстрактный класс сущности.
/// </summary>
public abstract class AbstractEntity : IAbstractEntity, IEntityWithName, IEntityWithDescription
{
    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="name">Наименование.</param>
    /// <param name="description">Описание.</param>
    protected AbstractEntity(string name, string? description = null)
    {
        Name = name;
        Description = description;
    }

    /// <inheritdoc />
    public int Id { get; set; }
    
    /// <inheritdoc />
    public string Name { get; set; }
    
    /// <inheritdoc />
    public string? Description { get; set; }    
}
