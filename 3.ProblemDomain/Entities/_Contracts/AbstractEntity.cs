using System.Collections.Generic;

namespace ProblemDomain.Entities._Contracts;

/// <summary>
/// Абстрактный класс сущности.
/// </summary>
public abstract class AbstractEntity<TId> : IAbstractEntity, INamedEntity, IDescriptionedEntity
    where TId : struct
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

    /// <inheritdoc cref="IAbstractEntity.Id"/>
    public TId Id { get; init; }
    
    /// <summary>
    /// Явная реализация IAbstractEntity.
    /// </summary>
    object IAbstractEntity.Id => Id;
    
    /// <inheritdoc />
    public virtual string Name { get; set; }
    
    /// <inheritdoc />
    public string? Description { get; set; }
    
    /// <inheritdoc />
    public bool IsNew => EqualityComparer<TId>.Default.Equals(Id, default);
}
