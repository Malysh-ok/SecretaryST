using System.Collections.ObjectModel;
using AppDomain.AppExceptions;
using AppDomain.Phrases;
using AppDomain.UseCases._Contracts;
using Common.BaseComponents.Components;
using Common.BaseComponents.Wrappers;
using Common.BaseExtensions.Collections;
using ProblemDomain.Entities.CommonEntities;
using ProblemDomain.Entities.LibraryEntities;
using ProblemDomain.Entities.LibraryEntities.Enums;

namespace AppDomain.UseCases.Services;

/// <summary>
/// Сервис для работы с Данными о соревнованиях.
/// </summary>
/// <param name="repository">Репозиторий.</param>
public class CompetitionDataService(IRepository repository)
{
    /// <summary>
    /// Получаем (обновляем) коллекцию проводящих организаций.
    /// </summary>
    /// <param name="competitionData">Данные о соревнованиях, откуда получаем проводящие организации.</param>
    /// <param name="conductingOrganizations">Коллекция проводящих организаций, которую обновляем</param>
    public Result<int> GetConductingOrganizations(CompetitionData? competitionData, ObservableCollection<StringItem> conductingOrganizations)
    {
        try
        {
            conductingOrganizations.Clear();
            competitionData?.ConductingOrganizations.ForEach(item => conductingOrganizations.Add(new StringItem(item)));
            
            return Result<int>.Done(conductingOrganizations.Count);
        }
        catch (Exception ex)
        {
            return Result<int>.Fail(new AppException(AppPhrases.ConductingOrganizationsGetError, ex));
        }
    }
    
    /// <summary>
    /// Задаем проводящие организации.
    /// </summary>
    /// <param name="competitionData">Данные о соревнованиях, в которой устанавливаем проводящие организации.</param>
    /// <param name="conductingOrganizations">Коллекция проводящих организаций - источник данных.</param>
    public Result<int> SetConductingOrganizations(CompetitionData? competitionData, ObservableCollection<StringItem> conductingOrganizations)
    {
        try
        {
            if (competitionData != null)
            {
                competitionData.ConductingOrganizations.Clear();
                conductingOrganizations.ForEach(item => competitionData.ConductingOrganizations.Add(item.Value));
            }
            
            return Result<int>.Done(competitionData!.ConductingOrganizations.Count);
        }
        catch (Exception ex)
        {
            return Result<int>.Fail(new AppException(AppPhrases.ConductingOrganizationsSetError, ex));
        }
    }
    
    /// <summary>
    /// Создаем новую проводящую организацию.
    /// </summary>
    /// <param name="conductingOrganizations">Текущая коллекция проводящих организаций.</param>
    /// <param name="index">Индекс текущей проводящей организации.</param>
    /// <returns>Индекс новой текущей проводящей организации.</returns>
    public Result<int> AddConductingOrganization(ObservableCollection<StringItem> conductingOrganizations, int index)
    {
        try
        {
            var newIndex = (index >= conductingOrganizations.Count || index < 0)
                ? conductingOrganizations.Count
                : index + 1;

            var newConductingOrganization = new StringItem("НОВАЯ ПРОВОДЯЩАЯ ОРГАНИЗАЦИЯ");
        
            // Добавляем в коллекцию организацию
            conductingOrganizations.Insert(newIndex, newConductingOrganization);
        
            return Result<int>.Done(newIndex);
        }
        catch (Exception ex)
        {
            return Result<int>.Fail(new AppException(AppPhrases.ConductingOrganizationsAddError, ex));
        }
    }
    
    /// <summary>
    /// Удаляем проводящую организацию.
    /// </summary>
    /// <param name="conductingOrganizations">Текущая коллекция проводящих организаций.</param>
    /// <param name="index">Индекс удаляемой проводящей организации.</param>
    /// <returns>Индекс новой текущей проводящей организации.</returns>
    public Result<int> RemoveConductingOrganization(ObservableCollection<StringItem> conductingOrganizations, int index)
    {
        try
        {
            if (index >= conductingOrganizations.Count || index < 0)
                return Result<int>.Done(index);
        
            // Удаляем их коллекции организацию
            conductingOrganizations.RemoveAt(index);
        
            return Result<int>.Done(index == 0 ? 0 : index - 1);
        }
        catch (Exception ex)
        {
            return Result<int>.Fail(new AppException(AppPhrases.ConductingOrganizationsRemoveError, ex));
        }
    }

    /// <summary>
    /// Получаем список статусов соревнований.
    /// </summary>
    public async Task<Result<IList<CompetitionsStatus>>> GetCompetitionsStatusesAsync()
    {
        var competitionsStatusesResult = await repository.GetAllAsync<CompetitionsStatus>();

        return competitionsStatusesResult 
            ? Result<IList<CompetitionsStatus>>.Done(competitionsStatusesResult.Value!) 
            : Result<IList<CompetitionsStatus>>.Fail(new AppException(AppPhrases.CompetitionsStatusesLoadError));
    }

    /// <summary>
    /// Получаем список статусов и обобщенных наименований соревнований.
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
    /// <param name="shortName">Краткое название соревнований.</param>
    /// <param name="competitionsStatus">Статус соревнования.</param>
    /// <param name="detailedCompetitionStatus">Статус и обобщенное наименование соревнования.</param>
    /// <param name="description">Создаем и сохраняем в репозитории.</param>
    public async Task<Result<CompetitionData>> CreateCompetitionDataAsync(string name, 
        IList<string> conductingOrganizations, DateTime initialDate, DateTime endDate, string venue, string shortName,
        CompetitionsStatus competitionsStatus, DetailedCompetitionStatus detailedCompetitionStatus,
        string? description = null)
    {
        // Создаем данные о соревновании и добавляем в репозиторий
        var competition =
            new CompetitionData(name, conductingOrganizations, initialDate, endDate, venue, shortName,
                competitionsStatus, detailedCompetitionStatus,
                description);
        var intResult = repository.Add(competition);
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
    /// Получение всех соревнований.
    /// </summary>
    /// <remarks>
    /// Без навигационных свойств ("легкий" список).
    /// </remarks>
    public async Task<Result<int>> GetAllCompetitionsDataAsync(ObservableCollection<CompetitionData> competitionDataCollection)
    {
        // Сбрасываем отслеживание сущностей
        var result = repository.DetachAll<CompetitionData>();
        if (!result)
            return Result<int>.Fail(new AppException(AppPhrases.CompetitionDataLoadError, result.Excptn));
        
        // Загружаем данные из репозитория
        var competitionsDataResult = await repository.GetAllAsync<CompetitionData>();
        if (!competitionsDataResult)
            return Result<int>.Fail(new AppException(AppPhrases.CompetitionDataLoadError, result.Excptn));

        // Перезаписываем коллекцию соревнований новыми данными
        competitionDataCollection.Clear();
        competitionsDataResult.Value.ForEach(competitionDataCollection.Add);

        return Result<int>.Done(competitionsDataResult.Value!.Count);
    }
    
    /// <summary>
    /// Получение соревнования.
    /// </summary>
    /// <remarks>
    /// С навигационными свойствами ("тяжелый" объект).
    /// </remarks>
    public async Task<Result<CompetitionData?>> GetCompetitionDataAsync(int competitionId, bool isCheckForNull = false)
    {
        // Сбрасываем отслеживание сущностей
        var result = repository.DetachAll<CompetitionData>();
        if (! result)
            return Result<CompetitionData?>.Fail(
                new AppException(AppPhrases.CompetitionDataLoadError, result.Excptn));

        // Загружаем данные из репозитория
        var competitionDataResult = await repository.GetByIdAsync<CompetitionData>(competitionId,
            nameof(CompetitionsStatus),
            nameof(DetailedCompetitionStatus));
        if (! competitionDataResult)
            return Result<CompetitionData?>.Fail(
                new AppException(AppPhrases.CompetitionDataLoadError, competitionDataResult.Excptn));
        
        if (isCheckForNull && competitionDataResult.Value == null)
            return Result<CompetitionData?>.Fail(
                new AppException(AppPhrases.CompetitionDataLoadError, 
                    new AppException(AppPhrases.CompetitionDataNotFound)));

        return Result<CompetitionData?>.Done(competitionDataResult.Value);
    }

    /// <summary>
    /// Добавление нового соревнования после текущего.
    /// </summary>
    /// <param name="competitionDataCollection">Текущая коллекция соревнований.</param>
    /// <returns>Индекс нового соревнования.</returns>
    public async Task<Result<CompetitionData>> AddCompetitionDataAsync(
        ObservableCollection<CompetitionData> competitionDataCollection)
    {
        AppException innerException;
        
        // Получаем статус соревнований
        var competitionsStatusResult = await repository.FindAsync<CompetitionsStatus>(
            CompetitionsStatusEnm.Regional);
        if (! competitionsStatusResult)
        {
            innerException = new AppException(AppPhrases.CompetitionsStatusFindError, competitionsStatusResult.Excptn);
            return Result<CompetitionData>.Fail(
                new AppException(AppPhrases.CompetitionDataCreateError, innerException)
            );
        }
        
        // Получаем статус и обобщенное наименование соревнования
        var detailedCompetitionStatusResult = await repository.FindAsync<DetailedCompetitionStatus>(
            DetailedCompetitionStatusEnm.RegionalJuniorChampionship);
        if (! detailedCompetitionStatusResult)
        {
            innerException = new AppException(AppPhrases.DetailedCompetitionStatusFindError, detailedCompetitionStatusResult.Excptn);
            return Result<CompetitionData>.Fail(
                new AppException(AppPhrases.CompetitionDataCreateError, innerException)
            );
        }
        
        // Создаем данные о соревновании
        var newCompetition = new CompetitionData(
            "НАЗВАНИЕ СОРЕВНОВАНИЙ",
            ["ПРОВОДЯЩАЯ ОРГАНИЗАЦИЯ"],
            DateTime.Now, DateTime.Now,
            "МЕСТО ПРОВЕДЕНИЯ", "СОРЕВЫ",
            competitionsStatusResult.Value!,
            detailedCompetitionStatusResult.Value!
        );
        
        // Добавляем в коллекцию соревнование
        competitionDataCollection.Add(newCompetition);
        
        // Обновляем судей в репозитории
        var intResult = repository.UpdateRange(competitionDataCollection);
        if (! intResult)
        {
            return Result<CompetitionData>.Fail(
                new AppException(AppPhrases.CompetitionDataCreateError, intResult.Excptn)
            );
        }

        return Result<CompetitionData>.Done(newCompetition);
    }

    /// <summary>
    /// Удаление текущего соревнования.
    /// </summary>
    /// <param name="competitionDataCollection">Текущая коллекция соревнований.</param>
    /// <param name="competition">Удаляемое соревнование</param>
    /// <returns>Новое текущее соревнование.</returns>
    public Task<Result<CompetitionData>> RemoveCompetitionDataAsync(
        ObservableCollection<CompetitionData> competitionDataCollection, CompetitionData? competition)
    {
        if (competition == null)
            return Task.FromResult(Result<CompetitionData>.Fail(
                new AppException(AppPhrases.CompetitionDataRemoveError, 
                    new AppException(AppPhrases.CompetitionDataIsNull))
            ));
        
        // Удаляем из коллекции соревнование
        var index = competitionDataCollection.IndexOf(competition);
        competitionDataCollection.Remove(competition);
        if (index >= competitionDataCollection.Count)
            index = competitionDataCollection.Count - 1;
        else if (index < 0)
            index = 0;
        
        // Удаляем из репозитория
        var intResult = repository.Remove(competition);
        if (! intResult)
        {
            return Task.FromResult(Result<CompetitionData>.Fail(
                new AppException(AppPhrases.CompetitionDataRemoveError, intResult.Excptn)
            ));
        }
        
        // Обновляем соревнования в репозитории
        intResult = repository.UpdateRange(competitionDataCollection);
        if (! intResult)
        {
            return Task.FromResult(Result<CompetitionData>.Fail(
                new AppException(AppPhrases.CompetitionDataRemoveError, intResult.Excptn)
            ));
        }

        return Task.FromResult(Result<CompetitionData>.Done(competitionDataCollection[index]));
    }

    /// <summary>
    /// Сохранение соревнования.
    /// </summary>
    public async Task<Result<int>> SaveCompetitionDataAsync()
    {
        var intResult = await repository.SaveChangesAsync();
        return intResult 
            ? intResult
            : Result<int>.Fail(new AppException(AppPhrases.CompetitionDataSaveError, intResult.Excptn));
    }
}