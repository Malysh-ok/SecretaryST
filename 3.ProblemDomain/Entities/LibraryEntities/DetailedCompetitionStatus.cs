using System;
using System.Collections.Generic;
using ProblemDomain.Entities._Contracts;
using ProblemDomain.Entities.LibraryEntities.Enums;
// ReSharper disable InvalidXmlDocComment
// ReSharper disable MemberCanBePrivate.Global

namespace ProblemDomain.Entities.LibraryEntities;

/// <summary>
/// Статус и обобщенное наименование соревнования.
/// </summary>
public sealed class DetailedCompetitionStatus
    : AbstractEntity, ICloneable, ICopy
{
    /// <summary>
    /// Конструктор для EF.
    /// </summary>
    /// <param name="id">Идентификатор.</param>
    /// <inheritdoc />
    private DetailedCompetitionStatus(DetailedCompetitionStatusEnm id, string name, string? description = null) 
        : base(name, description)
    {
        Id = id;
    }
    
    /// <summary>
    /// Конструктор на основе готового экземпляра.
    /// </summary>
    private DetailedCompetitionStatus(DetailedCompetitionStatus detailedCompetitionStatus)
        : this(
            detailedCompetitionStatus.Id,
            detailedCompetitionStatus.Name,
            detailedCompetitionStatus.CompetitionsStatus,
            detailedCompetitionStatus.Description
        )
    {
    }

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <inheritdoc />
    /// <param name="competitionsStatus">Статус соревнований.</param>
    public DetailedCompetitionStatus(DetailedCompetitionStatusEnm id, string name, 
        CompetitionsStatus competitionsStatus, string? description = null) 
        : this(id, name, description)
    {
        CompetitionsStatus = competitionsStatus;
    }
    
    /// <inheritdoc cref="AbstractEntity.Id"/>
    public new DetailedCompetitionStatusEnm Id { get; set; }
    
    /// <summary>
    /// Связь со статусом соревнований (объектом-владельцем).
    /// </summary>
    public CompetitionsStatusEnm CompetitionsStatusId { get; set; }

    /// <inheritdoc cref="CompetitionsStatusId"/>
    public CompetitionsStatus CompetitionsStatus { get; set; } = null!;
    
        
    /// <summary>
    /// Список Соревнований.
    /// </summary>
    // ReSharper disable once CollectionNeverUpdated.Global
    public ICollection<Competition> Competitions { get; set; } = 
        new HashSet<Competition>();
    
    /// <summary>
    /// Клонирование.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public DetailedCompetitionStatus Clone()
        => new(this);
    
    /// <inheritdoc />
    object ICloneable.Clone() {
        return Clone();
    }
    
    /// <inheritdoc cref="ICopy.Copy"/>
    // ReSharper disable once MemberCanBePrivate.Global
    public void Copy(DetailedCompetitionStatus destination)
    {
        destination.Id = Id;
        destination.Name = Name;
        destination.CompetitionsStatus = CompetitionsStatus;
        destination.Description = Description;
    }
    
    /// <inheritdoc />
    void ICopy.Copy(IAbstractEntity destination)
    {
        Copy((DetailedCompetitionStatus)destination);
    }}