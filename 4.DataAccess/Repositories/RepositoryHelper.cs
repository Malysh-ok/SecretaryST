using AppDomain.UseCases._Contracts;
using Common.BaseComponents.Components;
using Microsoft.EntityFrameworkCore;
using ProblemDomain.Entities.LibraryEntities;

namespace DataAccess.Repositories;

/// <summary>
/// Методы, помогающие работать с <see cref="IRepository"/>.
/// </summary>
public class RepositoryHelper : IRepositoryHelper
{
    private readonly IRepository _repository = null!;
    
    /// <summary>
    /// Конструктор, запрещающий создание экземпляра без параметров.
    /// </summary>
    private RepositoryHelper()
    {
    }

    /// <summary>
    /// Конструктор.
    /// </summary>
    public RepositoryHelper(IRepository repository) : this()
    {
        _repository =  repository;
    }

    /// <inheritdoc />
    public async Task<bool> IsExistLibrary()
    {
        try
        {
            var isExist = (await _repository.GetFirstAsync<CompetitionsStatus>()).Value != null &&
                          (await _repository.GetFirstAsync<DetailedCompetitionStatus>()).Value != null &&
                          (await _repository.GetFirstAsync<DisciplineGroup>()).Value != null &&
                          (await _repository.GetFirstAsync<DisciplineSubGroup>()).Value != null &&
                          (await _repository.GetFirstAsync<Discipline>()).Value != null &&
                          (await _repository.GetFirstAsync<RefereeLevel>()).Value != null &&
                          (await _repository.GetFirstAsync<RefereeJobTitle>()).Value != null &&
                          (await _repository.GetFirstAsync<Sex>()).Value != null &&
                          (await _repository.GetFirstAsync<SportUnitType>()).Value != null;

            return isExist;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<Result<bool>> RebuildRepository()
    {
        // Полностью удаляем БД
        await _repository.GetDbContext().Database.EnsureDeletedAsync();
        
        // Применяем ожидающие миграции
        await _repository.GetDbContext().Database.MigrateAsync();
        
        // Наполняем БД библиотечными данными
        return await FillDatabase();
    }

    /// <summary>
    /// Заполняем БД библиотечными данными.
    /// </summary>
    private async Task<Result<bool>> FillDatabase()
    {
        // Добавляем статусы соревнований
        var resultCompetitionsStatusLst = 
            await RepositoryPlaceholder.FillCompetitionsStatuses(_repository);
        if (!resultCompetitionsStatusLst.HasValue)
            return Result<bool>.Fail(resultCompetitionsStatusLst.Excptn!);
        var competitionsStatusLst = resultCompetitionsStatusLst.Value!;
        
        // Добавляем статусы и наименования спортивных соревнований
        var resultDetailedCompetitionStatusLst = 
            await RepositoryPlaceholder.FillDetailedCompetitionStatuses(_repository, competitionsStatusLst);
        if (!resultDetailedCompetitionStatusLst.HasValue)
            return Result<bool>.Fail(resultDetailedCompetitionStatusLst.Excptn!);
        // ReSharper disable once UnusedVariable
        var detailedCompetitionStatusLst = resultDetailedCompetitionStatusLst.Value!;
        
        // Добавляем группы дисциплин
        var resultDisciplineGroupLst = 
            await RepositoryPlaceholder.FillDisciplineGroups(_repository);
        if (!resultDisciplineGroupLst.HasValue)
            return Result<bool>.Fail(resultDisciplineGroupLst.Excptn!);
        var disciplineGroupLst = resultDisciplineGroupLst.Value!;

        // Добавляем подгруппы дисциплин
        var resultDisciplineSubGroupLst = 
            await RepositoryPlaceholder.FillDisciplineSubGroups(_repository, disciplineGroupLst);
        if (!resultDisciplineSubGroupLst.HasValue)
            return Result<bool>.Fail(resultDisciplineSubGroupLst.Excptn!);
        var disciplineSubGroupLst = resultDisciplineSubGroupLst.Value!;

        // Добавляем дисциплины
        var resultDisciplineLst = 
            await RepositoryPlaceholder.FillDisciplines(_repository, disciplineGroupLst, disciplineSubGroupLst);
        if (!resultDisciplineLst.HasValue)
            return Result<bool>.Fail(resultDisciplineLst.Excptn!);
        // ReSharper disable once UnusedVariable
        var disciplineLst = resultDisciplineLst.Value;
        
        // Добавляем судейские категории
        var resultRefereeLevelLst = 
            await RepositoryPlaceholder.FillRefereeLevels(_repository);
        if (!resultRefereeLevelLst.HasValue)
            return Result<bool>.Fail(resultRefereeLevelLst.Excptn!);
        // ReSharper disable once UnusedVariable
        var refereeLevels = resultRefereeLevelLst.Value;
        
        // Добавляем судейские должности
        var resultFillRefereeJobTitleLst = 
            await RepositoryPlaceholder.FillRefereeJobTitles(_repository);
        if (!resultFillRefereeJobTitleLst.HasValue)
            return Result<bool>.Fail(resultFillRefereeJobTitleLst.Excptn!);
        // ReSharper disable once UnusedVariable
        var refereeingPositionLst = resultFillRefereeJobTitleLst.Value;

        // Добавляем варианты пола
        var resultSexLst = 
            await RepositoryPlaceholder.FillSexes(_repository);
        if (!resultSexLst.HasValue)
            return Result<bool>.Fail(resultSexLst.Excptn!);
        // ReSharper disable once UnusedVariable
        var sexLst = resultSexLst.Value;
        
        // Добавляем типы спортивных юнитов
        var resultSportUnitTypeLst = 
            await RepositoryPlaceholder.FillSportUnitType(_repository);
        if (!resultSportUnitTypeLst.HasValue)
            return Result<bool>.Fail(resultSportUnitTypeLst.Excptn!);
        // ReSharper disable once UnusedVariable
        var sportUnitTypeLst = resultSportUnitTypeLst.Value;

        return Result<bool>.Done(true);
    }
}