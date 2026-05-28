using System;
using System.Collections.Generic;
using ProblemDomain.Entities._Contracts;
using ProblemDomain.Entities.CommonEntities;
using ProblemDomain.Entities.LibraryEntities.Enums;
// ReSharper disable InvalidXmlDocComment

namespace ProblemDomain.Entities.LibraryEntities;

/// <summary>
/// Судейская должность.
/// </summary>
public sealed class RefereeJobTitle
    : AbstractEntity, ICloneable, ICopyEntity
{
    /// <summary>
    /// Конструктор на основе готового экземпляра.
    /// </summary>
    private RefereeJobTitle(RefereeJobTitle refereeJobTitle)
        : this(
            refereeJobTitle.Id,
            refereeJobTitle.Name,
            refereeJobTitle.Description
        )
    {
    }
    
    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="id">Идентификатор.</param>
    /// <inheritdoc />
    public RefereeJobTitle(RefereeJobTitleEnm id, string name, string? description = null) 
        : base(name, description)
    {
        Id = id;
    }
    
    /// <inheritdoc cref="AbstractEntity.Id"/>
    public new RefereeJobTitleEnm Id { get; set; }

    /// <summary>
    /// Коллекция судей.
    /// </summary>
    // ReSharper disable once CollectionNeverUpdated.Global
    public ICollection<Referee> Referees { get; set; } = new HashSet<Referee>();

    /// <summary>
    /// Клонирование.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public RefereeJobTitle Clone()
        => new(this);
    
    /// <inheritdoc />
    object ICloneable.Clone() {
        return Clone();
    }
    
    /// <inheritdoc cref="ICopyEntity.Copy"/>
    // ReSharper disable once MemberCanBePrivate.Global
    public void Copy(RefereeJobTitle destination)
    {
        destination.Id = Id;
        destination.Name = Name;
        destination.Description = Description;
    }
    
    /// <inheritdoc />
    void ICopyEntity.Copy(IAbstractEntity destination)
    {
        Copy((RefereeJobTitle)destination);
    }

    /// <inheritdoc />
    public override string ToString()
        => Name;
}