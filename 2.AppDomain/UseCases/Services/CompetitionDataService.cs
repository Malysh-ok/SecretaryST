using AppDomain.AppExceptions;
using AppDomain.Phrases;
using AppDomain.UseCases._Contracts;
using Common.BaseComponents.Components;
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
        var competitionsStatusesResult = await repository.GetAllAsync<CompetitionsStatus>();

        return competitionsStatusesResult 
            ? Result<IList<CompetitionsStatus>>.Done(competitionsStatusesResult.Value!) 
            : Result<IList<CompetitionsStatus>>.Fail(new AppException(AppPhrases.CompetitionsStatusesLoadError));
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
            = await repository.GetAllAsync<DetailedCompetitionStatus>(nameof(CompetitionsStatus));
        
        return detailedCompetitionStatuses
            ? Result<IList<DetailedCompetitionStatus>>.Done(detailedCompetitionStatuses.Value!) 
            : Result<IList<DetailedCompetitionStatus>>.Fail(new AppException(AppPhrases.DetailedCompetitionStatusesLoadError));
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
        // Удаляем данные о соревнованиях из репозитория
        var intResult = repository.RemoveAllQuickly<CompetitionData>();
        if (!intResult)
        {
            return Result<CompetitionData>.Fail(
                new AppException(AppPhrases.CompetitionDataCreateError, intResult.Excptn)
            );
        }

        // Создаем данные о соревновании и добавляем в репозиторий
        var competition =
            new CompetitionData(name, conductingOrganizations, initialDate, endDate, venue,
                competitionsStatus, detailedCompetitionStatus,
                description);
        intResult = repository.Add(competition);
        if (!intResult)
        {
            return Result<CompetitionData>.Fail(
                new AppException(AppPhrases.CompetitionDataCreateError, intResult.Excptn)
            );
        }
        
        intResult = await repository.SaveChangesAsync();
        // ReSharper disable once InvertIf
        if (!intResult)
        {
            return Result<CompetitionData>.Fail(
                new AppException(AppPhrases.CompetitionDataCreateError, intResult.Excptn)
            );
        }

        return Result<CompetitionData>.Done(competition);
    }
    
    /// <summary>
    /// Получение данных о соревнованиях.
    /// </summary>
    public async Task<Result<CompetitionData?>> GetCompetitionDataAsync()
    {
        // Сбрасываем отслеживание сущностей
        var result = repository.DetachAll<CompetitionData>();
        if (!result)
            return Result<CompetitionData?>.Fail(new AppException(AppPhrases.CompetitionDataLoadError));

        // Загружаем данные из репозитория
        var competitionDataResult = await repository.GetFirstAsync<CompetitionData>(
            nameof(CompetitionsStatus),
            nameof(DetailedCompetitionStatus));

        return competitionDataResult
            ? Result<CompetitionData?>.Done(competitionDataResult.Value)
            : Result<CompetitionData?>.Fail(new AppException(AppPhrases.CompetitionDataLoadError));
    }
    
    /// <summary>
    /// Сохранение данных о соревнованиях.
    /// </summary>
    /// <param name="competitionData">Данные о соревнования.</param>
    public async Task<Result<int>> SaveCompetitionDataAsync(CompetitionData competitionData)
    {
        var intResult1 = repository.Update(competitionData);
        var intResult2 = await repository.SaveChangesAsync();
        return intResult1 && intResult2 
            ? intResult2
            : Result<int>.Fail(new AppException(AppPhrases.CompetitionDataSaveError, 
                !intResult1 ? intResult1.Excptn : intResult2.Excptn));
    }
}