using System.Collections.Generic;
using ProblemDomain.Entities._Contracts;
using ProblemDomain.Entities.LibraryEntities.Enums;
// ReSharper disable InvalidXmlDocComment
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace ProblemDomain.Entities.LibraryEntities;

/// <summary>
/// Подгруппа дисциплин.
/// </summary>
public sealed class DisciplineSubGroup : AbstractEntity<DisciplineSubGroupEnm>, IEntityCopyable
{
    /// <summary>
    /// Конструктор для EF.
    /// </summary>
    /// <param name="id">Идентификатор.</param>
    /// <inheritdoc />
    private DisciplineSubGroup(DisciplineSubGroupEnm id, string name, string? description = null)
        : base(name, description)
    {
        Id = id;
    }

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <inheritdoc />
    /// <param name="disciplineGroup">Группа дисциплин.</param>
    public DisciplineSubGroup(
        DisciplineSubGroupEnm id, 
        string name, 
        DisciplineGroup disciplineGroup,
        string? description = null) : this(id, name, description)
    {
        DisciplineGroup = disciplineGroup;
    }

    /// <summary>
    /// Связь с группой дисциплин (объектом-владельцем).
    /// </summary>
    public DisciplineGroupEnm DisciplineGroupId { get; set; }

    /// <inheritdoc cref="DisciplineGroupId"/>
    public DisciplineGroup DisciplineGroup { get; set; } = null!;

    /// <summary>
    /// Коллекция дисциплин.
    /// </summary>
    // ReSharper disable once CollectionNeverUpdated.Global
    public ICollection<Discipline> Disciplines { get; set; } = new HashSet<Discipline>();
    
    /// <summary>
    /// Коллекция возрастных групп.
    /// </summary>
    // ReSharper disable once CollectionNeverUpdated.Global
    public ICollection<AgeGroup> AgeGroups { get; set; } = new HashSet<AgeGroup>();

    /// <inheritdoc cref="IEntityCopyable.Copy"/>
    // ReSharper disable once MemberCanBePrivate.Global
    public void Copy(DisciplineSubGroup destination)
    {
        destination.Name = Name;
        destination.Description = Description;
    }

    /// <inheritdoc />
    void IEntityCopyable.Copy(IAbstractEntity destination)
    {
        Copy((DisciplineSubGroup)destination);
    }

    /// <inheritdoc />
    public override string ToString()
        => Name;
}