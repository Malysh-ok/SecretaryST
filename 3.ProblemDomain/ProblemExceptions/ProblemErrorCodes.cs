namespace ProblemDomain.ProblemExceptions;

/// <summary>
/// Содержит строковые коды ошибок предметной области (3.ProblemDomain).
/// </summary>
/// <remarks>
/// Коды используются для идентификации типа ошибки и последующей локализации сообщения.
/// Коды должны быть уникальными в пределах домена.
/// </remarks>
public static class ProblemErrorCodes
{
    public const string CompetitionDataCreateError = nameof(CompetitionDataCreateError);
    public const string CompetitionDataIsNull = nameof(CompetitionDataIsNull);
    public const string CompetitionDataListLoadError = nameof(CompetitionDataListLoadError);
    public const string CompetitionDataLoadError = nameof(CompetitionDataLoadError);
    public const string CompetitionDataNotFound = nameof(CompetitionDataNotFound);
    public const string CompetitionDataRemoveError = nameof(CompetitionDataRemoveError);
    public const string CompetitionDataSaveError = nameof(CompetitionDataSaveError);
    public const string CompetitionStatusesLoadError = nameof(CompetitionStatusesLoadError);
    public const string CompetitionStatusFindError = nameof(CompetitionStatusFindError);
    
    public const string ConductingOrganizationCreateError = nameof(ConductingOrganizationCreateError);
    public const string ConductingOrganizationRemoveError = nameof(ConductingOrganizationRemoveError);
    public const string ConductingOrganizationsLoadError = nameof(ConductingOrganizationsLoadError);
    public const string ConductingOrganizationsSetError = nameof(ConductingOrganizationsSetError);
    
    public const string DetailedCompetitionStatusesLoadError = nameof(DetailedCompetitionStatusesLoadError);
    public const string DetailedCompetitionStatusFindError = nameof(DetailedCompetitionStatusFindError);

    public const string DifficultyLoadError = nameof(DifficultyLoadError);
    public const string DifficultiesLoadError = nameof(DifficultiesLoadError);

    public const string DisciplineGroupsLoadError = nameof(DisciplineGroupsLoadError);
    public const string DisciplineLoadError = nameof(DisciplineLoadError);
    public const string DisciplinesLoadError = nameof(DisciplinesLoadError);
    public const string DisciplineSubGroupsLoadError = nameof(DisciplineSubGroupsLoadError);
    
    public const string RefereeCreateError = nameof(RefereeCreateError);
    public const string RefereeJobTitleFindError = nameof(RefereeJobTitleFindError);
    public const string RefereeJobTitlesLoadError = nameof(RefereeJobTitlesLoadError);
    public const string RefereeLevelFindError = nameof(RefereeLevelFindError);
    public const string RefereeLevelsLoadError = nameof(RefereeLevelsLoadError);
    public const string RefereeRemoveError = nameof(RefereeRemoveError);
    public const string RefereesLoadError = nameof(RefereesLoadError);
    
    public const string SportEventCreateError = nameof(SportEventCreateError);
    public const string SportEventRemoveError = nameof(SportEventRemoveError);
    public const string SportEventsLoadError = nameof(SportEventsLoadError);
}