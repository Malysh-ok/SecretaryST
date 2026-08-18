using Common.BaseComponents.Components;
using Common.BaseExtensions.Collections;
using ProblemDomain.Entities.CommonEntities;
using ProblemDomain.Entities.LibraryEntities;
using ProblemDomain.Entities.LibraryEntities.Enums;
using ProblemDomain.ProblemExceptions;
using ProblemDomain.UseCases._Contracts;

namespace ProblemDomain.UseCases.Services;

/// <summary>
/// Сервис для работы с соревнованиями (данными о соревновании).
/// </summary>
/// <param name="repository">Репозиторий.</param>
/// <param name="problemErrorMsgProvider">Провайдер сообщений об ошибках слоя предметной области.</param>
public class CompetitionDataService(IRepository repository, IProblemErrorMsgProvider problemErrorMsgProvider)
{
    /// <summary>
    /// Получение проводящих организаций, связанных с соревнованием.
    /// </summary>
    /// <param name="competition">Текущее соревнование.</param>
    public Result<IList<string>> GetConductingOrganizations(CompetitionData? competition)
    {
        // Проверяем наличие соревнования
        if (competition == null)
        {
            return Result<IList<string>>.Fail(
                problemErrorMsgProvider.CreateException(ProblemErrorCodes.ConductingOrganizationsLoadError,
                    problemErrorMsgProvider.CreateException(ProblemErrorCodes.CompetitionDataIsNull))
            );
        }

        return Result<IList<string>>.Done(competition.ConductingOrganizations);
    }
    
    /// <summary>
    /// Создание проводящей организации.
    /// </summary>
    public string CreateConductingOrganization()
    {
        return "НОВАЯ ПРОВОДЯЩАЯ ОРГАНИЗАЦИЯ";
    }

    /// <summary>
    /// Установка проводящих организаций в связанном с ними соревновании.
    /// </summary>
    /// <param name="competition">Текущее соревнование.</param>
    /// <param name="conductingOrganizations">Коллекция проводящих организаций (источник данных).</param>
    public Result<bool> SetConductingOrganizations(CompetitionData? competition, IList<string> conductingOrganizations)
    {
        // Проверяем наличие соревнования
        if (competition == null)
        {
            return Result<bool>.Fail(
                problemErrorMsgProvider.CreateException(ProblemErrorCodes.ConductingOrganizationsSetError,
                    problemErrorMsgProvider.CreateException(ProblemErrorCodes.CompetitionDataIsNull))
            );
        }

        competition.ConductingOrganizations = conductingOrganizations;
        return Result<bool>.Done(true);
    }

    /// <summary>
    /// Получение списка статусов соревнований.
    /// </summary>
    public async Task<Result<IList<CompetitionsStatus>>> GetCompetitionsStatusesAsync()
    {
        var competitionsStatusesResult = await repository.GetAllAsync<CompetitionsStatus>();

        return competitionsStatusesResult 
            ? Result<IList<CompetitionsStatus>>.Done(competitionsStatusesResult.Value!)
            : Result<IList<CompetitionsStatus>>.Fail(
                problemErrorMsgProvider.CreateException(
                ProblemErrorCodes.CompetitionStatusesLoadError, competitionsStatusesResult.Excptn)
            );
        
    }

    /// <summary>
    /// Получение списка статусов и обобщенных наименований соревнований.
    /// </summary>
    /// <remarks>
    /// Включая навигационное свойство <see cref="CompetitionsStatus"/>.
    /// </remarks>
    public async Task<Result<IList<DetailedCompetitionStatus>>> GetDetailedCompetitionsStatusesAsync()
    {
        var detailedCompetitionStatuses
            = await repository.GetAllAsync<DetailedCompetitionStatus>(navigationProperties: nameof(CompetitionsStatus));
        
        return detailedCompetitionStatuses
            ? Result<IList<DetailedCompetitionStatus>>.Done(detailedCompetitionStatuses.Value!)
            : Result<IList<DetailedCompetitionStatus>>.Fail(
                problemErrorMsgProvider.CreateException(
                    ProblemErrorCodes.DetailedCompetitionStatusesLoadError, detailedCompetitionStatuses.Excptn)
            );
    }
    
    /// <summary>
    /// Получение всех соревнований и обновление ими коллекции.
    /// </summary>
    /// <param name="competitionCollection">Обновляемая коллекция соревнований.</param>
    /// <remarks>
    /// Без навигационных свойств ("легкий" список).
    /// </remarks>
    public async Task<Result<int>> GetAllCompetitionsDataAsync(IList<CompetitionData> competitionCollection)
    {
        // Сбрасываем отслеживание сущностей
        var result = repository.DetachAll<CompetitionData>();
        if (! result)
            return Result<int>.Fail(
                problemErrorMsgProvider.CreateException(
                ProblemErrorCodes.CompetitionDataListLoadError, result.Excptn)
            );
        
        // Загружаем данные из репозитория
        var competitionsDataResult = await repository.GetAllAsync<CompetitionData>();
        if (! competitionsDataResult)
            return Result<int>.Fail(
                problemErrorMsgProvider.CreateException(
                ProblemErrorCodes.CompetitionDataListLoadError, competitionsDataResult.Excptn)
            );

        // Перезаписываем коллекцию соревнований новыми данными
        competitionCollection.Clear();
        competitionsDataResult.Value.ForEach(competitionCollection.Add);

        return Result<int>.Done(competitionsDataResult.Value!.Count);
    }

    /// <summary>
    /// Получение соревнования.
    /// </summary>
    /// <param name="competitionId">Id получаемого соревнования.</param>
    /// <param name="isCheckForNull">Флаг проверки результата:
    /// если при установленном флаге результат равен Null - выбрасывается исключение.</param>
    /// <remarks>
    /// С навигационными свойствами ("тяжелый" объект).
    /// </remarks>
    public async Task<Result<CompetitionData?>> GetCompetitionDataAsync(int competitionId, bool isCheckForNull = false)
    {
        // Сбрасываем отслеживание сущностей
        var result = repository.DetachAll<CompetitionData>();
        if (! result)
            return Result<CompetitionData?>.Fail(
                problemErrorMsgProvider.CreateException(
                ProblemErrorCodes.CompetitionDataLoadError, result.Excptn)
                );

        // Загружаем данные из репозитория
        var competitionResult = await repository.GetByIdAsync<CompetitionData>(competitionId,
            nameof(CompetitionsStatus),
            nameof(DetailedCompetitionStatus));
        if (! competitionResult)
            return Result<CompetitionData?>.Fail(
                problemErrorMsgProvider.CreateException(
                ProblemErrorCodes.CompetitionDataLoadError, competitionResult.Excptn)
            );
        
        if (isCheckForNull && competitionResult.Value == null)
            return Result<CompetitionData?>.Fail(
                problemErrorMsgProvider.CreateException(ProblemErrorCodes.CompetitionDataLoadError,
                    problemErrorMsgProvider.CreateException(ProblemErrorCodes.CompetitionDataNotFound))
            );

        return Result<CompetitionData?>.Done(competitionResult.Value);
    }

    /// <summary>
    /// Создание соревнования - экземпляра <see cref="CompetitionData"/> (фабричный метод).
    /// </summary>
    /// <remarks>
    /// Создаем с сохранением изменений в репозитории.
    /// </remarks>
    /// <param name="name">Наименование.</param>
    /// <param name="conductingOrganizations">Проводящие организации.</param>
    /// <param name="initialDate">Начальная дата проведения.</param>
    /// <param name="endDate">Конечная дата проведения.</param>
    /// <param name="venue">Место проведения.</param>
    /// <param name="shortName">Краткое название соревнований.</param>
    /// <param name="competitionsStatus">Статус соревнования.</param>
    /// <param name="detailedCompetitionStatus">Статус и обобщенное наименование соревнования.</param>
    /// <param name="isStudentCompetition">Признак того, что соревнования студенческие.</param>
    /// <param name="description">Описание.</param>
    public async Task<Result<CompetitionData>> CreateCompetitionDataAsync(string name, 
        IList<string> conductingOrganizations, DateTime initialDate, DateTime endDate, string venue, string shortName,
        CompetitionsStatus competitionsStatus, DetailedCompetitionStatus detailedCompetitionStatus,
        bool isStudentCompetition = false,
        string? description = null)
    {
        // Создаем данные о соревновании и добавляем в репозиторий
        var competition =
            new CompetitionData(name, conductingOrganizations, initialDate, endDate, venue, shortName,
                competitionsStatus, detailedCompetitionStatus,
                isStudentCompetition, description);
        var intResult = repository.Add(competition);
        if (! intResult)
        {
            return Result<CompetitionData>.Fail(
                problemErrorMsgProvider.CreateException(
                    ProblemErrorCodes.CompetitionDataCreateError, intResult.Excptn)
            );
        }
        
        // Сохраняем изменения в репозитории
        intResult = await repository.SaveChangesAsync();
        if (! intResult)
        {
            return Result<CompetitionData>.Fail(
                problemErrorMsgProvider.CreateException(
                    ProblemErrorCodes.CompetitionDataCreateError, intResult.Excptn)
            );
        }

        return Result<CompetitionData>.Done(competition);
    }

    /// <summary>
    /// Создание нового нового соревнования и добавление в коллекцию после текущего.
    /// </summary>
    /// <remarks>
    /// Создаем с сохранением изменений в репозитории.
    /// </remarks>
    /// <param name="competitionCollection">Обновляемая коллекция соревнований.</param>
    /// <returns>Индекс нового соревнования.</returns>
    public async Task<Result<CompetitionData>> CreateCompetitionDataAsync(IList<CompetitionData> competitionCollection)
    {
        ProblemException innerException;
        
        // Получаем статус соревнования
        var competitionStatusResult = await repository.FindAsync<CompetitionsStatus>(
            CompetitionsStatusEnm.Regional);
        if (! competitionStatusResult)
        {
            innerException = problemErrorMsgProvider.CreateException(
                ProblemErrorCodes.CompetitionStatusFindError, competitionStatusResult.Excptn);

            return Result<CompetitionData>.Fail(
                problemErrorMsgProvider.CreateException(
                    ProblemErrorCodes.CompetitionDataCreateError, innerException)
            );
        }
        
        // Получаем статус и обобщенное наименование соревнования
        var detailedCompetitionStatusResult = await repository.FindAsync<DetailedCompetitionStatus>(
            DetailedCompetitionStatusEnm.RegionalJuniorChampionship);
        if (! detailedCompetitionStatusResult)
        {
            innerException = problemErrorMsgProvider.CreateException(
                ProblemErrorCodes.DetailedCompetitionStatusFindError, detailedCompetitionStatusResult.Excptn);
            
            return Result<CompetitionData>.Fail(
                problemErrorMsgProvider.CreateException(
                    ProblemErrorCodes.CompetitionDataCreateError, innerException)
            );
        }
        
        // Создаем данные о соревновании
        var newCompetitionResult = await CreateCompetitionDataAsync(
            "НАЗВАНИЕ СОРЕВНОВАНИЙ",
            ["ПРОВОДЯЩАЯ ОРГАНИЗАЦИЯ"],
            DateTime.Now, DateTime.Now,
            "МЕСТО ПРОВЕДЕНИЯ", "СОРЕВЫ",
            competitionStatusResult.Value!,
            detailedCompetitionStatusResult.Value!
        );

        return newCompetitionResult
            ? Result<CompetitionData>.Done(newCompetitionResult.Value!)
            : Result<CompetitionData>.Fail(
                problemErrorMsgProvider.CreateException(
                    ProblemErrorCodes.CompetitionDataCreateError, newCompetitionResult.Excptn)
            );
    }

    /// <summary>
    /// Удаление соревнования из коллекции.
    /// </summary>
    /// <param name="competitionCollection">Обновляемая коллекция соревнований.</param>
    /// <param name="competition">Удаляемое соревнование.</param>
    /// <returns>Новое текущее соревнование.</returns>
    public Task<Result<CompetitionData>> RemoveCompetitionDataAsync(
        IList<CompetitionData> competitionCollection, CompetitionData? competition)
    {
        if (competition == null)
            return Task.FromResult(Result<CompetitionData>.Fail(
                problemErrorMsgProvider.CreateException(ProblemErrorCodes.CompetitionDataRemoveError,
                problemErrorMsgProvider.CreateException(ProblemErrorCodes.CompetitionDataIsNull))
            ));
        
        // Удаляем из коллекции соревнование
        var index = competitionCollection.IndexOf(competition);
        competitionCollection.Remove(competition);
        if (index >= competitionCollection.Count)
            index = competitionCollection.Count - 1;
        else if (index < 0)
            index = 0;
        
        // Удаляем из репозитория
        var intResult = repository.Remove(competition);
        if (! intResult)
        {
            return Task.FromResult(Result<CompetitionData>.Fail(
                problemErrorMsgProvider.CreateException(
                    ProblemErrorCodes.CompetitionDataRemoveError, intResult.Excptn)
            ));
        }

        return Task.FromResult(Result<CompetitionData>.Done(competitionCollection[index]));
    }

    /// <summary>
    /// Сохранение соревнования (включая зависимые сущности).
    /// </summary>
    public async Task<Result<int>> SaveCompetitionDataAsync()
    {
        var intResult = await repository.SaveChangesAsync();
        return intResult
            ? intResult
            : Result<int>.Fail(
                problemErrorMsgProvider.CreateException(
                    ProblemErrorCodes.CompetitionDataSaveError, intResult.Excptn)
            );
    }
}