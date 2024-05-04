using System;
using System.Collections.Generic;
using ProblemDomain.Entities._Contracts;
using ProblemDomain.Entities.CommonEntities;
using ProblemDomain.Entities.LibraryEntities;
using ProblemDomain.Entities.LibraryEntities.Enums;
// ReSharper disable InvalidXmlDocComment

namespace ProblemDomain.Entities;

/// <summary>
/// Соревнование.
/// </summary>
/// <remarks>
/// Маловероятно, но в рамках одного мероприятия может быть несколько соревнований.
/// </remarks>
public sealed class Competition
    : AbstractEntity, ICloneable, ICopy
{
    /// <summary>
    /// Конструктор для EF.
    /// </summary>
    /// <inheritdoc />
    /// <param name="conductingOrganizations">Проводящие организации.</param>
    /// <param name="venue">Место проведения.</param>
    /// <param name="date">Дата проведения.</param>
    /// <param name="dayCount">Количество дней.</param>
    private Competition(string name, 
        string conductingOrganizations, DateTime date, int dayCount, string venue, 
        string? description = null) 
        : base(name, description)
    {
        ConductingOrganizations = conductingOrganizations;
        Venue = venue;
        Date = date;
        DayCount = dayCount;
    }
    
    /// <summary>
    /// Конструктор на основе готового экземпляра.
    /// </summary>
    private Competition(Competition competition)
        : this(
            competition.Name,
            competition.ConductingOrganizations,
            competition.Date,
            competition.DayCount,
            competition.Venue,
            competition.CompetitionsStatus,
            competition.DetailedCompetitionStatus,
            competition.ChiefReferee,
            competition.ChiefSecretary,
            competition.MandateChairman,
            competition.Secretaries,
            competition.Description
        )
    {
    }

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <inheritdoc />
    /// 
    public Competition(string name,
        string conductingOrganizations, DateTime date, int dayCount, string venue,
        CompetitionsStatus competitionsStatus, DetailedCompetitionStatus detailedCompetitionStatus,
        Referee chiefReferee, Referee chiefSecretary, Referee mandateChairman, List<Referee> secretaries,
        string? description = null)
        : this(name, conductingOrganizations, date, dayCount, venue, description)
    {
        CompetitionsStatus = competitionsStatus;
        DetailedCompetitionStatus = detailedCompetitionStatus;
        ChiefReferee = chiefReferee;
        ChiefSecretary = chiefSecretary;
        MandateChairman = mandateChairman;
        Secretaries = secretaries;
    }
    
    // TODO: Name переделать в список?

    /// <summary>
    /// Проводящие организации.
    /// </summary>
    // TODO: Переделать в список.
    public string ConductingOrganizations { get; set; }
    
    /// <summary>
    /// Дата проведения.
    /// </summary>
    public DateTime Date  { get; set; }
    
    /// <summary>
    /// Количество дней проведения.
    /// </summary>
    public int DayCount { get; set; }
        
    /// <summary>
    /// Место проведения.
    /// </summary>
    public string Venue { get; set; }
    
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
    
    #region [----- Связь с судьями -----]

    /// <summary>
    /// Главный судья.
    /// </summary>
    /// <remarks>Связь с объектом-владельцем.</remarks>
    public JobTitleEnm ChiefRefereeId { get; set; }
    
    /// <inheritdoc cref="ChiefRefereeId"/>
    public Referee ChiefReferee { get; set; } = null!;

    /// <summary>
    /// Главный секретарь.
    /// </summary>
    /// <remarks>Связь с объектом-владельцем.</remarks>
    public JobTitleEnm ChiefSecretaryId { get; set; }
    
    /// <inheritdoc cref="ChiefSecretaryId"/>
    public Referee ChiefSecretary { get; set; } = null!;

    /// <summary>
    /// Председатель комиссии по допуску.
    /// </summary>
    /// <remarks>Связь с объектом-владельцем.</remarks>
    public JobTitleEnm MandateChairmanId  { get; set; }
    
    /// <inheritdoc cref="MandateChairmanId"/>
    public Referee MandateChairman  { get; set; } = null!;
    
    /// <summary>
    /// Список секретарей.
    /// </summary>
    public List<Referee> Secretaries { get; set; } = [];
    
    #endregion
    
    /// <summary>
    /// Клонирование.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public Competition Clone()
        => new(this);
    
    /// <inheritdoc />
    object ICloneable.Clone() {
        return Clone();
    }
    
    /// <inheritdoc cref="ICopy.Copy"/>
    // ReSharper disable once MemberCanBePrivate.Global
    // TODO: Не понятно, что делать с копированием списка секретарей
    public void Copy(Competition destination)
    {
        destination.Name = Name;
        destination.Description = Description;
        destination.ConductingOrganizations = ConductingOrganizations;
        destination.Date = Date;
        destination.DayCount = DayCount;
        destination.Venue = Venue;
        destination.CompetitionsStatus = CompetitionsStatus;
        destination.DetailedCompetitionStatus = DetailedCompetitionStatus;
        destination.ChiefReferee = ChiefReferee;
        destination.ChiefSecretary = ChiefSecretary;
        destination.MandateChairman = MandateChairman;
    }
    
    /// <inheritdoc />
    void ICopy.Copy(IAbstractEntity destination)
    {
        Copy((Competition)destination);
    }
}