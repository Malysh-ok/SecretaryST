using AppDomain.Setting.Services;
using AppDomain.UseCases._Contracts;
using Common.BaseComponents.Components;
using Common.BaseComponents.Components.Exceptions;
using Common.BaseExtensions.ValueTypes;
using DataAccess.DbContexts;
using DataAccess.DbContexts.DbConfigure;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Presentation.Shell;
using ProblemDomain.Entities;
using ProblemDomain.Entities.CommonEntities;
using ProblemDomain.Entities.DistanceEntities;
using ProblemDomain.Entities.LibraryEntities;
using ProblemDomain.Entities.LibraryEntities.Enums;
using ProblemDomain.Services;

namespace DataAccess.Tests;

public class DbContextTests
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
        
        Result<int> result;
        var foundRepresentative = // получаем из репозитория
            (await repository.GetAllAsync<Representative>(representative)).FirstOrDefault();
        
        if (foundRepresentative is null)
            result = await repository.AddAsync(representative);
        else
        {
            representative.Copy(foundRepresentative);
            result = await repository.UpdateAsync(foundRepresentative);
        }
        
        Assert.That(result.Excptn, Is.Null);

        return foundRepresentative ?? representative;
    }
    
    #endregion

    [Test, Category($"{NoAutomatic}")]
    public async Task RebuildDbTest()
    {
        var dbContext = new DbContextFactory().CreateDbContext([]);
        var repository = new Repository<AppDbContext>(dbContext);

        await repository.RebuildRepository();
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
        var result = Result<int>.Done(0);

        // Представитель
        // var representative = await GetRepresentative(dbContext);
        var representative = await GetRepresentative(repository);
        
        // Делегации
        var delegations = new List<Delegation>()
        {
            new(1, name: "Делегация 11111", "Якутия", representative),
            new(2, name: "Делегация 22222", "Сахалин", representative),
        };
        
        delegations.ForEach(async d =>
        {
            var foundDelegation = (await repository.GetAllFromNumberAsync<Delegation>(d.Number)).FirstOrDefault();
            if (foundDelegation is null)
                result =await repository.AddAsync(d);
            else
            {
                d.Copy(foundDelegation);
                result =await repository.UpdateAsync(foundDelegation);
            }
            
            Assert.That(result.Excptn, Is.Null);
        });

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

        var discipline = await repository.GetFromIdAsync<Discipline>(DisciplineEnm.DistanceMountainBunch.ToInt());

        var sportUnitType = await repository.GetFromIdAsync<SportUnitType>(SportUnitTypeEnm.Group.ToInt());
        
        var dlg = (await repository.GetAllFromNumberAsync<Delegation>(1)).FirstOrDefault();
        // var representative = repository.GetFromId<Representative>(1);
        // var dlg = GetDelegation(repository, representative);

        // Создаем Вид программы
        const string sportEventName = "Вид программы 1";
        var sportEvent = new SportEvent(sportEventName, null,
            Difficulty.IdEnm.Fourth, discipline!);

        var foundSportEvent = // получаем из репозитория
            (await repository.GetAllFromNameAsync<SportEvent>(sportEventName)).FirstOrDefault();
        if (foundSportEvent is null)
            result = await repository.AddAsync(sportEvent);
        else
        {
            sportEvent.Copy(foundSportEvent);
            result = await repository.UpdateAsync(foundSportEvent);
        }
        
        Assert.That(result.Excptn, Is.Null);

        var sexes = (await repository.GetAllAsync<Sex>()).ToList();

        // Создаем спортивные юниты
        var sportUnits = new List<SportUnit>()
        {
            new("Группа 1", sexes.Find(s => s.Id == SexEnm.Male), 
                sportUnitType, sportEvent),
            new("Группа 2", sexes.Find(s => s.Id == SexEnm.Female), 
                sportUnitType, sportEvent),
            new("Группа судов 1", sexes.Find(s => s.Id == SexEnm.Mixed),
                await repository.GetFromIdAsync<SportUnitType>(SportUnitTypeEnm.WaterTeam.ToInt()), sportEvent),
        };
        sportUnits[2].ChildSportUnits.Add(sportUnits[1]);   // юнит3 -> юнит2
        
        sportUnits.ForEach(async su =>
        {
            var foundSportUnit = (await repository.GetAllFromNameAsync<SportUnit>(su.Name)).FirstOrDefault();
            if (foundSportUnit is null)
                result = await repository.AddAsync(su);
            else
            {
                su.Copy(foundSportUnit);
                result = await repository.UpdateAsync(foundSportUnit);
            }
            
            Assert.That(result.Excptn, Is.Null);
        });

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
                sexes.Find(s => s.Id == SexEnm.Female), dlg, sportUnit),
            new("Васильева", "Ольга",  DateTime.Today,
                sexes.Find(s => s.Id == SexEnm.Female), dlg, sportUnit),
            new("Копылова", "Юлия",  DateTime.Today,
                sexes.Find(s => s.Id == SexEnm.Female), dlg, sportUnit),
        };

        foreach (var athlete in athletes)
        {
            var foundAthlete = (await repository.GetAllAsync<Athlete>(athlete)).FirstOrDefault();
            Result<int> result;
            if (foundAthlete is null)
                result = await repository.AddAsync(athlete);
            else
            {
                athlete.Copy(foundAthlete);
                result = await repository.UpdateAsync(foundAthlete);
            }
            
            Assert.That(result.Excptn, Is.Null);  
        }
        
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
        referees.Add(new Referee(jobTitle,
            "Сретенский", "Сергей", 
            "г. Чебоксары", 
            (await repository.GetFromIdAsync<RefereeLevel>(RefereeLevelEnm.Category1.ToInt()))!,
            (await repository.GetFromIdAsync<RefereeJobTitle>(jobTitle.ToInt()))!,
            "Валентинович"));
        
        jobTitle = RefereeJobTitleEnm.ChiefSecretary;
        referees.Add(new Referee(jobTitle,
            "Иванова", "Кристина", 
            "г. Чебоксары", 
            (await repository.GetFromIdAsync<RefereeLevel>(RefereeLevelEnm.Category1.ToInt()))!,
            (await repository.GetFromIdAsync<RefereeJobTitle>(jobTitle.ToInt()))!));
        
        jobTitle = RefereeJobTitleEnm.MandateChairman;
        referees.Add(new Referee(jobTitle,
            "Черкасова", "Маргарита", 
            "г. Санкт-Петербург", 
            (await repository.GetFromIdAsync<RefereeLevel>(RefereeLevelEnm.AllRussCategory.ToInt()))!,
            (await repository.GetFromIdAsync<RefereeJobTitle>(jobTitle.ToInt()))!));
        
        jobTitle = RefereeJobTitleEnm.Secretary;
        referees.Add(new Referee(jobTitle,
            "Тетка", "1", 
            "г. Чебоксары", 
            (await repository.GetFromIdAsync<RefereeLevel>(RefereeLevelEnm.Category2.ToInt()))!,
            (await repository.GetFromIdAsync<RefereeJobTitle>(jobTitle.ToInt()))!));
        
        jobTitle = RefereeJobTitleEnm.MajorStageReferee;
        referees.Add(new Referee(jobTitle,
            "Тетка", "2", 
            "г. Чебоксары", 
            (await repository.GetFromIdAsync<RefereeLevel>(RefereeLevelEnm.Category3.ToInt()))!,
            (await repository.GetFromIdAsync<RefereeJobTitle>(jobTitle.ToInt()))!));

        // Добавляем судей в репозиторий
        foreach (var referee in referees)
        {
            var foundReferee = // получаем из репозитория
                (await repository.GetFromIdAsync<Referee>((int)referee.Id));

            if (foundReferee is null)
                result = await repository.AddAsync(referee);
            else
            {
                referee.Copy(foundReferee);
                result = await repository.UpdateAsync(foundReferee);
            }
            
            if (! result)
                exceptionsList.Add(new BaseException(
                    $"Failed to create or get the referee '{referee.Id}' from the repository.", result.Excptn, 
                    "ru", $"Не удалось создать или получить судью '{referee.Id}' из репозитория."));
        }        
        
        Assert.That(exceptionsList.GetAll(), Is.Empty, exceptionsList.GetAllMassage());
        
        referees = (await repository.GetAllAsync<Referee>()).ToList();

        Assert.That(referees.Count(), Is.Positive);
        
        repository.Dispose();
    }

    [Test, Order(5)]
    public async Task CompetitionData_Test()
    {
        var dbContext = new DbContextFactory().CreateDbContext([]);
        var repository = new Repository<AppDbContext>(dbContext);
        var competitionService = new CompetitionDataService();

        var resultCompetition = await competitionService.CreateAsync(repository, "Горные соревы...", "ФСТ ЧР",
            new DateTime(2023, 2, 23), 4,
            "Овраг близ этнокомплекса \"Амазония\", г. Чебоксары", 
            (await repository.GetFromIdAsync<CompetitionsStatus>(CompetitionsStatusEnm.Regional.ToInt()))!,
            (await repository.GetFromIdAsync<DetailedCompetitionStatus>(DetailedCompetitionStatusEnm.RegionalChampionship.ToInt()))!);

        Assert.That(resultCompetition.Excptn, Is.Null);
        
        var competitions = await repository.GetAllAsync<CompetitionData>();
        
        Assert.That(competitions.Count(), Is.Positive);
    }

}