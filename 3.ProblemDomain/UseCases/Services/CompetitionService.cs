using Common.BaseComponents.Components;
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
public class CompetitionService(IRepository repository, IProblemErrorMsgProvider problemErrorMsgProvider)
{
    /// <summary>
    /// Получение проводящих организаций, связанных с соревнованием.
    /// </summary>
    /// <param name="competition">Текущее соревнование.</param>
    public Result<IList<string>> GetConductingOrganizations(Competition? competition)
    {
        // Проверяем наличие соревнования
        if (competition == null)
        {
            return Result<IList<string>>.Fail(
                problemErrorMsgProvider.CreateException(ProblemErrorCodes.ConductingOrganizationsLoadError,
                    problemErrorMsgProvider.CreateException(ProblemErrorCodes.CompetitionIsNull))
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
    public Result<bool> SetConductingOrganizations(Competition? competition, IList<string> conductingOrganizations)
    {
        // Проверяем наличие соревнования
        if (competition == null)
        {
            return Result<bool>.Fail(
                problemErrorMsgProvider.CreateException(ProblemErrorCodes.ConductingOrganizationsSetError,
                    problemErrorMsgProvider.CreateException(ProblemErrorCodes.CompetitionIsNull))
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
    /// Получение коллекции всех соревнований.
    /// </summary>
    /// <remarks>
    /// Без навигационных свойств ("легкий" список).
    /// </remarks>
    public async Task<Result<IList<Competition>>> GetAllCompetitionsAsync()
    {
        // Сбрасываем отслеживание сущностей
        var result = repository.DetachAll<Competition>();
        if (! result)
            return Result<IList<Competition>>.Fail(
                problemErrorMsgProvider.CreateException(
                ProblemErrorCodes.CompetitionListLoadError, result.Excptn)
            );
        
        // Загружаем данные из репозитория
        var competitionsResult = await repository.GetAllAsync<Competition>();
        if (! competitionsResult)
            return Result<IList<Competition>>.Fail(
                problemErrorMsgProvider.CreateException(
                ProblemErrorCodes.CompetitionListLoadError, competitionsResult.Excptn)
            );

        return Result<IList<Competition>>.Done(competitionsResult.Value!);
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
    public async Task<Result<Competition?>> GetCompetitionAsync(int competitionId, bool isCheckForNull = false)
    {
        // Сбрасываем отслеживание сущности
        var result = repository.Detach<Competition>(competitionId);
        if (! result)
            return Result<Competition?>.Fail(
                problemErrorMsgProvider.CreateException(
                    ProblemErrorCodes.CompetitionLoadError, result.Excptn)
            );
        
        // Загружаем данные из репозитория
        var competitionResult = await repository.GetByIdAsync<Competition>(competitionId,
            nameof(CompetitionsStatus),
            nameof(DetailedCompetitionStatus));
        if (! competitionResult)
            return Result<Competition?>.Fail(
                problemErrorMsgProvider.CreateException(
                ProblemErrorCodes.CompetitionLoadError, competitionResult.Excptn)
            );
        
        if (isCheckForNull && competitionResult.Value == null)
            return Result<Competition?>.Fail(
                problemErrorMsgProvider.CreateException(ProblemErrorCodes.CompetitionLoadError,
                    problemErrorMsgProvider.CreateException(ProblemErrorCodes.CompetitionNotFound))
            );

        return Result<Competition?>.Done(competitionResult.Value);
    }

    /// <summary>
    /// Создание соревнования - экземпляра <see cref="Competition"/> (фабричный метод).
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
    public async Task<Result<Competition>> CreateCompetitionAsync(string name, 
        IList<string> conductingOrganizations, DateTime initialDate, DateTime endDate, string venue, string shortName,
        CompetitionsStatus competitionsStatus, DetailedCompetitionStatus detailedCompetitionStatus,
        bool isStudentCompetition = false,
        string? description = null)
    {
        // Создаем данные о соревновании и добавляем в репозиторий
        var competition =
            new Competition(name, conductingOrganizations, initialDate, endDate, venue, shortName,
                competitionsStatus, detailedCompetitionStatus,
                isStudentCompetition, description);
        var intResult = repository.Add(competition);
        if (! intResult)
        {
            return Result<Competition>.Fail(
                problemErrorMsgProvider.CreateException(
                    ProblemErrorCodes.CompetitionCreateError, intResult.Excptn)
            );
        }
        
        // Сохраняем изменения в репозитории
        intResult = await repository.SaveChangesAsync();
        if (! intResult)
        {
            return Result<Competition>.Fail(
                problemErrorMsgProvider.CreateException(
                    ProblemErrorCodes.CompetitionCreateError, intResult.Excptn)
            );
        }

        return Result<Competition>.Done(competition);
    }

    /// <summary>
    /// Создание нового нового соревнования.
    /// </summary>
    /// <remarks>
    /// Создаем с сохранением изменений в репозитории.
    /// </remarks>
    public async Task<Result<Competition>> CreateCompetitionAsync()
    {
        ProblemException innerException;
        
        // Получаем статус соревнования
        var competitionStatusResult = await repository.FindAsync<CompetitionsStatus>(
            CompetitionsStatusEnm.Regional);
        if (! competitionStatusResult)
        {
            innerException = problemErrorMsgProvider.CreateException(
                ProblemErrorCodes.CompetitionStatusFindError, competitionStatusResult.Excptn);

            return Result<Competition>.Fail(
                problemErrorMsgProvider.CreateException(
                    ProblemErrorCodes.CompetitionCreateError, innerException)
            );
        }
        
        // Получаем статус и обобщенное наименование соревнования
        var detailedCompetitionStatusResult = await repository.FindAsync<DetailedCompetitionStatus>(
            DetailedCompetitionStatusEnm.RegionalJuniorChampionship);
        if (! detailedCompetitionStatusResult)
        {
            innerException = problemErrorMsgProvider.CreateException(
                ProblemErrorCodes.DetailedCompetitionStatusFindError, detailedCompetitionStatusResult.Excptn);
            
            return Result<Competition>.Fail(
                problemErrorMsgProvider.CreateException(
                    ProblemErrorCodes.CompetitionCreateError, innerException)
            );
        }
        
        // Создаем соревнование
        var newCompetitionResult = await CreateCompetitionAsync(
            "НАЗВАНИЕ СОРЕВНОВАНИЙ",
            ["ПРОВОДЯЩАЯ ОРГАНИЗАЦИЯ"],
            DateTime.Now, DateTime.Now,
            "МЕСТО ПРОВЕДЕНИЯ", "СОРЕВЫ",
            competitionStatusResult.Value!,
            detailedCompetitionStatusResult.Value!
        );

        return newCompetitionResult
            ? Result<Competition>.Done(newCompetitionResult.Value!)
            : Result<Competition>.Fail(
                problemErrorMsgProvider.CreateException(
                    ProblemErrorCodes.CompetitionCreateError, newCompetitionResult.Excptn)
            );
    }

    /// <summary>
    /// Удаление соревнования.
    /// </summary>
    /// <param name="competition">Удаляемое соревнование.</param>
    /// <remarks>
    /// Удаляем с сохранением изменений в репозитории.
    /// </remarks>
    public async Task<Result<int>> RemoveCompetitionAsync(Competition competition)
    {
        // Удаляем из репозитория
        var intResult = repository.Remove(competition);
        if (! intResult)
        {
            return Result<int>.Fail(
                problemErrorMsgProvider.CreateException(
                    ProblemErrorCodes.CompetitionRemoveError, intResult.Excptn)
            );
        }
        
        // Сохраняем изменения в репозитории
        intResult = await repository.SaveChangesAsync();
        if (! intResult)
        {
            return Result<int>.Fail(
                problemErrorMsgProvider.CreateException(
                    ProblemErrorCodes.CompetitionRemoveError, intResult.Excptn)
            );
        }

        return Result<int>.Done(1);
    }


    /// <summary>
    /// Сохранение соревнования (включая зависимые сущности).
    /// </summary>
    public async Task<Result<int>> SaveCompetitionAsync()
    {
        var intResult = await repository.SaveChangesAsync();
        return intResult
            ? intResult
            : Result<int>.Fail(
                problemErrorMsgProvider.CreateException(
                    ProblemErrorCodes.CompetitionSaveError, intResult.Excptn)
            );
    }
}