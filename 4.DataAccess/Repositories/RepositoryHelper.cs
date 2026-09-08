using Common.BaseComponents.Components;
using DataAccess.DbContexts;
using Microsoft.EntityFrameworkCore;
using ProblemDomain.Entities.LibraryEntities;
using ProblemDomain.UseCases._Contracts;

namespace DataAccess.Repositories;

/// <summary>
/// Методы, помогающие работать с <see cref="IRepository"/>.
/// </summary>
public class RepositoryHelper : IRepositoryHelper
{
    private readonly IRepository _repository = null!;
    private readonly AppDbContext _dbContext = null!;
    
    /// <summary>
    /// Конструктор, запрещающий создание экземпляра без параметров.
    /// </summary>
    private RepositoryHelper()
    {
    }

    /// <summary>
    /// Конструктор.
    /// </summary>
    public RepositoryHelper(IRepository repository, AppDbContext dbContext) : this()
    {
        _repository =  repository;
        _dbContext = dbContext;
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
                          (await _repository.GetFirstAsync<RefereeCategory>()).Value != null &&
                          (await _repository.GetFirstAsync<RefereeRole>()).Value != null &&
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
    public async Task<Result<bool>> RebuildRepository(bool isUseMigrations = true)
    {
        // Полностью удаляем БД
        await _dbContext.Database.EnsureDeletedAsync();
        
        // Применяем ожидающие миграции
        try
        {
            if (isUseMigrations)
                await _dbContext.Database.MigrateAsync();
            else
            {
                await _dbContext.Database.EnsureCreatedAsync();
            }
            
            return Result<bool>.Done(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Fail(ex);
        }
    }

    /// <inheritdoc />
    public async Task<Result<bool>> FillDatabase()
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
        var resultRefereeCategoryLst = 
            await RepositoryPlaceholder.FillRefereeCategories(_repository);
        if (!resultRefereeCategoryLst.HasValue)
            return Result<bool>.Fail(resultRefereeCategoryLst.Excptn!);
        // ReSharper disable once UnusedVariable
        var refereeCategoryLst = resultRefereeCategoryLst.Value;
        
        // Добавляем судейские должности
        var resultRefereeRoleLst = 
            await RepositoryPlaceholder.FillRefereeRoles(_repository, disciplineGroupLst);
        if (!resultRefereeRoleLst.HasValue)
            return Result<bool>.Fail(resultRefereeRoleLst.Excptn!);
        // ReSharper disable once UnusedVariable
        var refereeRoleLst = resultRefereeRoleLst.Value;
        
        // Добавляем доступности судейских должностей
        var resultRefereeRoleAvailabilityLst = 
            await RepositoryPlaceholder.FillRefereeRoleAvailabilities(_repository);
        if (!resultRefereeRoleAvailabilityLst.HasValue)
            return Result<bool>.Fail(resultRefereeRoleAvailabilityLst.Excptn!);
        // ReSharper disable once UnusedVariable
        var refereeRoleAvailabilityLst = resultRefereeRoleAvailabilityLst.Value;

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

        // Добавляем трудности
        var resultDifficultyLst = 
            await RepositoryPlaceholder.FillDifficulties(_repository);
        if (!resultDifficultyLst.HasValue)
            return Result<bool>.Fail(resultDifficultyLst.Excptn!);
        // ReSharper disable once UnusedVariable
        var difficultyLst = resultDifficultyLst.Value;
            
        // Добавляем возрастные группы
        var resultAgeGroupLst = 
            await RepositoryPlaceholder.FillAgeGroups(_repository);
        if (!resultAgeGroupLst.HasValue)
            return Result<bool>.Fail(resultAgeGroupLst.Excptn!);
        // ReSharper disable once UnusedVariable
        var ageGroupLst = resultAgeGroupLst.Value;

        return Result<bool>.Done(true);
    }
}