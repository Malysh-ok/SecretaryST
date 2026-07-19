using System.Reflection;
using AppDomain.AppAssets.Services;
using AppDomain.AppUseCases._Contracts;
using Common.BaseComponents.Components;
using Common.BaseComponents.Components.Exceptions;
using Common.BaseExtensions.ValueTypes;
using DataAccess.DataAccessAssets.Services;
using DataAccess.DbContexts;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Presentation.Shell.Infrastructure;
using ProblemDomain.Entities.CommonEntities;
using ProblemDomain.Entities.DistanceEntities;
using ProblemDomain.Entities.LibraryEntities;
using ProblemDomain.Entities.LibraryEntities.Enums;
using ProblemDomain.UseCases._Contracts;
using ProblemDomain.UseCases.Services;

namespace DataAccess.Tests;

public class RepositoryTests
{
    // Константа для маркировки теста: неавтоматический режим теста
    protected const string NoAutomatic = nameof(NoAutomatic);
    
    // Атрибут для маркировки тестов: в тесте пересоздаются сущности (а не добавляются)
    [AttributeUsage(AttributeTargets.Method)]
    private class RecreateEntitiesAttribute : Attribute { }
    
    private bool _isRecreateEntities = false;       // признак пересоздания сущностей
    
    [SetUp]
    public void Setup()
    {
        SQLitePCL.Batteries_V2.Init();
        
        // Игнорируем тест с атрибутом NoAutomatic
        var testCategory = (string)TestContext.CurrentContext.Test.Properties["Category"].FirstOrDefault()!;
        if (testCategory == NoAutomatic)
            Assert.Inconclusive();
        
        // Устанавливаем (или сбрасываем) признак пересоздания сущностей,
        // в зависимости от установленного атрибута RecreateEntities
        var methodInfo = TestContext.CurrentContext.Test.Method?.MethodInfo;
        _isRecreateEntities = methodInfo?.GetCustomAttribute<RecreateEntitiesAttribute>() != null;
    }
    
    #region [---------- Вспомогательные методы ----------]
    
    // Создать или получить Представителя
    private async Task<Representative> GetRepresentative(IRepository repository)
    {
        var representative = DateTime.Now.Millisecond % 2 == 0
            ? new Representative("Шлепко", "Яна", "Ульяновна")
            :new Representative("Банько", "Яна", "Евгеньевна");

        // Заменяем первого найденного в базе представителя на нового (для примера)
        var foundRepresentative = // получаем из репозитория
            (await repository.GetFirstAsync<Representative>()).Value;
        if (foundRepresentative is not null)
            representative.Copy(foundRepresentative);
        var result = repository.Update(foundRepresentative ?? representative);
        Assert.That(result.Excptn, Is.Null, result.Excptn?.Message);
        
        result = await repository.SaveChangesAsync();
        Assert.That(result.Excptn, Is.Null, result.Excptn?.Message);

        return foundRepresentative ?? representative;
    }

    /// <summary>
    /// Получение коллекции судей.
    /// </summary>
    private async Task<IList<Referee>> GetReferees(IRepository repository, CompetitionData competitionData)
    {
        List<Referee> referees = [];

        var jobTitle = // должность
            RefereeJobTitleEnm.ChiefReferee;
        referees.Add(new Referee(1,
                "Сретенский", "Сергей", 
                "г. Чебоксары", 
                (await repository.FindAsync<RefereeLevel>(RefereeLevelEnm.Category1)).Value!,
                (await repository.FindAsync<RefereeJobTitle>(jobTitle)).Value!,
                competitionData,
                "Валентинович",
                description: competitionData.ShortName  // временно
            )
        );
        
        jobTitle = RefereeJobTitleEnm.ChiefSecretary;
        referees.Add(new Referee(2,
                "Иванова", "Кристина", 
                "г. Чебоксары", 
                (await repository.FindAsync<RefereeLevel>(RefereeLevelEnm.Category1)).Value!,
                (await repository.FindAsync<RefereeJobTitle>(jobTitle)).Value!,
                competitionData,
                description: competitionData.ShortName  // временно
            )
        );
        
        jobTitle = RefereeJobTitleEnm.MandateChairman;
        referees.Add(new Referee(3,
                "Черкасова", "Маргарита", 
                "г. Санкт-Петербург", 
                (await repository.FindAsync<RefereeLevel>(RefereeLevelEnm.AllRussCategory)).Value!,
                (await repository.FindAsync<RefereeJobTitle>(jobTitle)).Value!,
                competitionData,
                description: competitionData.ShortName  // временно
            )
        );
        
        jobTitle = RefereeJobTitleEnm.Secretary;
        referees.Add(new Referee(4,
                "Тетка", "1", 
                "г. Чебоксары", 
                (await repository.FindAsync<RefereeLevel>(RefereeLevelEnm.Category2)).Value!,
                (await repository.FindAsync<RefereeJobTitle>(jobTitle)).Value!,
                competitionData,
                description: competitionData.ShortName  // временно
            )
        );
        
        jobTitle = RefereeJobTitleEnm.MajorStageReferee;
        referees.Add(new Referee(5,
                "Тетка", "2", 
                "г. Чебоксары", 
                (await repository.FindAsync<RefereeLevel>(RefereeLevelEnm.Category3)).Value!,
                (await repository.FindAsync<RefereeJobTitle>(jobTitle)).Value!,
                competitionData,
                description: competitionData.ShortName  // временно
            )
        );
        
        return referees;
    }
    
    /// <summary>
    /// Получение коллекции видов программы.
    /// </summary>
    private async Task<IList<SportEvent>> GetSportEvents(IRepository repository, CompetitionData competitionData)
    {
       
        var disciplineResult = await repository.FindAsync<Discipline>(DisciplineEnm.DistanceMountainBunch);

        // var sportUnitTypes = (await repository.GetAllAsync<SportUnitType>()).Value!;
        // var groupUnitType = sportUnitTypes.FirstOrDefault(sut => sut.Id == SportUnitTypeEnm.Group);
        // var waterTeamUnitType = sportUnitTypes.FirstOrDefault(sut => sut.Id == SportUnitTypeEnm.WaterTeam);
        
        // Создаем Виды программ
        var sportEvents = new List<SportEvent>()
        {
            new("Вид программы 1", true,
                Difficulty.IdEnm.Fourth, disciplineResult.Value!, competitionData),
            new("Вид программы 2", false,
                Difficulty.IdEnm.Third, disciplineResult.Value!, competitionData),
            new("Вид программы 3", null,
                Difficulty.IdEnm.Third, disciplineResult.Value!, competitionData),
        };
        
        return sportEvents;
    }

    private async Task<IList<SportUnit>> GetSportUnits(IRepository repository, 
        CompetitionData  competitionData, IList<SportEvent> sportEvents)
    {
        var sexes = (await repository.GetAllAsync<Sex>()).Value!.ToList();
        var sportUnitTypes = (await repository.GetAllAsync<SportUnitType>()).Value!;
        var groupUnitType = sportUnitTypes.FirstOrDefault(sut => sut.Id == SportUnitTypeEnm.Group);
        var waterTeamUnitType = sportUnitTypes.FirstOrDefault(sut => sut.Id == SportUnitTypeEnm.WaterTeam);

        var indexSportEvent = -1;

        var sportUnits = new List<SportUnit>()
        {
            new("Группа 1", sexes.Find(s => s.Id == SexEnm.Male)!, 
                groupUnitType!, GetNextSportEvent()),
            new("Группа 2", sexes.Find(s => s.Id == SexEnm.Female)!, 
                groupUnitType!, GetNextSportEvent()),
            new("Группа судов 1", sexes.Find(s => s.Id == SexEnm.Mixed)!,
                waterTeamUnitType!, GetNextSportEvent()),
        };
        sportUnits[2].ChildSportUnits.Add(sportUnits[1]);   // юнит3 -> юнит2
        
        return sportUnits;

        SportEvent GetNextSportEvent()
        {
            indexSportEvent++;
            if (indexSportEvent >= sportEvents.Count) indexSportEvent = 0;
            return sportEvents[indexSportEvent];
        }
    }

    private async Task<IList<Delegation>> GetDelegations(IRepository repository, CompetitionData competitionData)
    {
        // Представитель
        var representative = await GetRepresentative(repository);
        
        // Новые Делегации
        var delegations = new List<Delegation>()
        {
            new(1, name: "Делегация 11111", "Якутия", 
                representative, competitionData, 
                competitionData.ShortName), // временно
            new(2, name: "Делегация 22222", "Сахалин", 
                representative, competitionData, 
                competitionData.ShortName), // временно
            new(3, name: "Делегация 33333", "Камчатка", 
                representative, competitionData, 
                competitionData.ShortName), // временно
        };
        
        return delegations;
    }
    
    #endregion

    /// <summary>
    /// Полностью пересоздать БД.
    /// </summary>
    [Test]
    [Category($"{NoAutomatic}")]
    // [Category($"OnceAtBeginning")]
    public async Task RebuildDbTest()
    {
        IRepository? repository = null;
        try
        {
            // Создаем сервис настроек приложения и создаем необходимые каталоги
            // (без каталога метод dbContext.Database.MigrateAsync вызовет ошибку)
            IAppErrorMsgProvider appErrorMsgProvider = new DomainErrorMsgProvider();
            IEmbeddedResourceProvider embeddedResourceProvider = new EmbeddedResourceProvider();
            var testAppInfo = ServiceFactory.CreateAppInfo("Test", new Version(1,0,0,0), DateTime.Now);
            var appDirService = ServiceFactory.CreateAppDirService(appErrorMsgProvider);
            var appSettingService = ServiceFactory.CreateAppSettingService(
                appErrorMsgProvider, embeddedResourceProvider, appDirService, testAppInfo);
            var result = appDirService.CreateAppDirs();
            Assert.That(result.Excptn, Is.Null, result.Excptn?.Message);
        
            var dbContext = new DbContextFactory().CreateDbContext([]);
            var dbErrorMsgProvider = new DataAccessErrorMsgProvider();
            repository = new Repository<AppDbContext>(dbContext, dbErrorMsgProvider);
            var repositoryHelper = new RepositoryHelper(repository, dbContext);

            result = await repositoryHelper.RebuildRepository(isUseMigrations: true);
            if (result)
                Assert.Pass();

            var resultPost = await repositoryHelper.RebuildRepository(isUseMigrations: false);
            if (resultPost)
                Assert.Inconclusive("Не смогли применить миграции, но БД создана:\n" + result.Excptn?.Message!);
            else
                Assert.Fail(resultPost.Excptn?.Message!);
        }
        finally
        {
            repository?.Dispose();
        }
    }
    
    [Test, Category($"Other")]
    public async Task ReloadRangeTest()
    {
        var dbContext = new DbContextFactory().CreateDbContext([]);
        var dbErrorMsgProvider = new DataAccessErrorMsgProvider();
        var repository = new Repository<AppDbContext>(dbContext,  dbErrorMsgProvider);
        const string description = "Проверка";
    
        var detailedCompetitionStatuses =
            (await repository.GetAllAsync<DetailedCompetitionStatus>()).Value!
                                                                       .Where(dcs => dcs.Id.ToInt() < 10)
                                                                       .ToList();
        
        detailedCompetitionStatuses[0].Description = description;

        var result = await repository.ReloadRangeAsync(detailedCompetitionStatuses);
        Assert.That(detailedCompetitionStatuses.Where(dcs => dcs.Description == description), Is.Empty);
        
        repository.Dispose();
    }
    
    [Test, Category($"Other")]
    public async Task TmpTest()
    {
        var dbContext = new DbContextFactory().CreateDbContext([]);
        var dbErrorMsgProvider = new DataAccessErrorMsgProvider();
        var repository = new Repository<AppDbContext>(dbContext, dbErrorMsgProvider);
        const string description = "Проверка";

        var competitionDataResult =
            (await repository.FindAsync<CompetitionData>(0));
        Assert.That(competitionDataResult.Excptn, Is.Null);
        
        repository.Dispose();
    }

    
    [Test, Order(0)]
    public void CreatingDbContextTest()
    {
        IAppErrorMsgProvider appErrorMsgProvider = new DomainErrorMsgProvider();

        // Создаем сервис директорий приложения
        var appDirService = ServiceFactory.CreateAppDirService(appErrorMsgProvider);
        
        // Создаем конфигуратор БД
        var dbConfigurator = ServiceFactory.CreateDbConfigurator(appDirService);

        // Контекст БД
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        dbConfigurator.UseProvider<AppDbContext>(optionsBuilder);
        var dbContext = new AppDbContext(optionsBuilder.Options, dbConfigurator);
        
        using (Assert.EnterMultipleScope())
        {
            Assert.That(dbContext.IsNullOrEmptyConnectionString, Is.False, "Пустая строка подключения.");
            Assert.That(dbContext.IsPossibleConnect, Is.True, "Невозможно подключиться к БД.");
        }
        
        dbContext.Dispose();
    }

    [Test, Order(0)]
    public void CreatingDbContextTest_DbContextFactory()
    {
        var dbContext = new DbContextFactory().CreateDbContext([]);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(dbContext.IsNullOrEmptyConnectionString, Is.False, "Пустая строка подключения.");
            Assert.That(dbContext.IsPossibleConnect, Is.True, "Невозможно подключиться к БД.");
        }
        
        // Сбрасываем начальные значения автоинкремента 
        // dbContext.ClearAutoincrementSequence(nameof(IAbstractEntity.Id),
        //     typeof(Athlete), typeof(Delegation),
        //     typeof(Representative),
        //     typeof(SportUnit), typeof(SportEvent),
        //     typeof(CompetitionData), typeof(Referee));
        //
        // dbContext.Dispose();
    }
    
    [Test, Order(2)]
    [RecreateEntities]
    public async Task Delegations_Test()
    {
        var dbContext = new DbContextFactory().CreateDbContext([]);
        var dbErrorMsgProvider = new DataAccessErrorMsgProvider();
        var repository = new Repository<AppDbContext>(dbContext,  dbErrorMsgProvider);
        Result<int> result;

        // Удаляем все сущности при атрибуте RecreateEntities
        if (_isRecreateEntities)
        {
            result = repository.RemoveAllQuickly<Delegation>();
            Assert.That(result.Excptn, Is.Null, result.Excptn?.Message);
        }
        
        // Получаем все соревнования
        var competitionResult = await repository.GetAllAsync<CompetitionData>();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(competitionResult.Excptn, Is.Null, competitionResult.Excptn?.Message);
            Assert.That(competitionResult.Value, Is.Not.Null, "Коллекция соревнований пуста.");
        }

        // Перебираем соревнования
        foreach (var cd in competitionResult.Value)
        {
            // Получаем Делегации
            var delegations = await GetDelegations(repository, cd);

            result = repository.UpdateRange(delegations);
            Assert.That(result.Excptn, Is.Null, result.Excptn?.Message);
            result = await repository.SaveChangesAsync();
            Assert.That(result.Excptn, Is.Null, result.Excptn?.Message);

            var allDelegationResult = await repository.GetAllAsync<Delegation>(navigationProperties: nameof(Representative));
            Assert.That(allDelegationResult.Excptn, Is.Null, allDelegationResult.Excptn?.Message);
        }

        repository.Dispose();
    }

    [Test, Order(2)]
    [RecreateEntities]
    public async Task SportEvent_SportUnit_Test()
    {
        var dbContext = new DbContextFactory().CreateDbContext([]);
        var dbErrorMsgProvider = new DataAccessErrorMsgProvider();
        var repository = new Repository<AppDbContext>(dbContext,  dbErrorMsgProvider);
        Result<int> result;

        // Удаляем все сущности при атрибуте RecreateEntities
        if (_isRecreateEntities)
        {
            result = repository.RemoveAllQuickly<SportEvent>();
            Assert.That(result.Excptn, Is.Null, result.Excptn?.Message);
            
            result = repository.RemoveAllQuickly<SportUnit>();
            Assert.That(result.Excptn, Is.Null, result.Excptn?.Message);
        }

        // Получаем все соревнования
        var competitionResult = await repository.GetAllAsync<CompetitionData>();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(competitionResult.Excptn, Is.Null, competitionResult.Excptn?.Message);
            Assert.That(competitionResult.Value, Is.Not.Null, "Коллекция соревнований пуста.");
        }

        // Перебираем соревнования
        foreach (var cd in competitionResult.Value)
        {

            // Получаем Виды программ
            var sportEvents = await GetSportEvents(repository, cd);

            result = repository.UpdateRange(sportEvents);
            Assert.That(result.Excptn, Is.Null, result.Excptn?.Message);
            await repository.SaveChangesAsync();
            Assert.That(result.Excptn, Is.Null, result.Excptn?.Message);

            // Проверка
            var allSportEvents = (await repository.GetAllAsync<SportEvent>()).Value;
            Assert.That(allSportEvents, Is.Not.Empty, "Список видов программ пуст.");


            // Получаем спортивные юниты
            var sportUnits = await GetSportUnits(repository, cd, sportEvents);

            result = repository.UpdateRangeQuickly(sportUnits);
            Assert.That(result.Excptn, Is.Null, result.Excptn?.Message);
            await repository.SaveChangesAsync();
            Assert.That(result.Excptn, Is.Null, result.Excptn?.Message);

            var allSportUnitsResult = await repository.GetAllAsync<SportUnit>();
            Assert.That(allSportUnitsResult.Excptn, Is.Null, allSportUnitsResult.Excptn?.Message);
        }

        repository.Dispose();
    }

    [Test, Order(3)]
    [RecreateEntities]
    public async Task Athletes_Test()
    {
        var dbContext = new DbContextFactory().CreateDbContext([]);
        var dbErrorMsgProvider = new DataAccessErrorMsgProvider();
        var repository = new Repository<AppDbContext>(dbContext,   dbErrorMsgProvider);
        Result<int> result;

        // Удаляем все сущности при атрибуте RecreateEntities
        if (_isRecreateEntities)
        {
            result = repository.RemoveAllQuickly<Athlete>();
            Assert.That(result.Excptn, Is.Null, result.Excptn?.Message);
        }

        var dlg = (await repository.GetAllByNumberAsync<Delegation>(1)).Value!.FirstOrDefault();

        var sportUnit = await repository.GetLastAsync<SportUnit>();

        var sexes = (await repository.GetAllAsync<Sex>()).Value!.ToList();

        // Создаем спортсменов
        var athletes = new List<Athlete>()
        {
            new("Мишенкова", "Ксения", DateTime.Today,
                sexes.Find(s => s.Id == SexEnm.Female)!, dlg!, sportUnit.Value!),
            new("Васильева", "Ольга",  DateTime.Today,
                sexes.Find(s => s.Id == SexEnm.Female)!, dlg!, sportUnit.Value!),
            new("Копылова", "Юлия",  DateTime.Today,
                sexes.Find(s => s.Id == SexEnm.Female)!, dlg!, sportUnit.Value!),
        };

        result = repository.UpdateRangeQuickly(athletes);
        Assert.That(result.Excptn, Is.Null, result.Excptn?.Message);
        
        result = await repository.SaveChangesAsync();
        Assert.That(result.Excptn, Is.Null, result.Excptn?.Message);
        
        var allAthletesResult = await repository.GetAllAsync<SportUnit>();
        Assert.That(result.Excptn, Is.Null, result.Excptn?.Message);
        
        repository.Dispose();
    }

    [Test, Order(2)]
    [RecreateEntities]
    public async Task Referees_Test()
    {
        var dbContext = new DbContextFactory().CreateDbContext([]);
        var dbErrorMsgProvider = new DataAccessErrorMsgProvider();
        var repository = new Repository<AppDbContext>(dbContext,  dbErrorMsgProvider);
        Result<int> result;

        // Удаляем все сущности при атрибуте RecreateEntities
        if (_isRecreateEntities)
        {
            result = repository.RemoveAllQuickly<Referee>();
            Assert.That(result.Excptn, Is.Null, result.Excptn?.Message);
        }

        ExceptionList<BaseException> exceptionsList = [];

        // Получаем все соревнования
        var competitionResult = await repository.GetAllAsync<CompetitionData>();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(competitionResult.Excptn, Is.Null, competitionResult.Excptn?.Message);
            Assert.That(competitionResult.Value, Is.Not.Null, "Коллекция соревнований пуста.");
        }

        // Перебираем соревнования
        foreach (var cd in competitionResult.Value)
        {
            // Получаем список судей
            var referees = (List<Referee>)await GetReferees(repository, cd);
            
            // Добавляем судей в репозиторий
            result = repository.UpdateRange(referees);
            Assert.That(result.Excptn, Is.Null, result.Excptn?.Message);
            result = await repository.SaveChangesAsync();
            Assert.That(result.Excptn, Is.Null, result.Excptn?.Message);
        
            // Проверяем
            var newReferees = (await repository.GetAllAsync<Referee>()).Value;
            Assert.That(result.Excptn, Is.Null, result.Excptn?.Message);
        }
        
        repository.Dispose();
    }

    [Test, Order(1)]
    [RecreateEntities]
    public async Task CompetitionData_Test()
    {
        var dbContext = new DbContextFactory().CreateDbContext([]);
        var dbErrorMsgProvider = new DataAccessErrorMsgProvider();
        var repository = new Repository<AppDbContext>(dbContext, dbErrorMsgProvider);
        IProblemErrorMsgProvider problemErrorMsgProvider = new DomainErrorMsgProvider();
        var competitionService = new CompetitionDataService(repository, problemErrorMsgProvider);
        Result<int> result;
        
        // Удаляем все сущности при атрибуте RecreateEntities
        if (_isRecreateEntities)
        {
            result = repository.RemoveAllQuickly<CompetitionData>();
            Assert.That(result.Excptn, Is.Null, result.Excptn?.Message);
        }

        // Создаем данные о соревновании 1
        var resultCompetition = await competitionService.CreateCompetitionDataAsync(
            "Кубок России по спортивному туризму на горных дистанциях", 
            new List<string> {"ФСТ ЧР", "Авангард"},
            new DateTime(2023, 2, 23), new DateTime(2023, 2, 26),
            "Овраг близ этнокомплекса \"Амазония\", г. Чебоксары", "Горные соревы",
            (await repository.FindAsync<CompetitionsStatus>(CompetitionsStatusEnm.AllRussian)).Value!,
            (await repository.FindAsync<DetailedCompetitionStatus>(DetailedCompetitionStatusEnm.RussianCup)).Value!
            );
        Assert.That(resultCompetition.Excptn, Is.Null, resultCompetition.Excptn?.Message);
        
        // Создаем данные о соревновании 2
        resultCompetition = await competitionService.CreateCompetitionDataAsync(
            "Чемпионат Чувашской Республики по спортивному туризму на горных дистанциях", 
            new List<string> {"ФСТ ЧР", "Авангард", "Администрация Ядринского МО"},
            new DateTime(2026, 4, 10), new DateTime(2026, 4, 12),
            "р. Юнга, Ядринский МО", "Водные соревы",
            (await repository.FindAsync<CompetitionsStatus>(CompetitionsStatusEnm.Regional)).Value!,
            (await repository.FindAsync<DetailedCompetitionStatus>(DetailedCompetitionStatusEnm.RegionalChampionship)).Value!
        );
        Assert.That(resultCompetition.Excptn, Is.Null, resultCompetition.Excptn?.Message);

        // Проверяем
        var competitionsResult = await repository.GetAllAsync<CompetitionData>();
        Assert.That(competitionsResult.Excptn, Is.Null,  competitionsResult.Excptn?.Message);
        
        repository.Dispose();
    }
}