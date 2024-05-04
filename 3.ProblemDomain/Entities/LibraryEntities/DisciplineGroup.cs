using System;
using System.Collections.Generic;
using ProblemDomain.Entities._Contracts;
using ProblemDomain.Entities.LibraryEntities.Enums;
// ReSharper disable InvalidXmlDocComment

namespace ProblemDomain.Entities.LibraryEntities;

/// <summary>
/// Группа дисциплин.
/// </summary>
public sealed class DisciplineGroup
    : AbstractEntity, ICloneable, ICopy
{
    /// <summary>
    /// Конструктор на основе готового экземпляра.
    /// </summary>
    private DisciplineGroup(DisciplineGroup disciplineGroup)
        : this(
            disciplineGroup.Id,
            disciplineGroup.Name,
            disciplineGroup.Description
        )
    {
    }
        
    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="id">Идентификатор.</param>
    /// <inheritdoc />
    public DisciplineGroup(DisciplineGroupEnm id, string name, string? description = null) 
        : base(name, description)
    {
        Id = id;
    }
        
        
    /// <inheritdoc cref="AbstractEntity.Id"/>
    public new DisciplineGroupEnm Id { get; set; }
        
    /// <summary>
    /// Коллекция подгрупп дисциплин.
    /// </summary>
    // ReSharper disable once CollectionNeverUpdated.Global
    public ICollection<DisciplineSubGroup> DisciplineSubGroups { get; set; } = new HashSet<DisciplineSubGroup>();

    /// <summary>
    /// Коллекция дисциплин.
    /// </summary>
    // ReSharper disable once CollectionNeverUpdated.Global
    public ICollection<Discipline> Disciplines { get; set; } = new HashSet<Discipline>();
        
    /// <summary>
    /// Клонирование.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public DisciplineGroup Clone()
        => new(this);
    
    /// <inheritdoc />
    object ICloneable.Clone() {
        return Clone();
    }
    
    /// <inheritdoc cref="ICopy.Copy"/>
    // ReSharper disable once MemberCanBePrivate.Global
    public void Copy(DisciplineGroup destination)
    {
        destination.Id = Id;
        destination.Name = Name;
        destination.Description = Description;
    }
    
    /// <inheritdoc />
    void ICopy.Copy(IAbstractEntity destination)
    {
        Copy((DisciplineGroup)destination);
    }
}