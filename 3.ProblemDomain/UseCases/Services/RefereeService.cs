using Common.BaseComponents.Components;
using NExtensions.Async;
using ProblemDomain.Entities.CommonEntities;
using ProblemDomain.Entities.LibraryEntities;
using ProblemDomain.Entities.LibraryEntities.Enums;
using ProblemDomain.ProblemExceptions;
using ProblemDomain.UseCases._Contracts;
// ReSharper disable InvalidXmlDocComment

namespace ProblemDomain.UseCases.Services;

/// <summary>
/// Сервис для работы с Судьями.
/// </summary>
public class RefereeService
{
    private readonly IRepository _repository;
    private readonly IProblemErrorMsgProvider _problemErrorMsgProvider;

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="repository">Репозиторий.</param>
    /// <param name="problemErrorMsgProvider">Провайдер сообщений об ошибках слоя предметной области.</param>
    public RefereeService(IRepository repository, IProblemErrorMsgProvider problemErrorMsgProvider)
    {
        _repository = repository;
        _problemErrorMsgProvider = problemErrorMsgProvider;
        _roleAvailabilityCache = new AsyncLazy<Result<Dictionary<(RefereeCategoryEnm, DetailedCompetitionStatusEnm), List<RefereeRoleAvailability>>>>(
            async () => await LoadRoleAvailabilityCacheAsync()
        );
    }
    
    #region [---------- Судьи ----------]
    
    /// <summary>
    /// Получение коллекции всех судей, связанных с соревнованием.
    /// </summary>
    /// <param name="competition">Текущее соревнование.</param>
    public async Task<Result<IList<Referee>>> GetAllRefereesAsync(Competition? competition)
    {
        // Проверяем наличие соревнования
        if (competition == null)
        {
            return Result<IList<Referee>>.Fail(
                _problemErrorMsgProvider.CreateException(ProblemErrorCodes.RefereesLoadError,
                    _problemErrorMsgProvider.CreateException(ProblemErrorCodes.CompetitionIsNull))
            );
        }

        // Сбрасываем отслеживание сущностей
        var result = _repository.DetachAll<Referee>();
        if (! result)
            return Result<IList<Referee>>.Fail(
                _problemErrorMsgProvider.CreateException(
                ProblemErrorCodes.RefereesLoadError, result.Excptn)
            );
        
        // Загружаем данные из репозитория
        var refereesResult = await _repository.GetNumberedAllAsync<Referee>(
            true, 
            r => r.CompetitionId == competition.Id,
            nameof(Referee.Category),
            nameof(Referee.Role));
        if (! refereesResult)
            return Result<IList<Referee>>.Fail(
                _problemErrorMsgProvider.CreateException(
                ProblemErrorCodes.RefereesLoadError, refereesResult.Excptn)
            );

        return Result<IList<Referee>>.Done(refereesResult.Value ?? []);
    }

    /// <summary>
    /// Создание нового судьи.
    /// </summary>
    /// <param name="number">Номер судьи.</param>
    /// <param name="competition">Текущее соревнование.</param>
    /// <returns>Индекс нового текущего судьи.</returns>
    public async Task<Result<Referee>> CreateRefereeAsync(
        int number,
        Competition? competition)
    {
        ProblemException innerException;
        
        // Проверяем наличие соревнования
        if (competition == null)
        {
            return Result<Referee>.Fail(
                _problemErrorMsgProvider.CreateException(ProblemErrorCodes.RefereeCreateError,
                    _problemErrorMsgProvider.CreateException(ProblemErrorCodes.CompetitionIsNull))
            );
        }

        // Получаем судейскую категорию СС2К
        var refereeCategoryResult = await _repository.FindAsync<RefereeCategory>(RefereeCategoryEnm.Category2);
        if (! refereeCategoryResult)
        {
            innerException = _problemErrorMsgProvider.CreateException(
                ProblemErrorCodes.RefereeCategoryFindError, refereeCategoryResult.Excptn);
            
            return Result<Referee>.Fail(
                _problemErrorMsgProvider.CreateException(
                    ProblemErrorCodes.RefereeCreateError, innerException)
            );
        }
        
        // Получаем судейскую должность "Судья этапа"
        var refereeRoleResult = await _repository.FindAsync<RefereeRole>(RefereeRoleEnm.StageReferee);
        if (! refereeRoleResult)
        {
            innerException = _problemErrorMsgProvider.CreateException(
                ProblemErrorCodes.RefereeRoleFindError, refereeRoleResult.Excptn);
            
            return Result<Referee>.Fail(
                _problemErrorMsgProvider.CreateException(
                    ProblemErrorCodes.RefereeCreateError, innerException)
            );
        }

        var newReferee = new Referee(
            number,
            "ФАМИЛИЯ",
            "ИМЯ",
            "ТЕРРИТОРИЯ",
            refereeCategoryResult.Value!,
            refereeRoleResult.Value!,
            competition);
        
        // Добавляем судью в репозиторий
        var intResult = _repository.Add(newReferee);
        if (! intResult)
        {
            return Result<Referee>.Fail(
                _problemErrorMsgProvider.CreateException(
                    ProblemErrorCodes.RefereeCreateError, intResult.Excptn)
            );
        }

        return Result<Referee>.Done(newReferee);
    }
    
    /// <summary>
    /// Удаление судьи.
    /// </summary>
    /// <param name="refereeToRemove">Удаляемый судья.</param>
    public Result<int> RemoveReferee(Referee refereeToRemove)
    {
        // Удаляем из репозитория
        var intResult = _repository.Remove(refereeToRemove);
        if (! intResult)
        {
            return Result<int>.Fail(
                _problemErrorMsgProvider.CreateException(
                    ProblemErrorCodes.RefereeRemoveError, intResult.Excptn)
            );
        }

        return Result<int>.Done(1);
    }
    
    #endregion

    #region [---------- Суд. категории ----------]

    /// <summary>
    /// Получение коллекции всех судейских категорий.
    /// </summary>
    public async Task<Result<IList<RefereeCategory>>> GetAllRefereeCategoriesAsync()
    {
        var categoriesResult = await _repository.GetAllAsync<RefereeCategory>();

        return categoriesResult
            ? Result<IList<RefereeCategory>>.Done(categoriesResult.Value!)
            : Result<IList<RefereeCategory>>.Fail(
                _problemErrorMsgProvider.CreateException(
                    ProblemErrorCodes.RefereeCategoriesLoadError, categoriesResult.Excptn)
            );
    }

    /// <summary>
    /// Получение признака судейской категории, у которой срок действия истёк.
    /// </summary>
    /// <param name="referee">Судья, у которого проверяется категория.</param>
    /// <param name="date">Дата, относительно которой рассчитывается срок действия.</param>
    public bool? GetIsRefereeCategoryExpiredOn(Referee referee, DateTime? date)
    {
        // Получаем срок действия категории
        var validityPeriod = referee.CategoryId switch
        {
            RefereeCategoryEnm.Category3       => 1,
            RefereeCategoryEnm.AllRussCategory => 4,
            _                                  => 2
        };

        if (referee.CategoryGrantedDate == null || date == null)
            return null;
            
        return referee.CategoryGrantedDate.Value.AddYears(validityPeriod) < date;
    }

    /// <summary>
    /// Получение словаря доступности судейских категорий для судьи.
    /// </summary>
    /// <remarks>
    /// Проверяется только минимально возможная категория, в зависимости от статуса соревнования.
    /// </remarks>
    /// <param name="referee">Текущий судья.</param>
    /// <param name="categories">Исходная коллекция категорий.</param>
    /// <param name="isRefereeDataSelectionRestricted">Признак ограничения данных у судей.</param>
    /// <returns>Словарь: Id категории -> доступна ли она для данного судьи.</returns>
    public Dictionary<RefereeCategoryEnm, bool> GetAvailableCategories(
        Referee referee,
        IList<RefereeCategory> categories,
        bool isRefereeDataSelectionRestricted)
    {
        // Применяем бизнес-логику только если установлен признак
        if (isRefereeDataSelectionRestricted)
        {
            // Если статус < Regional — для категорий ниже Category2 возвращаем false
            if (referee.Competition.DetailedCompetitionStatus.CompetitionsStatus is not { Id: >= CompetitionsStatusEnm.Regional })
                return categories.ToDictionary(rc => rc.Id, rc => rc.Id >= RefereeCategoryEnm.Category2);
        }

        // Возвращаем словарь со всеми значениями категорий, равными true
        return categories.ToDictionary(rc => rc.Id, _ => true);
    }
    
    #endregion

    #region [---------- Суд. должности ----------]
    
    /// <summary>
    /// Результат расчета доступности должностей.
    /// </summary>
    private class AvailableRolesResult
    {
        /// <summary>
        /// Словарь доступности: Id должности -> доступна/недоступна.
        /// </summary>
        public Dictionary<RefereeRoleEnm, bool> AvailabilityDict { get; init; } = new();
        
        /// <summary>
        /// Список записей о доступности должностей из кэша.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public List<RefereeRoleAvailability> AvailabilityList { get; init; } = [];
        
        /// <summary>
        /// Словарь занятых должностей: Id должности -> количество судей, уже имеющих данную должность.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public Dictionary<RefereeRoleEnm, int> OccupiedCounts { get; init; } = new();
    }
    
    /// <summary>
    ///  Кеш для <see cref="RefereeRoleAvailability"/>: (категория, статус) -> RefereeRoleAvailability, обернутый в Result
    /// </summary>
    private readonly AsyncLazy<Result<Dictionary<(RefereeCategoryEnm Category, DetailedCompetitionStatusEnm Status), List<RefereeRoleAvailability>>>> 
        _roleAvailabilityCache;
    
    /// <summary>
    /// Загружает все доступности должностей из репозитория в кеш.
    /// </summary>
    /// <remarks>
    /// Кеш в виде словаря: (Категория, Детальный статус соревнований) → List&lt;<see cref="RefereeRoleAvailability"/>&gt;
    /// </remarks>
    private async Task<Result<Dictionary<(RefereeCategoryEnm, DetailedCompetitionStatusEnm), List<RefereeRoleAvailability>>>> 
        LoadRoleAvailabilityCacheAsync()
    {
        var roleAvailabilityResult = await _repository.GetAllAsync<RefereeRoleAvailability>();
        if (! roleAvailabilityResult)
        {
            return Result<Dictionary<(RefereeCategoryEnm, DetailedCompetitionStatusEnm), List<RefereeRoleAvailability>>>.Fail(
                roleAvailabilityResult.Excptn!
            );
        }
        
        var cache = new Dictionary<(RefereeCategoryEnm, DetailedCompetitionStatusEnm), List<RefereeRoleAvailability>>();
        
        // Группируем записи по статусу соревнований
        var roleAvailabilityByStatus = roleAvailabilityResult.Value!
                                                             .GroupBy(item => item.DetailedCompetitionStatusId);
    
        foreach (var statusGroup in roleAvailabilityByStatus)
        {
            var competitionStatus = statusGroup.Key;
        
            // Перебираем каждую запись доступности роли для данного статуса
            foreach (var roleAvailability in statusGroup)
            {
                var requiredCategory = roleAvailability.RefereeCategoryId;
            
                // Должность доступна для всех категорий, которые выше требуемой
                // (например: СС3К (2) → СС2К (3), СС1К (4), ССВК (5))
                for (var category = requiredCategory; category <= RefereeCategoryEnm.AllRussCategory; category++)
                {
                    var cacheKey = (category, competitionStatus);
                
                    if (!cache.ContainsKey(cacheKey))
                        cache[cacheKey] = [];
                
                    // Добавляем должность для данной категории (с сохранением количества)
                    cache[cacheKey].Add(roleAvailability);
                }
            }
        }
        
        return Result<Dictionary<(RefereeCategoryEnm, DetailedCompetitionStatusEnm), List<RefereeRoleAvailability>>>.Done(cache);
    }
    
    /// <summary>
    /// Единый метод расчета доступности должностей.
    /// </summary>
    /// <inheritdoc cref="GetAvailableRolesAsync" />
    /// <param name="singleRoleId">Если указано - вычислять только для конкретной должности (оптимизация и валидация).</param>
    private async Task<Result<AvailableRolesResult>> CalculateRolesAsync(
        Referee referee,
        IList<RefereeRole> occupiedRefereeRoles,
        int sportEventCount,
        RefereeRoleEnm? singleRoleId = null)
    {
        // Получаем кэш
        var cacheResult = await _roleAvailabilityCache;
        if (! cacheResult)
            // Ошибка
            return Result<AvailableRolesResult>.Fail(
                _problemErrorMsgProvider.CreateException(
                    ProblemErrorCodes.RefereeAvailableRolesCalculateError, cacheResult.Excptn));
        
        var cache = cacheResult.Value!;

        // Определяем ключ для кэша
        var statusId = referee.Competition.DetailedCompetitionStatus.Id;
        var categoryId = referee.Category.Id;
        var key = (categoryId, statusId);
        if (! cache.TryGetValue(key, out var availabilityList))
        {
            // Нет доступных должностей - возвращаем пустые значения
            return Result<AvailableRolesResult>.Done(new AvailableRolesResult());
        }

        // Фильтруем по конкретной должности, если задана (для валидации/оптимизации)
        var filteredList = singleRoleId.HasValue
            ? availabilityList.Where(a => a.Id == singleRoleId.Value).ToList()
            : availabilityList;

        // Получаем словарь занятых должностей: Id должности -> количество занятых мест.
        var occupiedCounts = occupiedRefereeRoles
                             .GroupBy(rr => rr.Id)
                             .ToDictionary(grouping => grouping.Key, grouping => grouping.Count());

        // Исключаем текущего судьи, если проверяем конкретную должность (валидация)
        var excludeCurrentReferee = singleRoleId.HasValue && ! referee.IsNew;
        if (excludeCurrentReferee)
        {
            var currentRoleId = referee.Role.Id;
            if (occupiedCounts.TryGetValue(currentRoleId, out var count) && count > 0)
                occupiedCounts[currentRoleId] = count - 1;
        }

        // Формируем specialRoleIds: должности, количество которых может быть только 1
        var specialRoleIds = new HashSet<RefereeRoleEnm>
        {
            RefereeRoleEnm.ChiefReferee,            // Главный судья
            RefereeRoleEnm.ChiefSecretary,          // Главный секретарь
            RefereeRoleEnm.DeputyForRefereeing,     // Зам. гл. судьи по судейству
            RefereeRoleEnm.DeputyForSecurity,       // Зам. гл. судьи по безопасности
            
            RefereeRoleEnm.SeniorInspector,         // Старший судья-инспектор
            
            // А также
            RefereeRoleEnm.MandateChairman,         // Председатель КПД
            RefereeRoleEnm.RccChairman,             // Председатель МКК
        };

        // Формируем activeRoleIds: specialRoleIds + все должности ГСК и ИС
        var activeRoleIds = new HashSet<RefereeRoleEnm>(specialRoleIds);
        for (var roleId = RefereeRoleEnm.ChiefReferee; roleId <= RefereeRoleEnm.Inspector; roleId++)
        {
            activeRoleIds.Add(roleId);
        }

        // Формируем результат
        var availabilityDict = new Dictionary<RefereeRoleEnm, bool>();
        foreach (var role in filteredList)
        {
            // Если должность не входит в activeRoleIds — она считается ДОСТУПНОЙ (без ограничений)
            if (! activeRoleIds.Contains(role.Id))
            {
                availabilityDict[role.Id] = true;
                continue;
            }

            // количество судей, уже имеющих данную должность 
            var occupiedCount = occupiedCounts.GetValueOrDefault(role.Id, 0);

            // Для specialRoleIds может быть только одна должность
            var maxRefereesCount = specialRoleIds.Contains(role.Id)
                ? 1
                : role.MaxRefereesPerRole * sportEventCount;

            // Проверяем: количество занятых должностей меньше максимально допустимого количества?
            availabilityDict[role.Id] = occupiedCount < maxRefereesCount;
        }
        
        return Result<AvailableRolesResult>.Done(new AvailableRolesResult
        {
            AvailabilityDict = availabilityDict,
            AvailabilityList = filteredList,
            OccupiedCounts = occupiedCounts
        });
    }
    
    /// <summary>
    /// Получение коллекции всех судейских должностей.
    /// </summary>
    public async Task<Result<IList<RefereeRole>>> GetAllRefereeRolesAsync()
    {
        var rolesResult =
            await _repository.GetAllAsync<RefereeRole>(
                navigationProperties: [nameof(RefereeRole.DisciplineGroups)]    // навигационное свойство - коллекция групп дисциплин
            );

        return rolesResult 
            ? Result<IList<RefereeRole>>.Done(rolesResult.Value!)
            : Result<IList<RefereeRole>>.Fail(
                _problemErrorMsgProvider.CreateException(
                    ProblemErrorCodes.RefereeRolesLoadError, rolesResult.Excptn)
            );
    }

    /// <summary>
    /// Получение словаря доступности судейских должностей для судьи.
    /// </summary>
    /// <param name="referee">Судья.</param>
    /// <param name="roles">Исходная коллекция должностей.</param>
    /// <param name="isRefereeDataSelectionRestricted">Признак ограничения данных у судей.</param>
    /// <param name="disciplineGroupFilter">Фильтр по группе дисциплин.</param>
    /// <param name="occupiedRefereeRoles">Список уже занятых судейских должностей.</param>
    /// <param name="sportEventCount">Количество спортивных событий.</param>
    /// <returns>Словарь: Id должности -> доступна ли она для данного судьи.</returns>
    public async Task<Result<Dictionary<RefereeRoleEnm, bool>>> GetAvailableRolesAsync(
        Referee referee,
        IList<RefereeRole> roles,
        bool isRefereeDataSelectionRestricted,
        DisciplineGroup? disciplineGroupFilter,
        IList<RefereeRole> occupiedRefereeRoles,
        int sportEventCount)
    {
        // Применяем дальнейшую бизнес-логику только если установлен признак
        if (! isRefereeDataSelectionRestricted)
            // Возвращаем словарь со всеми значениями должностей, равными true
            return Result<Dictionary<RefereeRoleEnm, bool>>.Done(roles.ToDictionary(rr => rr.Id, _ => true));
        
        var result = await CalculateRolesAsync(referee, occupiedRefereeRoles, sportEventCount);

        if (! result)
            // Ошибка
            return Result<Dictionary<RefereeRoleEnm, bool>>.Fail(result.Excptn!);

        var availabilityDict = result.Value!.AvailabilityDict;

        // Исключаем должности не удовлетворяющие фильтру
        if (disciplineGroupFilter != null)
        {
            foreach (var role in roles.Where(rr => rr.DisciplineGroups.All(dg => dg.Id != disciplineGroupFilter.Id)))
            {
                availabilityDict[role.Id] = false;
            }
        }

        return Result<Dictionary<RefereeRoleEnm, bool>>.Done(availabilityDict);
    }

    /// <summary>
    /// Получение признака валидности должности судьи.
    /// </summary>
    /// <inheritdoc cref="GetAvailableRolesAsync" />
    /// <remarks>
    /// Учитывает, что судья уже может занимать эту должность (исключает его из подсчета).
    /// </remarks>
    /// <returns>True - должность валидна, False - невалидна.</returns>
    public async Task<Result<bool>> GetIsRoleValidAsync(
        Referee referee,
        bool isRefereeDataSelectionRestricted,
        DisciplineGroup? disciplineGroupFilter,
        IList<RefereeRole> occupiedRefereeRoles,
        int sportEventCount)
    {
        var roleId = referee.Role.Id;

        var result = await CalculateRolesAsync(referee, occupiedRefereeRoles, sportEventCount, roleId);
    
        if (!result)
            // Ошибка
            return Result<bool>.Fail(result.Excptn!);

        var availabilityDict = result.Value!.AvailabilityDict;

        // Применяем дальнейшую бизнес-логику только если установлен признак
        if (! isRefereeDataSelectionRestricted)
            return Result<bool>.Done(true);

        // Проверяем доступность должности по фильтру по группе дисциплин
        var isAvailable = ! (disciplineGroupFilter != null && referee.Role.DisciplineGroups.All(dg => dg.Id != disciplineGroupFilter.Id));

        // Проверяем доступность должности по словарю доступности
        isAvailable = isAvailable && availabilityDict.TryGetValue(roleId, out var isAvailableFromDict) && isAvailableFromDict;

        return Result<bool>.Done(isAvailable);
    }
    
    #endregion
}