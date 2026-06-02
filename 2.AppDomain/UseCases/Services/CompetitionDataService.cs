using AppDomain.AppExceptions;
using AppDomain.Phrases;
using AppDomain.UseCases._Contracts;
using Common.BaseComponents.Components;
using Common.BaseExtensions.Collections;
using ProblemDomain.Entities.CommonEntities;
using ProblemDomain.Entities.LibraryEntities;

namespace AppDomain.UseCases.Services;

/// <summary>
/// Сервис для работы с Данными о соревнованиях.
/// </summary>
/// <param name="repository">Репозиторий.</param>
public class CompetitionDataService(IRepository repository)
{
    /// <summary>
    /// Получение статусов соревнований.
    /// </summary>
    public async Task<Result<IList<CompetitionsStatus>>> GetCompetitionsStatusesAsync()
    {
        var competitionsStatuses = 
            (await repository.GetAllAsync<CompetitionsStatus>()).AsList();

        return competitionsStatuses is null 
            ? Result<IList<CompetitionsStatus>>.Fail(new AppException(AppPhrases.CompetitionsStatusesLoadError)) 
            : Result<IList<CompetitionsStatus>>.Done(competitionsStatuses);
    }

    /// <summary>
    /// Получение статусов и обобщенных наименований соревнований.
    /// </summary>
    /// <remarks>
    /// Включая навигационное свойство <see cref="CompetitionsStatus"/>.
    /// </remarks>
    public async Task<Result<IList<DetailedCompetitionStatus>>> GetDetailedCompetitionsStatusesAsync()
    {
        var detailedCompetitionStatuses 
            = (await repository.GetAllAsync<DetailedCompetitionStatus>(nameof(CompetitionsStatus))).AsList();
        
        return detailedCompetitionStatuses is null 
            ? Result<IList<DetailedCompetitionStatus>>.Fail(new AppException(AppPhrases.DetailedCompetitionStatusesLoadError)) 
            : Result<IList<DetailedCompetitionStatus>>.Done(detailedCompetitionStatuses);
    }

    /// <summary>
    /// Создание экземпляра <see cref="CompetitionData"/> (фабричный метод).
    /// </summary>
    /// <param name="name">Наименование.</param>
    /// <param name="conductingOrganizations">Проводящие организации.</param>
    /// <param name="initialDate">Начальная дата проведения.</param>
    /// <param name="endDate">Конечная дата проведения.</param>
    /// <param name="venue">Место проведения.</param>
    /// <param name="competitionsStatus">Статус соревнования.</param>
    /// <param name="detailedCompetitionStatus">Статус и обобщенное наименование соревнования.</param>
    /// <param name="description">Описание.</param>
    public async Task<Result<CompetitionData>> CreateAsync(string name, 
        string conductingOrganizations, DateTime initialDate, DateTime endDate, string venue,
        CompetitionsStatus competitionsStatus, DetailedCompetitionStatus detailedCompetitionStatus,
        string? description = null)
    {
        AppException innerException;
        
        // Удаляем данные о соревновании из репозитория
        var intResult = await repository.RemoveRangeAsync(await repository.GetAllAsync<CompetitionData>());

        if (! intResult)
        {
            innerException = new AppException(AppPhrases.CompetitionDataRemoveError, intResult.Excptn);
            return Result<CompetitionData>.Fail(
                new AppException(AppPhrases.CompetitionDataCreateError, innerException)
            );
        }

        // Создаем данные о соревновании и добавляем в репозиторий
        var competition =
            new CompetitionData(name, conductingOrganizations, initialDate, endDate, venue,
                competitionsStatus, detailedCompetitionStatus,
                description);
        intResult = await repository.AddAsync(competition);

        // ReSharper disable once InvertIf
        if (! intResult)
        {
            innerException = new AppException(AppPhrases.CompetitionDataSaveError, intResult.Excptn);
            return Result<CompetitionData>.Fail(
                new AppException(AppPhrases.CompetitionDataCreateError, innerException)
            );
        }
        
        return Result<CompetitionData>.Done(competition);
    }
    
    /// <summary>
    /// Получение данных о соревнованиях.
    /// </summary>
    /// <param name="competitionData">Текущие данные о соревнованиях, если не null - то обновляем значение.</param>
    public async Task<Result<CompetitionData>> GetCompetitionDataAsync(CompetitionData? competitionData)
    {
        // Загружаем данные из репозитория
        var readCompetitionData = await repository.GetFirstAsync<CompetitionData>(
            nameof(CompetitionsStatus),
            nameof(DetailedCompetitionStatus));

        return readCompetitionData is null
            ? Result<CompetitionData>.Fail(new AppException(AppPhrases.CompetitionDataLoadError))
            : Result<CompetitionData>.Done(readCompetitionData);
    }
    
    /// <summary>
    /// Сохранение данных о соревнованиях.
    /// </summary>
    /// <param name="competitionData">Данные о соревнования.</param>
    public async Task<Result<int>> SaveCompetitionDataAsync(CompetitionData competitionData)
    {
        var intResult = await repository.AddOrUpdateAsync(competitionData);

        return intResult
            ? intResult
            : Result<int>.Fail(new AppException(AppPhrases.CompetitionDataSaveError, intResult.Excptn));
    }
}