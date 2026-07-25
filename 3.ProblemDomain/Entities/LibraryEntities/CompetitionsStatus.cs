using System.Collections.Generic;
using ProblemDomain.Entities._Contracts;
using ProblemDomain.Entities.CommonEntities;
using ProblemDomain.Entities.LibraryEntities.Enums;
// ReSharper disable InvalidXmlDocComment
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace ProblemDomain.Entities.LibraryEntities;

/// <summary>
/// Статус соревнований.
/// </summary>
public sealed class CompetitionsStatus : AbstractEntity<CompetitionsStatusEnm>, IEntityCopyable
{
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
    
    /// <summary>
    /// Наименование во множественном числе.
    /// </summary>
    public string NamePlural { get; set; }
    
    /// <summary>
    /// Список Соревнований.
    /// </summary>
    // ReSharper disable once CollectionNeverUpdated.Global
    public ICollection<CompetitionData> Competitions { get; set; } = 
        new HashSet<CompetitionData>();
    
    /// <summary>
    /// Список Статусов и наименований соревнования.
    /// </summary>
    // ReSharper disable once CollectionNeverUpdated.Global
    public ICollection<DetailedCompetitionStatus> DetailedCompetitionStatuses { get; set; } = 
        new HashSet<DetailedCompetitionStatus>();
    
    /// <inheritdoc cref="IEntityCopyable.Copy"/>
    // ReSharper disable once MemberCanBePrivate.Global
    public void Copy(CompetitionsStatus destination)
    {
        destination.Name = Name;
        destination.NamePlural = NamePlural;
        destination.Description = Description;
    }
    
    /// <inheritdoc />
    void IEntityCopyable.Copy(IAbstractEntity destination)
    {
        Copy((CompetitionsStatus)destination);
    }
    
    /// <inheritdoc />
    public override string ToString()
        => Name;
}