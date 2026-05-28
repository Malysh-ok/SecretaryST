using System.Diagnostics.CodeAnalysis;

namespace ProblemDomain.Entities._Contracts;

/// <summary>
/// Абстрактный класс сущности.
/// </summary>
public abstract class AbstractEntity : IAbstractEntity, INamedEntity, IDescriptionedEntity
{
    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="name">Наименование.</param>
    /// <param name="description">Описание.</param>
    protected AbstractEntity(string name, string? description = null)
    {
        // ReSharper disable once VirtualMemberCallInConstructor
        Name = name;
        Description = description;
    }

    /// <inheritdoc />
    public int Id { get; set; }
    
    /// <inheritdoc />
    public virtual string Name { get; set; }
    
    /// <inheritdoc />
    public string? Description { get; set; }    
}
