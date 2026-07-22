using Common.BaseComponents.Components;
using ProblemDomain.Entities.CommonEntities;
using ProblemDomain.Entities.DistanceEntities;
using ProblemDomain.Entities.LibraryEntities;
using ProblemDomain.Entities.LibraryEntities.Enums;
using ProblemDomain.ProblemExceptions;
using ProblemDomain.UseCases._Contracts;

namespace ProblemDomain.UseCases.Services;

/// <summary>
/// Сервис для работы с Видами программы.
/// </summary>
/// <param name="repository">Репозиторий.</param>
/// <param name="problemErrorMsgProvider">Провайдер сообщений об ошибках слоя предметной области.</param>
public class SportEventService(IRepository repository, IProblemErrorMsgProvider problemErrorMsgProvider)
{
    /// <summary>
    /// Получение видов программы, связанных с соревнованием.
    /// </summary>
    /// <param name="competition">Текущее соревнование.</param>
    /// <returns>Список видов программы.</returns>
    public async Task<Result<IList<SportEvent>>> GetSportEventsAsync(CompetitionData competition)
    {
        // Проверяем наличие соревнования
        if (competition == null!)
        {
            return Result<IList<SportEvent>>.Fail(
                problemErrorMsgProvider.CreateException(ProblemErrorCodes.SportEventsLoadError,
                    problemErrorMsgProvider.CreateException(ProblemErrorCodes.CompetitionDataIsNull))
            );
        }

        // Сбрасываем отслеживание сущностей
        var result = repository.DetachAll<SportEvent>();
        if (! result)
            return Result<IList<SportEvent>>.Fail(
                problemErrorMsgProvider.CreateException(
                ProblemErrorCodes.SportEventsLoadError, result.Excptn)
            );
        
        // Загружаем данные из репозитория
        var sportEventsResult = await repository.GetAllAsync<SportEvent>(
            se => se.CompetitionDataId == competition.Id,
            nameof(Discipline));
        if (! sportEventsResult)
            return Result<IList<SportEvent>>.Fail(
                problemErrorMsgProvider.CreateException(
                ProblemErrorCodes.SportEventsLoadError, sportEventsResult.Excptn)
            );

        return Result<IList<SportEvent>>.Done(sportEventsResult.Value ?? []);
    }

    /// <summary>
    /// Создание нового вида программы.
    /// </summary>
    /// <param name="competition">Текущее соревнование.</param>
    /// <param name="availableDisciplines">Коллекция доступных дисциплин.</param>
    public async Task<Result<SportEvent>> CreateSportEventAsync(
        CompetitionData? competition, 
        IList<Discipline> availableDisciplines)
    {
        // Проверяем наличие соревнования
        if (competition == null!)
        {
            return Result<SportEvent>.Fail(
                problemErrorMsgProvider.CreateException(ProblemErrorCodes.SportEventCreateError,
                    problemErrorMsgProvider.CreateException(ProblemErrorCodes.CompetitionDataIsNull))
            );
        }
        
        // Получаем дисциплину (по возможности - из дистанций и пешеходка)
        var discipline = availableDisciplines.FirstOrDefault(d => d.DisciplineSubGroupId == DisciplineSubGroupEnm.Hiking)
                         ?? availableDisciplines.FirstOrDefault(d => d.DisciplineGroupId == DisciplineGroupEnm.Distance) 
                         ?? availableDisciplines.First();
        
        // Получаем трудность
        DifficultyEnm difficultyId;
        if (discipline.DisciplineGroupId == DisciplineGroupEnm.Trek)
        {
            // TODO: Маршрут (пока только для мужчин/женщин)
            difficultyId = competition.CompetitionsStatusId switch
            {
                CompetitionsStatusEnm.Municipal     => DifficultyEnm.First,
                CompetitionsStatusEnm.Regional      => DifficultyEnm.Second,
                CompetitionsStatusEnm.Interregional => DifficultyEnm.Fourth,
                _                                   => DifficultyEnm.Fifth,     // всероссийские
            };
        }
        else
        {
            // TODO: Дистанция (требует проверки)
            difficultyId = competition.CompetitionsStatusId switch
            {
                CompetitionsStatusEnm.Municipal     => DifficultyEnm.First,
                CompetitionsStatusEnm.Regional      => DifficultyEnm.Third,
                CompetitionsStatusEnm.Interregional => DifficultyEnm.Fourth,
                _                                   => DifficultyEnm.Fifth,     // всероссийские
            };
        }
        var difficultyResult = await GetDifficultyAsync(difficultyId, discipline.DisciplineGroupId);
        if (! difficultyResult)
        {
            var innerException = problemErrorMsgProvider.CreateException(
                ProblemErrorCodes.DifficultyLoadError, difficultyResult.Excptn);
        
            return Result<SportEvent>.Fail(
                problemErrorMsgProvider.CreateException(
                    ProblemErrorCodes.SportEventCreateError, innerException)
            );
        }
        
        // Новый вид программы
        var newSportEvent = new SportEvent(
            "НОВЫЙ ВИД ПРОГРАММЫ",
            null,
            difficultyResult.Value!,
            discipline,
            competition);

        // Добавляем в репозиторий
        var intResult = repository.Add(newSportEvent);
        if (! intResult)
        {
            return Result<SportEvent>.Fail(
                problemErrorMsgProvider.CreateException(
                    ProblemErrorCodes.SportEventCreateError, intResult.Excptn)
            );
        }

        return Result<SportEvent>.Done(newSportEvent);
    }

    /// <summary>
    /// Удаление вида программы.
    /// </summary>
    public Result<int> RemoveSportEvent(SportEvent sportEventToRemove)
    {
        // Удаляем из репозитория
        var intResult = repository.Remove(sportEventToRemove);
        if (! intResult)
        {
            return Result<int>.Fail(
                problemErrorMsgProvider.CreateException(
                    ProblemErrorCodes.SportEventRemoveError, intResult.Excptn)
            );
        }

        return Result<int>.Done(1);
    }
    
    /// <summary>
    /// Получение списка групп дисциплин.
    /// </summary>
    public async Task<Result<IList<DisciplineGroup>>> GetDisciplineGroupsAsync()
    {
        var disciplineGroupsResult = await repository.GetAllAsync<DisciplineGroup>();

        return disciplineGroupsResult
            ? Result<IList<DisciplineGroup>>.Done(disciplineGroupsResult.Value!)
            : Result<IList<DisciplineGroup>>.Fail(
                problemErrorMsgProvider.CreateException(
                ProblemErrorCodes.DisciplineGroupsLoadError, disciplineGroupsResult.Excptn)
            );
    }

    /// <summary>
    /// Получение списка подгрупп дисциплин.
    /// </summary>
    public async Task<Result<IList<DisciplineSubGroup>>> GetDisciplineSubGroupsAsync()
    {
        var disciplineSubGroupsResult = await repository.GetAllAsync<DisciplineSubGroup>();

        return disciplineSubGroupsResult
            ? Result<IList<DisciplineSubGroup>>.Done(disciplineSubGroupsResult.Value!)
            : Result<IList<DisciplineSubGroup>>.Fail(
                problemErrorMsgProvider.CreateException(
                ProblemErrorCodes.DisciplineSubGroupsLoadError, disciplineSubGroupsResult.Excptn)
            );
    }
        
    /// <summary>
    /// Получение списка дисциплин.
    /// </summary>
    public async Task<Result<IList<Discipline>>> GetDisciplinesAsync()
    {
        var disciplinesResult = await repository.GetAllAsync<Discipline>();

        return disciplinesResult
            ? Result<IList<Discipline>>.Done(disciplinesResult.Value!)
            : Result<IList<Discipline>>.Fail(
                problemErrorMsgProvider.CreateException(
                ProblemErrorCodes.DisciplinesLoadError, disciplinesResult.Excptn)
            );
    }

    /// <summary>
    /// Получение отфильтрованной коллекции доступных дисциплин.
    /// </summary>
    /// <param name="disciplines">Полная коллекция дисциплин.</param>
    /// <param name="filteringDisciplineSubGroup">Подгруппа дисциплин, которой ограничиваем значения дисциплин.</param>
    /// <param name="filteringDisciplineGroup">Группа дисциплин, которой ограничиваем значения дисциплин.</param>
    public IList<Discipline> GetAvailableDisciplines(
        ICollection<Discipline> disciplines,
        DisciplineSubGroup? filteringDisciplineSubGroup,
        DisciplineGroup? filteringDisciplineGroup)
    {
        if (filteringDisciplineSubGroup != null)
            return new List<Discipline>(
                disciplines
                    .Where(d => d.DisciplineSubGroupId == filteringDisciplineSubGroup.Id)
            );
        if (filteringDisciplineGroup != null)
            return new List<Discipline>(
                disciplines
                    .Where(d => d.DisciplineGroupId == filteringDisciplineGroup.Id)
            );
        return new List<Discipline>(
            disciplines
        );
    }
    
    /// <summary>
    /// Получение коллекции трудностей видов программы.
    /// </summary>
    public async Task<Result<IList<Difficulty>>> GetAllDifficultiesAsync()
    {
        var difficultiesResult = await repository.GetAllAsync<Difficulty>();
        
        return difficultiesResult
            ? Result<IList<Difficulty>>.Done(difficultiesResult.Value!)
            : Result<IList<Difficulty>>.Fail(
                problemErrorMsgProvider.CreateException(
                    ProblemErrorCodes.DifficultiesLoadError, difficultiesResult.Excptn)
            );
    }

    /// <summary>
    /// Получение трудности вида программы.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public async Task<Result<Difficulty>> GetDifficultyAsync(
        DifficultyEnm difficultyId,
        DisciplineGroupEnm disciplineGroupId
        )
    {
        var difficultyResult = await repository.GetByConditionAsync<Difficulty>(
            d => d.Id == difficultyId && d.DisciplineGroupId == disciplineGroupId);
        
        return difficultyResult
            ? Result<Difficulty>.Done(difficultyResult.Value!)
            : Result<Difficulty>.Fail(
                problemErrorMsgProvider.CreateException(
                    ProblemErrorCodes.DifficultyLoadError, difficultyResult.Excptn)
            );
    }
    
    /// <summary>
    /// Получение отфильтрованной коллекции значений трудности.
    /// </summary>
    /// <param name="difficulties">Исходная коллекция значений трудности</param>
    /// <param name="discipline">Дисциплина, которой ограничиваем значения трудностей.</param>
    public IList<Difficulty> GetAvailableDifficulties(
        IList<Difficulty> difficulties,
        Discipline? discipline)
    {
        if (discipline == null)
            return [];

        return difficulties
               .Where(d => d.DisciplineGroupId == discipline.DisciplineGroupId)
               .ToList();
    }
    
    /// <summary>
    /// Получение доступности признака короткой дистанции.
    /// </summary>
    /// <remarks>
    /// Если группа дисциплин текущей дисциплины не является дистанцией, то признак короткой дистанции недоступен.
    /// </remarks>
    /// <param name="discipline">Текущая дисциплина.</param>
    public bool IsShortAvailable(Discipline? discipline)
    {
        return discipline?.DisciplineGroupId == DisciplineGroupEnm.Distance;
    }

    /// <summary>
    /// Обновление признака короткой дистанции.
    /// </summary>
    /// <remarks>
    /// Сброс в null признака короткой дистанции, если группа дисциплин текущей дисциплины не является дистанцией.
    /// </remarks>
    /// <param name="discipline">Текущая дисциплина.</param>
    /// <param name="sportEvent">Текущий вид программы.</param>
    /// <returns>True - если обновление признака произошло.</returns>
    public bool IsShortUpdate(Discipline? discipline, SportEvent? sportEvent)
    {
        if (discipline != null! && discipline.DisciplineGroupId != DisciplineGroupEnm.Distance)
        {
            sportEvent?.IsShort = null;
            return true;
        }
        
        return false;
    }
}