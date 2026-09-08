using System.Collections.Generic;
using System.Data.SqlTypes;
using ProblemDomain.Entities._Contracts;
using ProblemDomain.Entities.CommonEntities;
using ProblemDomain.Entities.LibraryEntities.Enums;
// ReSharper disable InvalidXmlDocComment
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace ProblemDomain.Entities.LibraryEntities;

/// <summary>
/// Судейская должность.
/// </summary>
public sealed class RefereeRole : AbstractEntity<RefereeRoleEnm>, IEntityCopyable
{
    /// <summary>
    /// Конструктор для EF.
    /// </summary>
    /// <param name="id">Идентификатор.</param>
    /// <inheritdoc />
    private RefereeRole(
        RefereeRoleEnm id, 
        string name, 
        string? description = null) : base(name, description)
    {
        Id = id;
    }

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="id">Идентификатор.</param>
    /// <inheritdoc />
    public RefereeRole(
        RefereeRoleEnm id, 
        string name, 
        ICollection<DisciplineGroup> disciplineGroups,
        bool isAvailable = true,
        string? description = null) : this(id, name, description)
    {
        DisciplineGroups = disciplineGroups;
        Id = id;
    }

    /// <summary>
    /// Коллекция судей.
    /// </summary>
    // ReSharper disable once CollectionNeverUpdated.Global
    public ICollection<Referee> Referees { get; set; } = new HashSet<Referee>();

    /// <summary>
    /// Коллекция групп дисциплин.
    /// </summary>
    public ICollection<DisciplineGroup> DisciplineGroups { get; set; } = new HashSet<DisciplineGroup>();
    
    /// <inheritdoc cref="IEntityCopyable.Copy"/>
    // ReSharper disable once MemberCanBePrivate.Global
    public void Copy(RefereeRole destination)
    {
        destination.Name = Name;
        destination.Description = Description;
    }

    /// <inheritdoc />
    void IEntityCopyable.Copy(IAbstractEntity destination)
    {
        Copy((RefereeRole)destination);
    }

    /// <inheritdoc />
    public override string ToString()
        => Name;
}