using AppDomain.AppExceptions;
using AppDomain.Phrases;
using AppDomain.UseCases._Contracts;
using Common.BaseComponents.Components;
using Common.BaseExtensions.Collections;
using Common.BaseExtensions.ValueTypes;
using ProblemDomain.Entities.CommonEntities;
using ProblemDomain.Entities.LibraryEntities;
using ProblemDomain.Entities.LibraryEntities.Enums;

namespace AppDomain.UseCases.Services;

/// <summary>
/// Сервис для работы с Судьями.
/// </summary>
/// <param name="repository">Репозиторий.</param>
public class RefereeService(IRepository repository)
{
    /// <summary>
    /// Получение судейских категорий.
    /// </summary>
    public async Task<Result<IList<RefereeLevel>>> GetRefereeLevelsAsync()
    {
        var refereeLevelsResult = 
            (await repository.GetAllAsync<RefereeLevel>()).AsList();

        return refereeLevelsResult is null 
            ? Result<IList<RefereeLevel>>.Fail(new AppException(AppPhrases.RefereeLevelsLoadError)) 
            : Result<IList<RefereeLevel>>.Done(refereeLevelsResult);
    }
    
    /// <summary>
    /// Получение судейских должностей.
    /// </summary>
    public async Task<Result<IList<RefereeJobTitle>>> GetRefereeJobTitlesAsync()
    {
        var refereeJobTitlesResult = 
            (await repository.GetAllAsync<RefereeJobTitle>()).AsList();

        return refereeJobTitlesResult is null 
            ? Result<IList<RefereeJobTitle>>.Fail(new AppException(AppPhrases.RefereeJobTitlesLoadError)) 
            : Result<IList<RefereeJobTitle>>.Done(refereeJobTitlesResult);
    }
    
    /// <summary>
    /// Получение списка судей.
    /// </summary>
    /// <param name="referees">Текущий список судей, если не пуст - то обновляем значения.</param>
    public async Task<Result<IList<Referee>>> GetRefereesAsync(IList<Referee> referees)
    {
        // Загружаем данные из репозитория
        var readReferees = (await repository.GetNumberedAllAsync<Referee>(
            nameof(RefereeLevel), nameof(RefereeJobTitle))).AsList();

        return readReferees is null
            ? Result<IList<Referee>>.Fail(new AppException(AppPhrases.RefereesLoadError))
            : Result<IList<Referee>>.Done(readReferees);
    }
    
    /// <summary>
    /// Добавление судьи.
    /// </summary>
    public async Task<Result<IList<Referee>>> AddRefereeAsync(IList<Referee> referees, int index)
    {
        if (index >= referees.Count || index < 0)
            // return Result<IList<Referee>>.Done(referees);
            index = 0;

        var refereeLevel = await repository.FindAsync<RefereeLevel>(RefereeLevelEnm.Category3);
        var refereeJobTitle = await repository.FindAsync<RefereeJobTitle>(RefereeJobTitleEnm.StageReferee);

        var newReferee = new Referee(index + 2,
            "ФАМИЛИЯ",
            "ИМЯ",
            "ТЕРРИТОРИЯ",
            refereeLevel!,
            refereeJobTitle!);
        
        List<Referee> newReferees = [..referees];
        newReferees.Insert(index + 1, newReferee);
        for (var i = index + 2; i < newReferees.Count; i++)
        {
            newReferees[i].Number = i + 1;
        }
            
        return Result<IList<Referee>>.Done(newReferees);
    }
    
    /// <summary>
    /// Удаление судьи.
    /// </summary>
    public async Task<Result<IList<Referee>>> RemoveRefereeAsync(IList<Referee> referees, int index)
    {
        if (index >= referees.Count || index < 0)
            return Result<IList<Referee>>.Done(referees);

        var removeResult = await repository.RemoveAsync(referees[index]);
        if (removeResult == false)
            Result<IList<Referee>>.Fail(removeResult.Excptn);
        
        List<Referee> newReferees = [..referees];
        newReferees.RemoveAt(index);
        for (var i = index + 2; i < newReferees.Count; i++)
        {
            newReferees[i].Number = i + 1;
        }
            
        return Result<IList<Referee>>.Done(newReferees);
    }
    
    /// <summary>
    /// Сохранение списка судей.
    /// </summary>
    /// <param name="referees">Список судей.</param>
    public async Task<Result<int>> SaveRefereesAsync(IEnumerable<Referee> referees)
        => await repository.AddOrUpdateRangeAsync(referees);
}