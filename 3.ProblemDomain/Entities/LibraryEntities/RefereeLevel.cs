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
    : AbstractEntity, ICloneable, ICopy
{
    /// <summary>
    /// Конструктор на основе готового экземпляра.
    /// </summary>
    private RefereeLevel(RefereeLevel refereeLevel)
        : this(
            refereeLevel.Id,
            refereeLevel.Name,
            refereeLevel.LongName,
            refereeLevel.Description
        )
    {
    }
    
    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="id">Идентификатор.</param>
    /// <inheritdoc />
    /// <param name="longName">Длинное наименование.</param>
    public RefereeLevel(RefereeLevelEnm id, string name, string longName, string? description = null) 
        : base(name, description)
    {
        Id = id;
        LongName = longName;
    }
    
    /// <inheritdoc cref="AbstractEntity.Id"/>
    public new RefereeLevelEnm Id { get; set; }
    
    /// <summary>
    /// Длинное наименование.
    /// </summary>
    public string LongName { get; set; }
    
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
    
    /// <inheritdoc cref="ICopy.Copy"/>
    // ReSharper disable once MemberCanBePrivate.Global
    public void Copy(RefereeLevel destination)
    {
        destination.Id = Id;
        destination.Name = Name;
        destination.LongName = LongName;
        destination.Description = Description;
    }
    
    /// <inheritdoc />
    void ICopy.Copy(IAbstractEntity destination)
    {
        Copy((RefereeLevel)destination);
    }
}