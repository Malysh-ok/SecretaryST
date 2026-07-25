using System.Collections.Generic;
using ProblemDomain.Entities._Contracts;
using ProblemDomain.Entities.CommonEntities;
using ProblemDomain.Entities.LibraryEntities.Enums;
// ReSharper disable InvalidXmlDocComment
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace ProblemDomain.Entities.LibraryEntities;

/// <summary>
/// Судейская категория.
/// </summary>
public sealed class RefereeLevel : AbstractEntity<RefereeLevelEnm>, IEntityCopyable
{
    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="id">Идентификатор.</param>
    /// <inheritdoc />
    /// <param name="fullName">Полное наименование.</param>
    public RefereeLevel(
        RefereeLevelEnm id, 
        string name, 
        string fullName, 
        string? description = null) : base(name, description)
    {
        Id = id;
        FullName = fullName;
    }

    /// <summary>
    /// Полное наименование.
    /// </summary>
    public string FullName { get; set; }

    /// <summary>
    /// Коллекция судей.
    /// </summary>
    // ReSharper disable once CollectionNeverUpdated.Global
    public ICollection<Referee> Referees { get; set; } = new HashSet<Referee>();

    /// <inheritdoc cref="IEntityCopyable.Copy"/>
    // ReSharper disable once MemberCanBePrivate.Global
    public void Copy(RefereeLevel destination)
    {
        destination.Name = Name;
        destination.FullName = FullName;
        destination.Description = Description;
    }

    /// <inheritdoc />
    void IEntityCopyable.Copy(IAbstractEntity destination)
    {
        Copy((RefereeLevel)destination);
    }

    /// <inheritdoc />
    public override string ToString()
        => Name;
}