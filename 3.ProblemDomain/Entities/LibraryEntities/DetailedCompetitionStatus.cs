using System.Collections.Generic;
using ProblemDomain.Entities._Contracts;
using ProblemDomain.Entities.CommonEntities;
using ProblemDomain.Entities.LibraryEntities.Enums;
// ReSharper disable InvalidXmlDocComment
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace ProblemDomain.Entities.LibraryEntities;

/// <summary>
/// Статус и обобщенное наименование соревнования.
/// </summary>
public sealed class DetailedCompetitionStatus : AbstractEntity<DetailedCompetitionStatusEnm>, IEntityCopyable
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
    /// Конструктор.
    /// </summary>
    /// <inheritdoc />
    /// <param name="competitionsStatus">Статус соревнований.</param>
    public DetailedCompetitionStatus(
        DetailedCompetitionStatusEnm id, 
        string name, 
        CompetitionsStatus competitionsStatus, 
        string? description = null) : this(id, name, description)
    {
        CompetitionsStatus = competitionsStatus;
    }
    
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
    public ICollection<CompetitionData> Competitions { get; set; } = 
        new HashSet<CompetitionData>();
    
    /// <inheritdoc cref="IEntityCopyable.Copy"/>
    // ReSharper disable once MemberCanBePrivate.Global
    public void Copy(DetailedCompetitionStatus destination)
    {
        destination.Name = Name;
        destination.Description = Description;
    }
    
    /// <inheritdoc />
    void IEntityCopyable.Copy(IAbstractEntity destination)
    {
        Copy((DetailedCompetitionStatus)destination);
    }
    
    /// <inheritdoc />
    public override string ToString()
        => Name;
}