using AppDomain.Setting.Services;
using AppDomain.UseCases._Contracts;
using AppDomain.UseCases.Services;
using Common.BaseComponents.Components;
using Common.BaseComponents.Components.Exceptions;
using Common.BaseExtensions.Collections;
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
    protected const string NoAutomatic = nameof(NoAutomatic);
    
    [SetUp]
    public void Setup()
    {
        // Игнорируем тест с аттрибутом NoAutomatic
        var testCategory = (string)TestContext.CurrentContext.Test.Properties["Category"].FirstOrDefault()!;
        if (testCategory == NoAutomatic)
            Assert.Inconclusive();
    }
    
    #region [---------- Вспомогательные методы ----------]
    
    // Создать или получить Представителя
    private async Task<Representative> GetRepresentative(IRepository repository)
    {
        var representative = new Representative("Банько", "Яна", "Евгеньевна");

        var foundRepresentative = // получаем из репозитория
            (await repository.GetAllAsync<Representative>(representative)).FirstOrDefault();
        if (foundRepresentative is not null)
            representative.Copy(foundRepresentative);
        
        var result = await repository.AddOrUpdateAsync(foundRepresentative ?? representative);
        Assert.That(result.Excptn, Is.Null);

        return foundRepresentative ?? representative;
    }
    
    #endregion

    /// <summary>
    /// Полностью пересоздать БД.
    /// </summary>
    [Test, Category($"{NoAutomatic}")]
    public async Task RebuildDbTest()
    {
        var dbContext = new DbContextFactory().CreateDbContext([]);
        var repository = new Repository<AppDbContext>(dbContext);

        await repository.RebuildRepository();
        
        repository.Dispose();
    }
    
    [Test, Category($"Other")]
    public async Task ReloadRangeTest()
    {
        var dbContext = new DbContextFactory().CreateDbContext([]);
        var repository = new Repository<AppDbContext>(dbContext);
        const string description = "Проверка";

        var detailedCompetitionStatuses =
            (await repository.GetAllAsync<DetailedCompetitionStatus>())
            .Where(dcs => dcs.Id.ToInt() < 10)
            .ToList();
        
        detailedCompetitionStatuses[0].Description = description;
        
        detailedCompetitionStatuses = 
            (await repository.ReloadRangeAsync(detailedCompetitionStatuses)).ToList();
        Assert.That(
            detailedCompetitionStatuses.Where(dcs => dcs.Description == description), 
            Is.Empty);
        
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
        Assert.Multiple(() =>
        {
            Assert.That(dbContext.IsNullOrEmptyConnectionString, Is.False, "Пустая строка подключения.");
            Assert.That(dbContext.IsPossibleConnect, Is.True, "Невозможно подключиться к БД.");
        });
        
        dbContext.Dispose();
    }

    [Test, Order(0)]
    public void CreatingDbContextTest_DbContextFactory()
    {
        var dbContext = new DbContextFactory().CreateDbContext([]);
        Assert.Multiple(() =>
        {
            Assert.That(dbContext.IsNullOrEmptyConnectionString, Is.False, "Пустая строка подключения.");
            Assert.That(dbContext.IsPossibleConnect, Is.True, "Невозможно подключиться к БД.");
        });
        
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
    public async Task Delegations_Test()
    {
        var dbContext = new DbContextFactory().CreateDbContext([]);
        var repository = new Repository<AppDbContext>(dbContext);

        // Представитель
        var representative = await GetRepresentative(repository);
        
        // Новые Делегации
        var delegations = new List<Delegation>()
        {
            new(1, name: "Делегация 11111", "Якутия", representative),
            new(2, name: "Делегация 22222", "Сахалин", representative),
        };

        var result = await repository.ReplaceNumberedRangeAsync(delegations);
        Assert.That(result.Excptn, Is.Null);

        var all = await repository.GetAllAsync<Delegation>(nameof(Representative));
        Assert.That(all.Count, Is.Positive);
        
        repository.Dispose();
    }

    [Test, Order(2)]
    public async Task SportEvent_SportUnit_Test()
    {
        var dbContext = new DbContextFactory().CreateDbContext([]);
        var repository = new Repository<AppDbContext>(dbContext);
        Result<int> result;

        var discipline = await repository.FindAsync<Discipline>(DisciplineEnm.DistanceMountainBunch);

        var sportUnitTypes = (await repository.GetAllAsync<SportUnitType>()).AsList();
        var groupUnitType = sportUnitTypes.FirstOrDefault(sut => sut.Id == SportUnitTypeEnm.Group);
        var waterTeamUnitType = sportUnitTypes.FirstOrDefault(sut => sut.Id == SportUnitTypeEnm.WaterTeam);
        
        // Создаем Виды программ
        var sportEvents = new List<SportEvent>()
        {
            new("Вид программы 1", null,Difficulty.IdEnm.Fourth, discipline!),
            new("Вид программы 2", null,Difficulty.IdEnm.Third, discipline!),
        };
        
        var resultSportEvents = await repository.ReplaceNamedRangeAsync(sportEvents);
        Assert.That(resultSportEvents.Excptn, Is.Null);

        // -------------------------------

        var sexes = (await repository.GetAllAsync<Sex>()).ToList();

        // Создаем спортивные юниты
        var sportUnits = new List<SportUnit>()
        {
            new("Группа 1", sexes.Find(s => s.Id == SexEnm.Male)!, 
                groupUnitType!, resultSportEvents.Value[0]),
            new("Группа 2", sexes.Find(s => s.Id == SexEnm.Female)!, 
                groupUnitType!, resultSportEvents.Value[0]),
            new("Группа судов 1", sexes.Find(s => s.Id == SexEnm.Mixed)!,
                waterTeamUnitType!, resultSportEvents.Value[1]),
        };
        sportUnits[2].ChildSportUnits.Add(sportUnits[1]);   // юнит3 -> юнит2

        var resultSportUnits = await repository.ReplaceNamedRangeAsync(sportUnits);
        Assert.That(resultSportUnits.Excptn, Is.Null);

        repository.Dispose();
    }

    [Test, Order(3)]
    public async Task Athletes_Test()
    {
        var dbContext = new DbContextFactory().CreateDbContext([]);
        var repository = new Repository<AppDbContext>(dbContext);

        var dlg = (await repository.GetAllFromNumberAsync<Delegation>(1)).FirstOrDefault();

        var sportUnit = await repository.GetLastAsync<SportUnit>();

        var sexes = (await repository.GetAllAsync<Sex>()).ToList();

        // Создаем спортсменов
        var athletes = new List<Athlete>()
        {
            new("Мишенкова", "Ксения", DateTime.Today,
                sexes.Find(s => s.Id == SexEnm.Female)!, dlg!, sportUnit!),
            new("Васильева", "Ольга",  DateTime.Today,
                sexes.Find(s => s.Id == SexEnm.Female)!, dlg!, sportUnit!),
            new("Копылова", "Юлия",  DateTime.Today,
                sexes.Find(s => s.Id == SexEnm.Female)!, dlg!, sportUnit!),
        };

        var result = await repository.ReplaceNamedRangeAsync(athletes);
        Assert.That(result.Excptn, Is.Null);
        
        repository.Dispose();
    }

    [Test, Order(4)]
    public async Task Referees_Test()
    {
        var dbContext = new DbContextFactory().CreateDbContext([]);
        var repository = new Repository<AppDbContext>(dbContext);
        
        Result<int> result;
        List<Referee> referees = [];
        ExceptionList<BaseException> exceptionsList = [];

        var jobTitle = // должность
            RefereeJobTitleEnm.ChiefReferee;
        referees.Add(new Referee(1,
            "Сретенский", "Сергей", 
            "г. Чебоксары", 
            (await repository.FindAsync<RefereeLevel>(RefereeLevelEnm.Category1))!,
            (await repository.FindAsync<RefereeJobTitle>(jobTitle))!,
            "Валентинович"));
        
        jobTitle = RefereeJobTitleEnm.ChiefSecretary;
        referees.Add(new Referee(2,
            "Иванова", "Кристина", 
            "г. Чебоксары", 
            (await repository.FindAsync<RefereeLevel>(RefereeLevelEnm.Category1))!,
            (await repository.FindAsync<RefereeJobTitle>(jobTitle))!));
        
        jobTitle = RefereeJobTitleEnm.MandateChairman;
        referees.Add(new Referee(3,
            "Черкасова", "Маргарита", 
            "г. Санкт-Петербург", 
            (await repository.FindAsync<RefereeLevel>(RefereeLevelEnm.AllRussCategory))!,
            (await repository.FindAsync<RefereeJobTitle>(jobTitle))!));
        
        jobTitle = RefereeJobTitleEnm.Secretary;
        referees.Add(new Referee(4,
            "Тетка", "1", 
            "г. Чебоксары", 
            (await repository.FindAsync<RefereeLevel>(RefereeLevelEnm.Category2))!,
            (await repository.FindAsync<RefereeJobTitle>(jobTitle))!));
        
        jobTitle = RefereeJobTitleEnm.MajorStageReferee;
        referees.Add(new Referee(5,
            "Тетка", "2", 
            "г. Чебоксары", 
            (await repository.FindAsync<RefereeLevel>(RefereeLevelEnm.Category3))!,
            (await repository.FindAsync<RefereeJobTitle>(jobTitle))!));

        // Добавляем судей в репозиторий
        var resultReferees = await repository.ReplaceNamedRangeAsync(referees);
        Assert.That(resultReferees.Excptn, Is.Null);
        
        // Проверяем
        referees = (await repository.GetAllAsync<Referee>()).ToList();
        Assert.That(referees, Is.Not.Empty);
        
        repository.Dispose();
    }

    [Test, Order(5)]
    public async Task CompetitionData_Test()
    {
        var dbContext = new DbContextFactory().CreateDbContext([]);
        var repository = new Repository<AppDbContext>(dbContext);
        var competitionService = new CompetitionDataService(repository);

        // Создаем данные о соревновании
        var resultCompetition = await competitionService.CreateAsync("Горные соревы...", "ФСТ ЧР",
            new DateTime(2023, 2, 23), new DateTime(2023, 2, 26),
            "Овраг близ этнокомплекса \"Амазония\", г. Чебоксары", 
            (await repository.FindAsync<CompetitionsStatus>(CompetitionsStatusEnm.Regional))!,
            (await repository.FindAsync<DetailedCompetitionStatus>(DetailedCompetitionStatusEnm.RegionalChampionship))!);
        Assert.That(resultCompetition.Excptn, Is.Null);
        
        // Проверяем
        var competitions = await repository.GetAllAsync<CompetitionData>();
        Assert.That(competitions, Is.Not.Empty);
    }
}