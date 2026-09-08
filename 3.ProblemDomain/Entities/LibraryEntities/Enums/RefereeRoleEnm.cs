namespace ProblemDomain.Entities.LibraryEntities.Enums;

/// <summary>
/// Судейские должности.
/// </summary>
public enum RefereeRoleEnm
{
    /// <summary>
    /// Главный судья.
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
    /// Зам. гл. судьи по виду.
    /// </summary>
    DeputyForDiscipline,
    
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
    SeniorInspector,
    
    /// <summary>
    /// Судья-инспектор.
    /// </summary>
    Inspector,
    
    /// <summary>
    /// Старший судья по виду.
    /// </summary>
    SeniorRefereeForDiscipline,
    
    /// <summary>
    /// Старший судья-эксперт.
    /// </summary>
    SeniorExpertReferee,

    /// <summary>
    /// Старший судья старта.
    /// </summary>
    SeniorStartReferee,

    /// <summary>
    /// Старший судья этапа.
    /// </summary>
    SeniorStageReferee,
    
    /// <summary>
    /// Старший судья финиша.
    /// </summary>
    SeniorFinishReferee,
    
    /// <summary>
    /// Старший судья-контролёр.
    /// </summary>
    SeniorRefereeController,
            
    /// <summary>
    /// Судья по виду.
    /// </summary>
    RefereeForDiscipline,
    
    /// <summary>
    /// Судья-эксперт.
    /// </summary>
    ExpertReferee,

    /// <summary>
    /// Судья-постановщик.
    /// </summary>
    InstallerReferee,

    /// <summary>
    /// Судья этапа.
    /// </summary>
    StageReferee,
    
    /// <summary>
    /// Судья-контролёр.
    /// </summary>
    RefereeController,

    /// <summary>
    /// Судья-секретарь.
    /// </summary>
    Secretary,
    
    /// <summary>
    /// Судья-стартёр.
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
    
    // REMARK: в Правилах отсутствует Судья по безопасности (хотя есть зам. гл. судьи и сама служба)
    
    /// <summary>
    /// Судья-страховщик.
    /// </summary>
    InsurerReferee,
    
    /// <summary>
    /// Судья при участниках.
    /// </summary>
    AccompanyingReferee,

    #region [---------- "Не судейские" должности ----------]

    /// <summary>
    /// Председатель комиссии по допуску.
    /// </summary>
    MandateChairman,

    /// <summary>
    /// Председатель Maршрутно-квалификационной комиссии (Route Certification Commission).
    /// </summary>
    RccChairman,

    /// <summary>
    /// Председатель технической комиссии.
    /// </summary>
    TechCommissionChairman

    #endregion
}