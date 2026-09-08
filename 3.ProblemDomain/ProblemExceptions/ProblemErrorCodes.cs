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
    public const string CompetitionCreateError = nameof(CompetitionCreateError);
    public const string CompetitionIsNull = nameof(CompetitionIsNull);
    public const string CompetitionListLoadError = nameof(CompetitionListLoadError);
    public const string CompetitionLoadError = nameof(CompetitionLoadError);
    public const string CompetitionNotFound = nameof(CompetitionNotFound);
    public const string CompetitionRemoveError = nameof(CompetitionRemoveError);
    public const string CompetitionSaveError = nameof(CompetitionSaveError);
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
    public const string RefereeRoleFindError = nameof(RefereeRoleFindError);
    public const string RefereeRolesLoadError = nameof(RefereeRolesLoadError);
    public const string RefereeCategoryFindError = nameof(RefereeCategoryFindError);
    public const string RefereeCategoriesLoadError = nameof(RefereeCategoriesLoadError);
    public const string RefereeRemoveError = nameof(RefereeRemoveError);
    public const string RefereesLoadError = nameof(RefereesLoadError);
    public const string RefereeAvailableRolesCalculateError = nameof(RefereeAvailableRolesCalculateError);
    
    public const string SportEventCreateError = nameof(SportEventCreateError);
    public const string SportEventRemoveError = nameof(SportEventRemoveError);
    public const string SportEventsLoadError = nameof(SportEventsLoadError);
    
    public const string AgeGroupLoadError = nameof(AgeGroupLoadError);
    public const string AgeGroupsLoadError = nameof(AgeGroupsLoadError);
}