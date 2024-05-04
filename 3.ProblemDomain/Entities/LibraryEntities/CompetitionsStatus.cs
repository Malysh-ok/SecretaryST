using System;
using System.Collections.Generic;
using ProblemDomain.Entities._Contracts;
using ProblemDomain.Entities.LibraryEntities.Enums;
// ReSharper disable InvalidXmlDocComment

namespace ProblemDomain.Entities.LibraryEntities;

/// <summary>
/// Статус соревнований.
/// </summary>
public sealed class CompetitionsStatus : AbstractEntity, ICloneable, ICopy
{
    /// <summary>
    /// Конструктор на основе готового экземпляра.
    /// </summary>
    private CompetitionsStatus(CompetitionsStatus competitionsStatus)
        : this(
            competitionsStatus.Id,
            competitionsStatus.Name,
            competitionsStatus.NamePlural,
            competitionsStatus.Description
        )
    {
    }

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="id">Идентификатор.</param>
    /// <inheritdoc />
    /// <param name="namePlural">Наименование во множественном числе.</param>
    public CompetitionsStatus(CompetitionsStatusEnm id, string name, string namePlural, string? description = null) 
        : base(name, description)
    {
        Id = id;
        NamePlural = namePlural;
    }
    
    /// <inheritdoc cref="AbstractEntity.Id"/>
    public new CompetitionsStatusEnm Id { get; set; }
    
    /// <summary>
    /// Наименование во множественном числе.
    /// </summary>
    public string NamePlural { get; set; }
    
    /// <summary>
    /// Список Соревнований.
    /// </summary>
    // ReSharper disable once CollectionNeverUpdated.Global
    public ICollection<Competition> Competitions { get; set; } = 
        new HashSet<Competition>();
    
    /// <summary>
    /// Список Статусов и наименований соревнования.
    /// </summary>
    // ReSharper disable once CollectionNeverUpdated.Global
    public ICollection<DetailedCompetitionStatus> DetailedCompetitionStatuses { get; set; } = 
        new HashSet<DetailedCompetitionStatus>();
    
    /// <summary>
    /// Клонирование.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public CompetitionsStatus Clone()
        => new(this);
    
    /// <inheritdoc />
    object ICloneable.Clone() {
        return Clone();
    }
    
    /// <inheritdoc cref="ICopy.Copy"/>
    // ReSharper disable once MemberCanBePrivate.Global
    public void Copy(CompetitionsStatus destination)
    {
        destination.Id = Id;
        destination.Name = Name;
        destination.NamePlural = NamePlural;
        destination.Description = Description;
    }
    
    /// <inheritdoc />
    void ICopy.Copy(IAbstractEntity destination)
    {
        Copy((CompetitionsStatus)destination);
    }
}