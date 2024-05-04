namespace ProblemDomain.Entities.LibraryEntities.Enums;

/// <summary>
/// Статусы и наименования спортивных соревнований.
/// </summary>
public enum DetailedCompetitionStatusEnm
{
    /// <summary>
    /// Чемпионат России.
    /// </summary>
    RussianChampionship = 1,
    
    /// <summary>
    /// Кубок России.
    /// </summary>
    RussianCup,
    
    /// <summary>
    /// Первенство России.
    /// </summary>
    RussianJuniorChampionship,
    
    /// <summary>
    /// Другие официальные всероссийские спортивные соревнования.
    /// </summary>
    OtherRussianCompetition,
    
    /// <summary>
    /// Чемпионат одного или нескольких федеральных округов.
    /// </summary>
    FederalDistrictChampionship,
    
    /// <summary>
    /// Первенство одного или нескольких федеральных округов.
    /// </summary>
    FederalDistrictJuniorChampionship,
    
    /// <summary>
    /// Чемпионат субъекта Российской Федерации.
    /// </summary>
    RegionalChampionship,
    
    /// <summary>
    /// Кубок субъекта Российской Федерации.
    /// </summary>
    RegionalCup,
    
    /// <summary>
    /// Первенство субъекта Российской Федерации.
    /// </summary>
    RegionalJuniorChampionship,
    
    /// <summary>
    /// Другие официальные спортивные соревнования субъекта Российской Федерации.
    /// </summary>
    OtherRegionalCompetition,
    
    /// <summary>
    /// Чемпионат муниципального образования.
    /// </summary>
    MunicipalChampionship,
    
    /// <summary>
    /// Первенство муниципального образования.
    /// </summary>
    MunicipalJuniorChampionship,
    
    /// <summary>
    /// Другие официальные соревнования муниципального образования.
    /// </summary>
    OtherMunicipalCompetition
}