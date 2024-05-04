using System;
using ProblemDomain.Entities._Contracts;
using ProblemDomain.Entities.LibraryEntities;
using ProblemDomain.Entities.LibraryEntities.Enums;

// ReSharper disable InvalidXmlDocComment

namespace ProblemDomain.Entities.CommonEntities;

/// <summary>
/// Данные о соревнования.
/// </summary>
/// <remarks>
/// Соревнования могут быть только в единственном экземпляре!
/// </remarks>
public sealed class CompetitionData
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
    private CompetitionData(string name, 
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
    private CompetitionData(CompetitionData competitionData)
        : this(
            competitionData.Name,
            competitionData.ConductingOrganizations,
            competitionData.Date,
            competitionData.DayCount,
            competitionData.Venue,
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
    /// 
    public CompetitionData(string name,
        string conductingOrganizations, DateTime date, int dayCount, string venue,
        CompetitionsStatus competitionsStatus, DetailedCompetitionStatus detailedCompetitionStatus,
        string? description = null)
        : this(name, conductingOrganizations, date, dayCount, venue, description)
    {
        CompetitionsStatus = competitionsStatus;
        DetailedCompetitionStatus = detailedCompetitionStatus;
    }
    
    // TODO: Name переделать в список?

    /// <summary>
    /// Проводящие организации.
    /// </summary>
    // TODO: Переделать в список?
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
    
    /// <inheritdoc cref="ICopy.Copy"/>
    // ReSharper disable once MemberCanBePrivate.Global
    // TODO: Не понятно, что делать с копированием списка секретарей
    public void Copy(CompetitionData destination)
    {
        destination.Name = Name;
        destination.Description = Description;
        destination.ConductingOrganizations = ConductingOrganizations;
        destination.Date = Date;
        destination.DayCount = DayCount;
        destination.Venue = Venue;
        destination.CompetitionsStatus = CompetitionsStatus;
        destination.DetailedCompetitionStatus = DetailedCompetitionStatus;
    }
    
    /// <inheritdoc />
    void ICopy.Copy(IAbstractEntity destination)
    {
        Copy((CompetitionData)destination);
    }
}