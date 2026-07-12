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
    // TODO: Временно, видимо
    // Словарь текстовых значений для трудностей вида программы.
    private Dictionary<(DisciplineGroupEnm, Difficulty.IdEnm), string> _difficulties = new()
    {
        {(DisciplineGroupEnm.Trek, Difficulty.IdEnm.LowThird), "3 ст. сложности"},
        {(DisciplineGroupEnm.Trek, Difficulty.IdEnm.LowSecond), "2 ст. сложности"},
        {(DisciplineGroupEnm.Trek, Difficulty.IdEnm.LowFirst), "1 ст. сложности"},
        {(DisciplineGroupEnm.Trek, Difficulty.IdEnm.First), "1 кат. сложности"},
        {(DisciplineGroupEnm.Trek, Difficulty.IdEnm.Second), "2 кат. сложности"},
        {(DisciplineGroupEnm.Trek, Difficulty.IdEnm.Third), "3 кат. сложности"},
        {(DisciplineGroupEnm.Trek, Difficulty.IdEnm.Fourth), "4 кат. сложности"},
        {(DisciplineGroupEnm.Trek, Difficulty.IdEnm.Fifth), "5 кат. сложности"},
        {(DisciplineGroupEnm.Trek, Difficulty.IdEnm.Sixth), "6 кат. сложности"},
        {(DisciplineGroupEnm.Distance, Difficulty.IdEnm.First), "1 класс дистанции"},
        {(DisciplineGroupEnm.Distance, Difficulty.IdEnm.Second), "2 класс дистанции"},
        {(DisciplineGroupEnm.Distance, Difficulty.IdEnm.Third), "3 класс дистанции"},
        {(DisciplineGroupEnm.Distance, Difficulty.IdEnm.Fourth), "4 класс дистанции"},
        {(DisciplineGroupEnm.Distance, Difficulty.IdEnm.Fifth), "5 класс дистанции"},
        {(DisciplineGroupEnm.Distance, Difficulty.IdEnm.Sixth), "6 класс дистанции"},
        {(DisciplineGroupEnm.NordicWalking, Difficulty.IdEnm.First), "1 класс дистанции"},
        {(DisciplineGroupEnm.NordicWalking, Difficulty.IdEnm.Second), "2 класс дистанции"},
        {(DisciplineGroupEnm.NordicWalking, Difficulty.IdEnm.Third), "3 класс дистанции"},
        {(DisciplineGroupEnm.NordicWalking, Difficulty.IdEnm.Fourth), "4 класс дистанции"},
        {(DisciplineGroupEnm.NordicWalking, Difficulty.IdEnm.Fifth), "5 класс дистанции"},
        {(DisciplineGroupEnm.NordicWalking, Difficulty.IdEnm.Sixth), "6 класс дистанции"},
    };
    
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
    public async Task<Result<SportEvent>> CreateSportEventAsync(CompetitionData? competition)
    {
        // Проверяем наличие соревнования
        if (competition == null!)
        {
            return Result<SportEvent>.Fail(
                problemErrorMsgProvider.CreateException(ProblemErrorCodes.SportEventCreateError,
                    problemErrorMsgProvider.CreateException(ProblemErrorCodes.CompetitionDataIsNull))
            );
        }
        
        // Получаем дисциплину
        var disciplineResult = await repository.FindAsync<Discipline>(DisciplineEnm.DistanceHiking);
        if (! disciplineResult)
        {
            var innerException = problemErrorMsgProvider.CreateException(
                ProblemErrorCodes.DisciplineLoadError, disciplineResult.Excptn);

            return Result<SportEvent>.Fail(
                problemErrorMsgProvider.CreateException(
                    ProblemErrorCodes.SportEventCreateError, innerException)
            );
        }
        
        // Новый вид программы
        var newSportEvent = new SportEvent(
            "НОВЫЙ ВИД ПРОГРАММЫ",
            null,
            Difficulty.IdEnm.First,
            disciplineResult.Value!,
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
    /// Получение словаря текстовых значений для трудностей видов программы: (DisciplineGroup, Difficulty) -> string.
    /// </summary>
    /// <remarks>
    /// TODO: Временно, потом сделаем с помощью БД.
    /// </remarks>
    public async Task<Result<Dictionary<(DisciplineGroupEnm, Difficulty.IdEnm), string>>> GetAllDifficultiesAsync()
    {
        return Result<Dictionary<(DisciplineGroupEnm, Difficulty.IdEnm), string>>.Done(_difficulties);
    }

    /// <summary>
    /// Получение трудности вида программы в виде текста.
    /// </summary>
    /// <remarks>
    /// TODO: Временно, потом сделаем с помощью БД.
    /// </remarks>
    public async Task<Result<string>> GetDifficultyAsync(
        DisciplineGroup disciplineGroup, 
        Difficulty.IdEnm difficulty)
    {
        return Result<string>.Done(_difficulties[(disciplineGroup.Id, difficulty)]);
    }
    
    /// <summary>
    /// Получение отфильтрованной коллекции значений трудности.
    /// </summary>
    /// <param name="difficultyMap">Исходный словарь значений трудности</param>
    /// <param name="discipline">Дисциплина, которой ограничиваем значения трудностей.</param>
    /// <remarks>
    /// TODO: Пока с использованием KeyValuePair. Когда будем из БД получать - переделаем, видимо.
    /// </remarks>
    public IList<KeyValuePair<Difficulty.IdEnm, string>> GetAvailableDifficulties(
        IDictionary<(DisciplineGroupEnm, Difficulty.IdEnm), string> difficultyMap,
        Discipline? discipline)
    {
        if (discipline == null)
            return [];
        
        return new List<KeyValuePair<Difficulty.IdEnm, string>>(
            difficultyMap
                .Where(kvp => kvp.Key.Item1 == discipline.DisciplineGroupId)
                .Select(kvp => new KeyValuePair<Difficulty.IdEnm, string>(kvp.Key.Item2, kvp.Value))
        );
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