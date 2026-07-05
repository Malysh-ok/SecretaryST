using System;
using System.Collections.Generic;
using ProblemDomain.Entities._Contracts;
using ProblemDomain.Entities.DistanceEntities;
using ProblemDomain.Entities.LibraryEntities;
using ProblemDomain.Entities.LibraryEntities.Enums;

// ReSharper disable InvalidXmlDocComment

namespace ProblemDomain.Entities.CommonEntities;

/// <summary>
/// Соревнование (данные о соревновании).
/// </summary>
public sealed class CompetitionData
    : AbstractEntity, ICloneable, ICopyEntity
{
    /// <summary>
    /// Конструктор для EF.
    /// </summary>
    /// <inheritdoc />
    /// <param name="conductingOrganizations">Проводящие организации.</param>
    /// <param name="initialDate">Начальная дата проведения.</param>
    /// <param name="endDate">Конечная дата проведения.</param>
    /// <param name="venue">Место проведения.</param>
    /// <param name="shortName">Краткое название.</param>
    private CompetitionData(string name, 
        IList<string> conductingOrganizations, DateTime initialDate, DateTime endDate, string venue, string shortName,
        string? description = null) 
        : base(name, description)
    {
        ConductingOrganizations = conductingOrganizations;
        Venue = venue;
        InitialDate = initialDate;
        EndDate = endDate;
        ShortName = shortName;
    }
    
    /// <summary>
    /// Конструктор на основе готового экземпляра.
    /// </summary>
    private CompetitionData(CompetitionData competitionData)
        : this(
            competitionData.Name,
            competitionData.ConductingOrganizations,
            competitionData.InitialDate,
            competitionData.EndDate,
            competitionData.Venue,
            competitionData.ShortName,
            competitionData.CompetitionsStatus,
            competitionData.DetailedCompetitionStatus,
            competitionData.Description
        )
    {
    }

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <inheritdoc />
    /// <param name="competitionsStatus">Статус соревнований.</param>
    /// <param name="detailedCompetitionStatus">Статус и наименования соревнования.</param>
    public CompetitionData(string name,
        IList<string> conductingOrganizations, DateTime initialDate, DateTime endDate, string venue, string shortName,
        CompetitionsStatus competitionsStatus, DetailedCompetitionStatus detailedCompetitionStatus,
        string? description = null)
        : this(name, conductingOrganizations, initialDate, endDate, venue, shortName, description)
    {
        CompetitionsStatus = competitionsStatus;
        DetailedCompetitionStatus = detailedCompetitionStatus;
    }

    /// <summary>
    /// Проводящие организации.
    /// </summary>
    public IList<string> ConductingOrganizations { get; set; }
    
    /// <summary>
    /// Начальная дата проведения.
    /// </summary>
    public DateTime InitialDate  { get; set; }
    
    /// <summary>
    /// Конечная дата проведения.
    /// </summary>
    public DateTime EndDate  { get; set; }
        
    /// <summary>
    /// Место проведения.
    /// </summary>
    public string Venue { get; set; }
    
    /// <summary>
    /// Краткое название соревнований.
    /// </summary>
    public string ShortName { get; set; }

    /// <summary>
    /// Статус соревнований.
    /// </summary>
    /// <remarks>Связь с объектом-владельцем.</remarks>
    public CompetitionsStatusEnm CompetitionsStatusId { get; set; }

    /// <inheritdoc cref="CompetitionsStatusId"/>
    public CompetitionsStatus CompetitionsStatus { get; set; } = null!;
    
    /// <summary>
    /// Статус и наименования соревнования.
    /// </summary>
    /// <remarks>Связь с объектом-владельцем.</remarks>
    public DetailedCompetitionStatusEnm DetailedCompetitionStatusId { get; set; }
    
    /// <inheritdoc cref="DetailedCompetitionStatusId"/>
    public DetailedCompetitionStatus DetailedCompetitionStatus { get; set; } = null!;
    
    /// <summary>
    /// Коллекция судей.
    /// </summary>
    public ICollection<Referee> Referees { get; set; } = new HashSet<Referee>();

    /// <summary>
    /// Коллекция видов программы.
    /// </summary>
    public ICollection<SportEvent> SportEvents { get; set; } = new HashSet<SportEvent>();

    /// <summary>
    /// Коллекция делегаций.
    /// </summary>
    public ICollection<Delegation> Delegations { get; set; } = new HashSet<Delegation>();
    
    /// <summary>
    /// Клонирование.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public CompetitionData Clone()
        => new(this);
    
    /// <inheritdoc />
    object ICloneable.Clone() {
        return Clone();
    }
    
    /// <inheritdoc cref="ICopyEntity.Copy"/>
    // ReSharper disable once MemberCanBePrivate.Global
    public void Copy(CompetitionData destination)
    {
        destination.Name = Name;
        destination.ShortName = ShortName;
        destination.Description = Description;
        destination.ConductingOrganizations = ConductingOrganizations;
        destination.InitialDate = InitialDate;
        destination.EndDate = EndDate;
        destination.Venue = Venue;
        destination.CompetitionsStatus = CompetitionsStatus;
        destination.DetailedCompetitionStatus = DetailedCompetitionStatus;
    }
    
    /// <inheritdoc />
    void ICopyEntity.Copy(IAbstractEntity destination)
    {
        Copy((CompetitionData)destination);
    }
    
    /// <inheritdoc />
    public override string ToString()
        => ShortName;
}