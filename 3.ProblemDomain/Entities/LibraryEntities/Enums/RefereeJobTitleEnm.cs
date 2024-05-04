namespace ProblemDomain.Entities.LibraryEntities.Enums;

/// <summary>
/// Судейские должности.
/// </summary>
public enum RefereeJobTitleEnm
{
    /// <summary>
    /// Главный судья
    /// </summary>
    ChiefReferee = 1,

    /// <summary>
    /// Главный секретарь.
    /// </summary>
    ChiefSecretary,

    /// <summary>
    /// Зам. гл. судьи по судейству.
    /// </summary>
    DeputyForRefereeing,
    
    /// <summary>
    /// Зам. гл. судьи по безопасности.
    /// </summary>
    DeputyForSecurity,
    
    /// <summary>
    /// Зам. гл. судьи по информации.
    /// </summary>
    DeputyForInformation,
    
    /// <summary>
    /// Зам. гл. судьи по СТО.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    DeputyForSTS,
    
    /// <summary>
    /// Зам. гл. судьи.
    /// </summary>
    Deputy,

    /// <summary>
    /// Зам. гл. секретаря.
    /// </summary>
    DeputyChiefSecretary,
    
    /// <summary>
    /// Начальник дистанции.
    /// </summary>
    HeadOfDistance,
    
    /// <summary>
    /// Старший судья-инспектор.
    /// </summary>
    MajorInspector,
    
    /// <summary>
    /// Судья-инспектор.
    /// </summary>
    Inspector,
    
    /// <summary>
    /// Старший судья старта.
    /// </summary>
    MajorStartReferee,
    
    /// <summary>
    /// Старший судья этапа.
    /// </summary>
    MajorStageReferee,
    
    /// <summary>
    /// Старший судья финиша.
    /// </summary>
    MajorFinishReferee,
    
    /// <summary>
    /// Судья этапа.
    /// </summary>
    StageReferee,
    
    /// <summary>
    /// Судья-постановщик.
    /// </summary>
    InstallerReferee,
    
    /// <summary>
    /// Судья-секретарь.
    /// </summary>
    Secretary,
    
    /// <summary>
    /// Судья-стартер.
    /// </summary>
    StarterReferee,
    
    /// <summary>
    /// Судья-хронометрист.
    /// </summary>
    TimekeeperReferee,
    
    /// <summary>
    /// Судья по информации.
    /// </summary>
    InformationReferee,
    
    /// <summary>
    /// Судья-страховщик.
    /// </summary>
    InsurerReferee,
    
    /// <summary>
    /// Судья при участниках.
    /// </summary>
    AccompanyingReferee,

        
    /// <summary>
    /// Председатель комиссии по допуску.
    /// </summary>
    MandateChairman,

    /// <summary>
    /// Председатель технической комиссии.
    /// </summary>
    TechCommissionChairman
}