using System.Collections.Generic;
using ProblemDomain.Entities._Contracts;
using ProblemDomain.Entities.DistanceEntities;
using ProblemDomain.Entities.LibraryEntities.Enums;
// ReSharper disable InvalidXmlDocComment
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace ProblemDomain.Entities.LibraryEntities;

/// <summary>
/// Дисциплина.
/// </summary>
public sealed class Discipline : AbstractEntity<DisciplineEnm>, IEntityCopyable
{
    /// <summary>
    /// Конструктор для EF.
    /// </summary>
    /// <param name="id">Идентификатор.</param>
    /// <inheritdoc />
    private Discipline(DisciplineEnm id, string name, string? description = null) 
        : base(name, description)
    {
        Id = id;
    }
        
    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <inheritdoc />
    /// <param name="disciplineGroup">Группа дисциплин.</param>
    /// <param name="disciplineSubGroup">Подгруппа дисциплин.</param>
    public Discipline(
        DisciplineEnm id, 
        string name, 
        DisciplineGroup disciplineGroup, 
        DisciplineSubGroup disciplineSubGroup, 
        string? description = null) : this(id, name, description)
    {
        DisciplineGroup = disciplineGroup;
        DisciplineSubGroup = disciplineSubGroup;
    }

    /// <summary>
    /// Связь с подгруппой дисциплин (объектом-владельцем).
    /// </summary>
    public DisciplineSubGroupEnm DisciplineSubGroupId { get; set; }

    /// <inheritdoc cref="DisciplineSubGroupId"/>
    public DisciplineSubGroup DisciplineSubGroup { get; set; } = null!;

    /// <summary>
    /// Связь с группой дисциплин (объектом-владельцем).
    /// </summary>
    public DisciplineGroupEnm DisciplineGroupId { get; set; }

    /// <inheritdoc cref="DisciplineGroupId"/>
    public DisciplineGroup DisciplineGroup { get; set; } = null!;
        
    /// <summary>
    /// Коллекция видов программ.
    /// </summary>
    // ReSharper disable once CollectionNeverUpdated.Global
    public ICollection<SportEvent> SportEvents { get; set; } = new HashSet<SportEvent>();
    
    /// <inheritdoc cref="IEntityCopyable.Copy"/>
    // ReSharper disable once MemberCanBePrivate.Global
    public void Copy(Discipline destination)
    {
        destination.Name = Name;
        destination.Description = Description;
    }
    
    /// <inheritdoc />
    void IEntityCopyable.Copy(IAbstractEntity destination)
    {
        Copy((Discipline)destination);
    }
    
    /// <inheritdoc />
    public override string ToString()
        => Name;
}