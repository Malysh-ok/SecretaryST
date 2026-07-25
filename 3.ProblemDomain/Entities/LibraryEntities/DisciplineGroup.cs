using System.Collections.Generic;
using ProblemDomain.Entities._Contracts;
using ProblemDomain.Entities.LibraryEntities.Enums;
// ReSharper disable InvalidXmlDocComment
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace ProblemDomain.Entities.LibraryEntities;

/// <summary>
/// Группа дисциплин.
/// </summary>
public sealed class DisciplineGroup : AbstractEntity<DisciplineGroupEnm>, IEntityCopyable
{
    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="id">Идентификатор.</param>
    /// <inheritdoc />
    public DisciplineGroup(
        DisciplineGroupEnm id, 
        string name, 
        string? description = null) : base(name, description)
    {
        Id = id;
    }

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
    /// Коллекция трудностей.
    /// </summary>
    // ReSharper disable once CollectionNeverUpdated.Global
    public ICollection<Difficulty> Difficulties { get; set; } = new HashSet<Difficulty>();
    
    /// <inheritdoc cref="IEntityCopyable.Copy"/>
    // ReSharper disable once MemberCanBePrivate.Global
    public void Copy(DisciplineGroup destination)
    {
        destination.Name = Name;
        destination.Description = Description;
    }

    /// <inheritdoc />
    void IEntityCopyable.Copy(IAbstractEntity destination)
    {
        Copy((DisciplineGroup)destination);
    }

    /// <inheritdoc />
    public override string ToString()
        => Name;
}