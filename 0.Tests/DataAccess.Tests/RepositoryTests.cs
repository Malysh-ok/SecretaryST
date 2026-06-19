using System.Reflection;
using AppDomain.Setting.Services;
using AppDomain.UseCases._Contracts;
using AppDomain.UseCases.Services;
using Common.BaseComponents.Components;
using Common.BaseComponents.Components.Exceptions;
using Common.BaseExtensions.ValueTypes;
using DataAccess.DbContexts;
using DataAccess.DbContexts.DbConfigure;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Presentation.Shell;
using ProblemDomain.Entities.CommonEntities;
using ProblemDomain.Entities.DistanceEntities;
using ProblemDomain.Entities.LibraryEntities;
using ProblemDomain.Entities.LibraryEntities.Enums;

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
        // var representative = new Representative("Банько", "Яна", "Евгеньевна");
        var representative = new Representative("Шлепко", "Яна", "Ульяновна");

        // Заменяем первого найденного в базе представителя на нового (для примера)
        var foundRepresentative = // получаем из репозитория
            (await repository.GetFirstAsync<Representative>()).Value;
        if (foundRepresentative is not null)
            representative.Copy(foundRepresentative);
        var result = repository.Update(foundRepresentative ?? representative);
        Assert.That(result.Excptn, Is.Null);
        
        result = await repository.SaveChangesAsync();
        Assert.That(result.Excptn, Is.Null);

        return foundRepresentative ?? representative;
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
        var dbContext = new DbContextFactory().CreateDbContext([]);
        var repository = new Repository<AppDbContext>(dbContext);
        var initRepository = new InitRepository(repository);

        await initRepository.RebuildRepository();
        
        repository.Dispose();
    }
    
    [Test, Category($"Other")]
    public async Task ReloadRangeTest()
    {
        var dbContext = new DbContextFactory().CreateDbContext([]);
        var repository = new Repository<AppDbContext>(dbContext);
        const string description = "Проверка";
    
        var detailedCompetitionStatuses =
            (await repository.GetAllAsync<DetailedCompetitionStatus>()).Value!
                                                                       .Where(dcs => dcs.Id.ToInt() < 10)
                                                                       .ToList();
        
        detailedCompetitionStatuses[0].Description = description;

        var result =
            (await repository.ReloadRangeAsync(detailedCompetitionStatuses));
        Assert.That(
            detailedCompetitionStatuses.Where(dcs => dcs.Description == description), 
            Is.Empty);
        
        repository.Dispose();
    }
    
    [Test, Category($"Other")]
    public async Task TmpTest()
    {
        var dbContext = new DbContextFactory().CreateDbContext([]);
        var repository = new Repository<AppDbContext>(dbContext);
        const string description = "Проверка";

        var competitionDataResult =
            (await repository.FindAsync<CompetitionData>(0));

        Assert.That(competitionDataResult.Excptn, Is.Null);
        
        repository.Dispose();
    }

    
    [Test, Order(0)]
    public void CreatingDbContextTest()
    {
        // Создаем сервис настроек приложения
        var appSettingService = new AppSettingService();
        
        // Создаем конфигурацию, наполняем ее данными из файла конфигурации
        var configuration = new ConfigurationManager();
        configuration.AddXmlFile(appSettingService.SettingFilePath, optional: true).Build();

        // Создаем конфигуратор БД
        var dbConfigurator = new DbConfigurator(configuration, appSettingService.AppDir.DatabasePath);

        var provider = new DbContextOptionsBuilder<AppDbContext>();
        
        // Контекст БД
        var dbContext = new AppDbContext(provider.Options, dbConfigurator);
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
    
    [Test, Order(1)]
    [RecreateEntities]
    public async Task Delegations_Test()
    {
        var dbContext = new DbContextFactory().CreateDbContext([]);
        var repository = new Repository<AppDbContext>(dbContext);
        Result<int> result;

        // Удаляем все сущности при атрибуте RecreateEntities
        if (_isRecreateEntities)
        {
            result = repository.RemoveAllQuickly<Delegation>();
            Assert.That(result.Excptn, Is.Null);
        }

        // Представитель
        var representative = await GetRepresentative(repository);
        
        // Новые Делегации
        var delegations = new List<Delegation>()
        {
            new(1, name: "Делегация 11111", "Якутия", representative),
            new(2, name: "Делегация 22222", "Сахалин", representative),
            new(3, name: "Делегация 33333", "Камчатка", representative),
        };

        result = repository.UpdateRangeQuickly(delegations);
        Assert.That(result.Excptn, Is.Null);
        
        result = await repository.SaveChangesAsync();
        Assert.That(result.Excptn, Is.Null);

        var allDelegationResult = await repository.GetAllAsync<Delegation>(nameof(Representative));
        Assert.That(allDelegationResult.Excptn, Is.Null);
        
        repository.Dispose();
    }

    [Test, Order(2)]
    [RecreateEntities]
    public async Task SportEvent_SportUnit_Test()
    {
        var dbContext = new DbContextFactory().CreateDbContext([]);
        var repository = new Repository<AppDbContext>(dbContext);
        Result<int> result;

        // Удаляем все сущности при атрибуте RecreateEntities
        if (_isRecreateEntities)
        {
            result = repository.RemoveAllQuickly<SportEvent>();
            Assert.That(result.Excptn, Is.Null);
            
            result = repository.RemoveAllQuickly<SportUnit>();
            Assert.That(result.Excptn, Is.Null);
        }

        var disciplineResult = await repository.FindAsync<Discipline>(DisciplineEnm.DistanceMountainBunch);

        var sportUnitTypes = (await repository.GetAllAsync<SportUnitType>()).Value!;
        var groupUnitType = sportUnitTypes.FirstOrDefault(sut => sut.Id == SportUnitTypeEnm.Group);
        var waterTeamUnitType = sportUnitTypes.FirstOrDefault(sut => sut.Id == SportUnitTypeEnm.WaterTeam);
        
        // Создаем Виды программ
        var sportEvents = new List<SportEvent>()
        {
            new("Вид программы 1", null,Difficulty.IdEnm.Fourth, disciplineResult.Value!),
            new("Вид программы 2", null,Difficulty.IdEnm.Third, disciplineResult.Value!),
        };
        
        result = repository.UpdateRangeQuickly(sportEvents);
        Assert.That(result.Excptn, Is.Null);
        await repository.SaveChangesAsync();
        Assert.That(result.Excptn, Is.Null);
        var allSportEvents = (await repository.GetAllAsync<SportEvent>()).Value;
        Assert.That(allSportEvents, Is.Not.Empty);

        // -------------------------------

        var sexes = (await repository.GetAllAsync<Sex>()).Value!.ToList();

        // Создаем спортивные юниты
        var sportUnits = new List<SportUnit>()
        {
            new("Группа 1", sexes.Find(s => s.Id == SexEnm.Male)!, 
                groupUnitType!, allSportEvents[0]),
            new("Группа 2", sexes.Find(s => s.Id == SexEnm.Female)!, 
                groupUnitType!, allSportEvents[0]),
            new("Группа судов 1", sexes.Find(s => s.Id == SexEnm.Mixed)!,
                waterTeamUnitType!, allSportEvents[1]),
        };
        sportUnits[2].ChildSportUnits.Add(sportUnits[1]);   // юнит3 -> юнит2

        result = repository.UpdateRangeQuickly(sportUnits);
        Assert.That(result.Excptn, Is.Null);
        
        await repository.SaveChangesAsync();
        Assert.That(result.Excptn, Is.Null);
        
        var allSportUnitsResult = await repository.GetAllAsync<SportUnit>();
        Assert.That(allSportUnitsResult.Excptn, Is.Null);

        repository.Dispose();
    }

    [Test, Order(3)]
    [RecreateEntities]
    public async Task Athletes_Test()
    {
        var dbContext = new DbContextFactory().CreateDbContext([]);
        var repository = new Repository<AppDbContext>(dbContext);
        Result<int> result;

        // Удаляем все сущности при атрибуте RecreateEntities
        if (_isRecreateEntities)
        {
            result = repository.RemoveAllQuickly<Athlete>();
            Assert.That(result.Excptn, Is.Null);
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
        Assert.That(result.Excptn, Is.Null);
        
        result = await repository.SaveChangesAsync();
        Assert.That(result.Excptn, Is.Null);
        
        var allAthletesResult = await repository.GetAllAsync<SportUnit>();
        Assert.That(allAthletesResult.Excptn, Is.Null);
        
        repository.Dispose();
    }

    [Test, Order(4)]
    [RecreateEntities]
    public async Task Referees_Test()
    {
        var dbContext = new DbContextFactory().CreateDbContext([]);
        var repository = new Repository<AppDbContext>(dbContext);
        Result<int> result;

        // Удаляем все сущности при атрибуте RecreateEntities
        if (_isRecreateEntities)
        {
            result = repository.RemoveAllQuickly<Referee>();
            Assert.That(result.Excptn, Is.Null);
        }
        
        List<Referee> referees = [];
        ExceptionList<BaseException> exceptionsList = [];

        var jobTitle = // должность
            RefereeJobTitleEnm.ChiefReferee;
        referees.Add(new Referee(1,
            "Сретенский", "Сергей", 
            "г. Чебоксары", 
            (await repository.FindAsync<RefereeLevel>(RefereeLevelEnm.Category1)).Value!,
            (await repository.FindAsync<RefereeJobTitle>(jobTitle)).Value!,
            "Валентинович"
            )
        );
        
        jobTitle = RefereeJobTitleEnm.ChiefSecretary;
        referees.Add(new Referee(2,
            "Иванова", "Кристина", 
            "г. Чебоксары", 
            (await repository.FindAsync<RefereeLevel>(RefereeLevelEnm.Category1)).Value!,
            (await repository.FindAsync<RefereeJobTitle>(jobTitle)).Value!
            )
        );
        
        jobTitle = RefereeJobTitleEnm.MandateChairman;
        referees.Add(new Referee(3,
            "Черкасова", "Маргарита", 
            "г. Санкт-Петербург", 
            (await repository.FindAsync<RefereeLevel>(RefereeLevelEnm.AllRussCategory)).Value!,
            (await repository.FindAsync<RefereeJobTitle>(jobTitle)).Value!
            )
        );
        
        jobTitle = RefereeJobTitleEnm.Secretary;
        referees.Add(new Referee(4,
            "Тетка", "1", 
            "г. Чебоксары", 
            (await repository.FindAsync<RefereeLevel>(RefereeLevelEnm.Category2)).Value!,
            (await repository.FindAsync<RefereeJobTitle>(jobTitle)).Value!
            )
        );
        
        jobTitle = RefereeJobTitleEnm.MajorStageReferee;
        referees.Add(new Referee(5,
            "Тетка", "2", 
            "г. Чебоксары", 
            (await repository.FindAsync<RefereeLevel>(RefereeLevelEnm.Category3)).Value!,
            (await repository.FindAsync<RefereeJobTitle>(jobTitle)).Value!
            )
        );

        // Добавляем судей в репозиторий
        result = repository.UpdateRangeQuickly(referees);
        Assert.That(result.Excptn, Is.Null);
        await repository.SaveChangesAsync();
        Assert.That(result.Excptn, Is.Null);
        
        // Проверяем
        var newReferees = (await repository.GetAllAsync<Referee>()).Value;
        Assert.That(newReferees, Is.Not.Empty);
        
        repository.Dispose();
    }

    [Test, Order(5)]
    [RecreateEntities]
    public async Task CompetitionData_Test()
    {
        var dbContext = new DbContextFactory().CreateDbContext([]);
        var repository = new Repository<AppDbContext>(dbContext);
        var competitionService = new CompetitionDataService(repository);
        Result<int> result;
        
        // Удаляем все сущности при атрибуте RecreateEntities
        if (_isRecreateEntities)
        {
            result = repository.RemoveAllQuickly<CompetitionData>();
            Assert.That(result.Excptn, Is.Null);
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
        Assert.That(resultCompetition.Excptn, Is.Null);
        
        // Создаем данные о соревновании 2
        resultCompetition = await competitionService.CreateCompetitionDataAsync(
            "Чемпионат Чувашской Республики по спортивному туризму на горных дистанциях", 
            new List<string> {"ФСТ ЧР", "Авангард", "Администрация Ядринского МО"},
            new DateTime(2026, 4, 10), new DateTime(2026, 4, 12),
            "р. Юнга, Ядринский МО", "Водные соревы",
            (await repository.FindAsync<CompetitionsStatus>(CompetitionsStatusEnm.Regional)).Value!,
            (await repository.FindAsync<DetailedCompetitionStatus>(DetailedCompetitionStatusEnm.RegionalChampionship)).Value!
        );
        Assert.That(resultCompetition.Excptn, Is.Null);

        // Проверяем
        var competitionsResult = await repository.GetAllAsync<CompetitionData>();
        Assert.That(competitionsResult.Excptn, Is.Null);
        
        repository.Dispose();
    }
}