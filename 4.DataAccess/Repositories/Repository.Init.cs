using AppDomain.UseCases._Contracts;
using Common.BaseComponents.Components;
using Microsoft.EntityFrameworkCore;
using ProblemDomain.Entities.LibraryEntities;

namespace DataAccess.Repositories;

public partial class Repository<TDbContext> : IInitRepository
{
    #region [---------- НЕ публичные члены ----------]

    

    #endregion

    /// <inheritdoc />
    public async Task<bool> IsExistLibrary()
    {
        var isExist = await GetFirstAsync<CompetitionsStatus>() != null &&
                      await GetFirstAsync<DetailedCompetitionStatus>() != null &&
                      await GetFirstAsync<DisciplineGroup>() != null &&
                      await GetFirstAsync<DisciplineSubGroup>() != null &&
                      await GetFirstAsync<Discipline>() != null &&
                      await GetFirstAsync<RefereeLevel>() != null &&
                      await GetFirstAsync<RefereeJobTitle>() != null &&
                      await GetFirstAsync<Sex>() != null &&
                      await GetFirstAsync<SportUnitType>() != null;

        return isExist;
    }

    /// <inheritdoc />
    public async Task RebuildRepository()
    {
        await DbContext.Database.EnsureDeletedAsync();
        await DbContext.Database.MigrateAsync();
        await FillDatabase();
    }

    /// <summary>
    /// Заполняем БД.
    /// </summary>
    public async Task<Result<bool>> FillDatabase()
    {
        // Добавляем статусы соревнований
        var resultCompetitionsStatusLst = await RepositoryPlaceholder.FillCompetitionsStatuses(this);
        if (!resultCompetitionsStatusLst)
            return Result<bool>.Fail(resultCompetitionsStatusLst.Excptn);
        var competitionsStatusLst = resultCompetitionsStatusLst.Value;
        
        // Добавляем статусы и наименования спортивных соревнований
        var resultDetailedCompetitionStatusLst = await RepositoryPlaceholder.FillDetailedCompetitionStatuses(this, competitionsStatusLst);
        if (!resultDetailedCompetitionStatusLst)
            return Result<bool>.Fail(resultDetailedCompetitionStatusLst.Excptn);
        var detailedCompetitionStatusLst = resultDetailedCompetitionStatusLst.Value;
        
        // Добавляем группы дисциплин
        var resultDisciplineGroupLst = await RepositoryPlaceholder.FillDisciplineGroups(this);
        if (!resultDisciplineGroupLst)
            return Result<bool>.Fail(resultDisciplineGroupLst.Excptn);
        var disciplineGroupLst = resultDisciplineGroupLst.Value;

        // Добавляем подгруппы дисциплин
        var resultDisciplineSubGroupLst = await RepositoryPlaceholder.FillDisciplineSubGroups(this, disciplineGroupLst);
        if (!resultDisciplineSubGroupLst)
            return Result<bool>.Fail(resultDisciplineSubGroupLst.Excptn);
        var disciplineSubGroupLst = resultDisciplineSubGroupLst.Value;

        // Добавляем дисциплины
        var resultDisciplineLst = await RepositoryPlaceholder.FillDisciplines(this, disciplineGroupLst, disciplineSubGroupLst);
        if (!resultDisciplineLst)
            return Result<bool>.Fail(resultDisciplineLst.Excptn);
        var disciplineLst = resultDisciplineLst.Value;
        
        // Добавляем судейские категории
        var resultRefereeLevelLst = await RepositoryPlaceholder.FillRefereeLevels(this);
        if (!resultRefereeLevelLst)
            return Result<bool>.Fail(resultRefereeLevelLst.Excptn);
        var refereeLevels = resultRefereeLevelLst.Value;
        
        // Добавляем судейские должности
        var resultFillRefereeJobTitleLst = await RepositoryPlaceholder.FillRefereeJobTitles(this);
        if (!resultFillRefereeJobTitleLst)
            return Result<bool>.Fail(resultFillRefereeJobTitleLst.Excptn);
        var refereeingPositionLst = resultFillRefereeJobTitleLst.Value;

        // Добавляем варианты пола
        var resultSexLst = await RepositoryPlaceholder.FillSexes(this);
        if (!resultSexLst)
            return Result<bool>.Fail(resultSexLst.Excptn);
        var sexLst = resultSexLst.Value;
        
        // Добавляем типы спортивных юнитов
        var resultSportUnitTypeLst = await RepositoryPlaceholder.FillSportUnitType(this);
        if (!resultSportUnitTypeLst)
            return Result<bool>.Fail(resultSportUnitTypeLst.Excptn);
        var sportUnitTypeLst = resultSportUnitTypeLst.Value;

        return Result<bool>.Done(true);
    }
}