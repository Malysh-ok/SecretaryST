using System;
using System.Collections.Generic;
using ProblemDomain.Entities._Contracts;
using ProblemDomain.Entities.CommonEntities;
using ProblemDomain.Entities.LibraryEntities.Enums;
// ReSharper disable InvalidXmlDocComment

namespace ProblemDomain.Entities.LibraryEntities;

/// <summary>
/// Судейская категория.
/// </summary>
public sealed class RefereeLevel
    : AbstractEntity, ICloneable, ICopyEntity
{
    /// <summary>
    /// Конструктор на основе готового экземпляра.
    /// </summary>
    private RefereeLevel(RefereeLevel refereeLevel)
        : this(
            refereeLevel.Id,
            refereeLevel.Name,
            refereeLevel.FullName,
            refereeLevel.Description
        )
    {
    }
    
    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="id">Идентификатор.</param>
    /// <inheritdoc />
    /// <param name="fullName">Длинное наименование.</param>
    public RefereeLevel(RefereeLevelEnm id, string name, string fullName, string? description = null) 
        : base(name, description)
    {
        Id = id;
        FullName = fullName;
    }
    
    /// <inheritdoc cref="AbstractEntity.Id"/>
    public new RefereeLevelEnm Id { get; set; }
    
    /// <summary>
    /// Полное наименование.
    /// </summary>
    public string FullName { get; set; }
    
    /// <summary>
    /// Коллекция судей.
    /// </summary>
    // ReSharper disable once CollectionNeverUpdated.Global
    public ICollection<Referee> Referees { get; set; } = new HashSet<Referee>();
    
    /// <summary>
    /// Клонирование.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public RefereeLevel Clone()
        => new(this);
    
    /// <inheritdoc />
    object ICloneable.Clone() {
        return Clone();
    }
    
    /// <inheritdoc cref="ICopyEntity.Copy"/>
    // ReSharper disable once MemberCanBePrivate.Global
    public void Copy(RefereeLevel destination)
    {
        destination.Id = Id;
        destination.Name = Name;
        destination.FullName = FullName;
        destination.Description = Description;
    }
    
    /// <inheritdoc />
    void ICopyEntity.Copy(IAbstractEntity destination)
    {
        Copy((RefereeLevel)destination);
    }
    
    /// <inheritdoc />
    public override string ToString()
        => Name;
}