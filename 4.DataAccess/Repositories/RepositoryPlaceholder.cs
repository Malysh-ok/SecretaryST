using Common.BaseComponents.Components;
using Common.BaseExtensions.ValueTypes;
using ProblemDomain.Entities.LibraryEntities;
using ProblemDomain.Entities.LibraryEntities.Enums;
using ProblemDomain.UseCases._Contracts;

namespace DataAccess.Repositories;

/// <summary>
/// Заполнитель библиотечной части репозитория (БД).
/// </summary>
public static class RepositoryPlaceholder
{
    /// <summary>
    /// Заполняем статусы соревнований.
    /// </summary>
    public static async Task<Result<List<CompetitionsStatus>>> FillCompetitionsStatuses(IRepository repository)
    {
        // Удаляем статусы соревнований
        var result = repository.RemoveAllQuickly<CompetitionsStatus>();
        if (!result)
            return Result<List<CompetitionsStatus>>.Fail(result.Excptn!);
        
        var competitionsStatusLst = new List<CompetitionsStatus>
        {
            new(CompetitionsStatusEnm.AllRussian, "Всероссийский", "Всероссийские"),
            new(CompetitionsStatusEnm.Interregional, "Межрегиональный", "Межрегиональные"),
            new(CompetitionsStatusEnm.Regional, "Региональный", "Региональные"),
            new(CompetitionsStatusEnm.Municipal, "Муниципальный", "Муниципальные"),
        };
        result = repository.AddRangeQuickly(competitionsStatusLst);
        if (!result)
            return Result<List<CompetitionsStatus>>.Fail(result.Excptn!);

        result = await repository.SaveChangesAsync();
        if (!result)
            return Result<List<CompetitionsStatus>>.Fail(result.Excptn!);

        return Result<List<CompetitionsStatus>>.Done(competitionsStatusLst);
    }

    /// <summary>
    /// Заполняем статусы и наименования спортивных соревнований.
    /// </summary>
    public static async Task<Result<List<DetailedCompetitionStatus>>> FillDetailedCompetitionStatuses(IRepository repository,
        List<CompetitionsStatus> competitionsStatusLst)
    {
        // Удаляем статусы и наименования соревнований
        var result = repository.RemoveAllQuickly<DetailedCompetitionStatus>();
        if (!result)
            return Result<List<DetailedCompetitionStatus>>.Fail(result.Excptn!);

        var detailedCompetitionsStatusLst = new List<DetailedCompetitionStatus>
        {
            new(DetailedCompetitionStatusEnm.RussianChampionship, "Чемпионат России",
                competitionsStatusLst.Find(dg => dg.Id == CompetitionsStatusEnm.AllRussian)!),
            new(DetailedCompetitionStatusEnm.RussianCup, "Кубок России",
                competitionsStatusLst.Find(dg => dg.Id == CompetitionsStatusEnm.AllRussian)!),
            new(DetailedCompetitionStatusEnm.RussianJuniorChampionship, "Первенство России",
                competitionsStatusLst.Find(dg => dg.Id == CompetitionsStatusEnm.AllRussian)!),
            new(DetailedCompetitionStatusEnm.OtherRussianCompetition, 
                "Другие официальные всероссийские спортивные соревнования",
                competitionsStatusLst.Find(dg => dg.Id == CompetitionsStatusEnm.AllRussian)!),
            
            new(DetailedCompetitionStatusEnm.FederalDistrictChampionship, 
                "Чемпионат федерального округа, двух и более федеральных округов, " +
                "чемпионаты г. Москва и г. Санкт-Петербурга",
                competitionsStatusLst.Find(dg => dg.Id == CompetitionsStatusEnm.Interregional)!),
            new(DetailedCompetitionStatusEnm.FederalDistrictJuniorChampionship, 
                "Первенство федерального округа, двух и более федеральных округов, " +
                "первенства г. Москва и г. Санкт-Петербурга",
                competitionsStatusLst.Find(dg => dg.Id == CompetitionsStatusEnm.Interregional)!),
            
            new(DetailedCompetitionStatusEnm.RegionalChampionship, "Чемпионат субъекта Российской Федерации",
                competitionsStatusLst.Find(dg => dg.Id == CompetitionsStatusEnm.Regional)!),
            new(DetailedCompetitionStatusEnm.RegionalCup, "Кубок субъекта Российской Федерации",
                competitionsStatusLst.Find(dg => dg.Id == CompetitionsStatusEnm.Regional)!),
            new(DetailedCompetitionStatusEnm.RegionalJuniorChampionship, "Первенство субъекта Российской Федерации",
                competitionsStatusLst.Find(dg => dg.Id == CompetitionsStatusEnm.Regional)!),
            new(DetailedCompetitionStatusEnm.OtherRegionalCompetition, 
                "Другие официальные спортивные соревнования субъекта Российской Федерации",
                competitionsStatusLst.Find(dg => dg.Id == CompetitionsStatusEnm.Regional)!),
            
            new(DetailedCompetitionStatusEnm.MunicipalChampionship, "Чемпионат муниципального образования",
                competitionsStatusLst.Find(dg => dg.Id == CompetitionsStatusEnm.Municipal)!),
            new(DetailedCompetitionStatusEnm.MunicipalJuniorChampionship, "Первенство муниципального образования",
                competitionsStatusLst.Find(dg => dg.Id == CompetitionsStatusEnm.Municipal)!),
            new(DetailedCompetitionStatusEnm.OtherMunicipalCompetition, 
                "Другие официальные соревнования муниципального образования",
                competitionsStatusLst.Find(dg => dg.Id == CompetitionsStatusEnm.Municipal)!),
        };
        
        result = repository.AddRange(detailedCompetitionsStatusLst);
        if (!result)
            return Result<List<DetailedCompetitionStatus>>.Fail(result.Excptn!);

        result = await repository.SaveChangesAsync();
        if (!result)
            return Result<List<DetailedCompetitionStatus>>.Fail(result.Excptn!);

        return Result<List<DetailedCompetitionStatus>>.Done(detailedCompetitionsStatusLst);
    }
    
    /// <summary>
    /// Заполняем группы дисциплин.
    /// </summary>
    public static async Task<Result<List<DisciplineGroup>>> FillDisciplineGroups(IRepository repository)
    {
        // Удаляем группы дисциплин
        var result = repository.RemoveAllQuickly<DisciplineGroup>();
        if (!result)
            return Result<List<DisciplineGroup>>.Fail(result.Excptn!);
        
        var disciplineGroupLst = new List<DisciplineGroup>
        {
            new(DisciplineGroupEnm.Trek, "Маршрут"),
            new(DisciplineGroupEnm.Distance, "Дистанция"),
            new(DisciplineGroupEnm.NordicWalking, "Северная ходьба")
        };
        
        result = repository.AddRange(disciplineGroupLst);
        if (!result)
            return Result<List<DisciplineGroup>>.Fail(result.Excptn!);
        
        result = await repository.SaveChangesAsync();
        if (!result)
            return Result<List<DisciplineGroup>>.Fail(result.Excptn!);

        return Result<List<DisciplineGroup>>.Done(disciplineGroupLst);
    }
    
    /// <summary>
    /// Заполняем подгруппы дисциплин.
    /// </summary>
    public static async Task<Result<List<DisciplineSubGroup>>> FillDisciplineSubGroups(IRepository repository,
        List<DisciplineGroup> disciplineGroupLst)
    {
        // Удаляем подгруппы дисциплин
        var result = repository.RemoveAllQuickly<DisciplineSubGroup>();
        if (!result)
            return Result<List<DisciplineSubGroup>>.Fail(result.Excptn!);
        
        var disciplineSubGroupLst = new List<DisciplineSubGroup>
        {
            new(DisciplineSubGroupEnm.Trek, "Маршрут",
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.Trek)!),

            new(DisciplineSubGroupEnm.Water, "Дистанция - водная",
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.Distance)!),
            new(DisciplineSubGroupEnm.Mountain, "Дистанция - горная",
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.Distance)!),
            new(DisciplineSubGroupEnm.Combined, "Дистанция - комбинированная",
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.Distance)!),
            new(DisciplineSubGroupEnm.Ski, "Дистанция - лыжная",
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.Distance)!),
            new(DisciplineSubGroupEnm.OnMovementItems, "Дистанция - на средствах передвижения",
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.Distance)!),
            new(DisciplineSubGroupEnm.Sailing, "Дистанция - парусная",
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.Distance)!),
            new(DisciplineSubGroupEnm.Hiking, "Дистанция - пешеходная",
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.Distance)!),
            new(DisciplineSubGroupEnm.Speleo, "Дистанция - спелео",
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.Distance)!),

            new(DisciplineSubGroupEnm.NordicWalking, "Северная ходьба",
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.NordicWalking)!),
        };

        result = repository.AddRange(disciplineSubGroupLst);
        if (!result)
            return Result<List<DisciplineSubGroup>>.Fail(result.Excptn!);
        
        result = await repository.SaveChangesAsync();
        if (!result)
            return Result<List<DisciplineSubGroup>>.Fail(result.Excptn!);

        return Result<List<DisciplineSubGroup>>.Done(disciplineSubGroupLst);
    }
    
    /// <summary>
    /// Заполняем дисциплины.
    /// </summary>
    public static async Task<Result<List<Discipline>>> FillDisciplines(IRepository repository,
        List<DisciplineGroup> disciplineGroupLst, List<DisciplineSubGroup> disciplineSubGroupLst)
    {
        // Удаляем дисциплины
        var result = repository.RemoveAllQuickly<Discipline>();
        if (!result)
            return Result<List<Discipline>>.Fail(result.Excptn!);

        var disciplineLst = new List<Discipline>
        {
            CreateTrek(DisciplineEnm.TrekWater, "Маршрут - водный"),
            CreateTrek(DisciplineEnm.TrekMountain, "Маршрут - горный"),
            CreateTrek(DisciplineEnm.TrekCombined, "Маршрут - комбинированный"),
            CreateTrek(DisciplineEnm.TrekSki, "Маршрут - лыжный"),
            CreateTrek(DisciplineEnm.TrekOnMovementItems, "Маршрут - на средствах передвижения"),
            CreateTrek(DisciplineEnm.TrekSailing, "Маршрут - парусный"),
            CreateTrek(DisciplineEnm.TrekHiking, "Маршрут - пешеходный"),
            CreateTrek(DisciplineEnm.TrekSpeleo, "Маршрут - спелео"),

            CreateDistance(DisciplineEnm.DistanceWaterKayak, "Дистанция - водная - каяк", DisciplineSubGroupEnm.Water),
            CreateDistance(DisciplineEnm.DistanceWaterTandemKayak, "Дистанция - водная - байдарка", DisciplineSubGroupEnm.Water),
            CreateDistance(DisciplineEnm.DistanceWaterCatamaran2, "Дистанция - водная - катамаран - 2", DisciplineSubGroupEnm.Water),
            CreateDistance(DisciplineEnm.DistanceWaterCatamaran4, "Дистанция - водная - катамаран - 4", DisciplineSubGroupEnm.Water),
            CreateDistance(DisciplineEnm.DistanceWaterTeamRace, "Дистанция - водная - командная гонка", DisciplineSubGroupEnm.Water),

            CreateDistance(DisciplineEnm.DistanceMountainGroup, "Дистанция - горная - группа", DisciplineSubGroupEnm.Mountain),
            CreateDistance(DisciplineEnm.DistanceMountainBunch, "Дистанция - горная - связка", DisciplineSubGroupEnm.Mountain),

            CreateDistance(DisciplineEnm.DistanceCombined, "Дистанция - комбинированная", DisciplineSubGroupEnm.Combined),

            CreateDistance(DisciplineEnm.DistanceSkiGroup, "Дистанция - лыжная - группа", DisciplineSubGroupEnm.Ski),
            CreateDistance(DisciplineEnm.DistanceSkiBunch, "Дистанция - лыжная - связка", DisciplineSubGroupEnm.Ski),
            CreateDistance(DisciplineEnm.DistanceSki, "Дистанция - лыжная", DisciplineSubGroupEnm.Ski),

            CreateDistance(DisciplineEnm.DistanceOnMovementItemsGroup, "Дистанция - на средствах передвижения - группа", 
                DisciplineSubGroupEnm.OnMovementItems),
            CreateDistance(DisciplineEnm.DistanceOnMovementItems, "Дистанция - на средствах передвижения", 
                DisciplineSubGroupEnm.OnMovementItems),

            CreateDistance(DisciplineEnm.DistanceSailing, "Дистанция - парусная", DisciplineSubGroupEnm.Sailing),

            CreateDistance(DisciplineEnm.DistanceHikingGroup, "Дистанция - пешеходная - группа", DisciplineSubGroupEnm.Hiking),
            CreateDistance(DisciplineEnm.DistanceHikingBunch, "Дистанция - пешеходная - связка", DisciplineSubGroupEnm.Hiking),
            CreateDistance(DisciplineEnm.DistanceHiking, "Дистанция - пешеходная", DisciplineSubGroupEnm.Hiking),

            CreateDistance(DisciplineEnm.DistanceSpeleoGroup, "Дистанция - спелео - группа", DisciplineSubGroupEnm.Speleo),
            CreateDistance(DisciplineEnm.DistanceSpeleoBunch, "Дистанция - спелео - связка", DisciplineSubGroupEnm.Speleo),
            CreateDistance(DisciplineEnm.DistanceSpeleo, "Дистанция - спелео", DisciplineSubGroupEnm.Speleo),

            CreateNordicWalking(DisciplineEnm.NordicWalking, "Северная ходьба"),
        };

        result = repository.AddRange(disciplineLst);
        if (!result)
            return Result<List<Discipline>>.Fail(result.Excptn!);
        
        result = await repository.SaveChangesAsync();
        if (!result)
            return Result<List<Discipline>>.Fail(result.Excptn!);

        return Result<List<Discipline>>.Done(disciplineLst);

        
        // Создаем маршрут
        Discipline CreateTrek(DisciplineEnm id, string name)
        {
            return new Discipline(
                id,
                name,
                $"{name} (1-6 категория)",
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.Trek)!,
                disciplineSubGroupLst.Find(dsg => dsg.Id == DisciplineSubGroupEnm.Trek)!
            );
        }

        // Создаем дистанцию
        Discipline CreateDistance(DisciplineEnm id, string name, DisciplineSubGroupEnm disciplineSubGroup)
        {
            return new Discipline(
                id,
                name,
                name,
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.Distance)!,
                disciplineSubGroupLst.Find(dsg => dsg.Id == disciplineSubGroup)!
            );
        }

        // Создаем северную ходьбу
        Discipline CreateNordicWalking(DisciplineEnm id, string name)
        {
            return new Discipline(
                id,
                name,
                name,
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.NordicWalking)!,
                disciplineSubGroupLst.Find(dsg => dsg.Id == DisciplineSubGroupEnm.NordicWalking)!
            );
        }
    }
    
    /// <summary>
    /// Заполняем судейские категории.
    /// </summary>
    public static async Task<Result<List<RefereeCategory>>> FillRefereeCategories(IRepository repository)
    {
        // Удаляем судейские категории
        var result = repository.RemoveAllQuickly<RefereeCategory>();
        if (!result)
            return Result<List<RefereeCategory>>.Fail(result.Excptn!);

        var refereeCategoryLst = new List<RefereeCategory>
        {
            new(RefereeCategoryEnm.YoungCategory, "ЮСС", "Юный спортивный судья"),
            new(RefereeCategoryEnm.Category3, "СС3К", "Спортивный судья третьей категории"),
            new(RefereeCategoryEnm.Category2, "СС2К", "Спортивный судья второй категории"),
            new(RefereeCategoryEnm.Category1, "СС1К", "Спортивный судья первой категории"),
            new(RefereeCategoryEnm.AllRussCategory, "ССВК", "Спортивный судья всероссийской категории"),
        };

        result = repository.AddRange(refereeCategoryLst);
        if (!result)
            return Result<List<RefereeCategory>>.Fail(result.Excptn!);
        
        result = await repository.SaveChangesAsync();
        if (!result)
            return Result<List<RefereeCategory>>.Fail(result.Excptn!);

        return Result<List<RefereeCategory>>.Done(refereeCategoryLst);
    }
    
    /// <summary>
    /// Заполняем судейские должности.
    /// </summary>
    public static async Task<Result<List<RefereeRole>>> FillRefereeRoles(IRepository repository,
        List<DisciplineGroup> disciplineGroupLst)
    {
        // Удаляем судейские должности
        var result = repository.RemoveAllQuickly<RefereeRole>();
        if (!result)
            return Result<List<RefereeRole>>.Fail(result.Excptn!);

        var refereeRoleLst = new List<RefereeRole>
        {
            new(RefereeRoleEnm.ChiefReferee, "Главный судья", GetAllDisciplineGroups()),
            new(RefereeRoleEnm.ChiefSecretary, "Главный секретарь", GetAllDisciplineGroups()),
            new(RefereeRoleEnm.DeputyForRefereeing, "Зам. гл. судьи по судейству", GetAllDisciplineGroups()),
            new(RefereeRoleEnm.DeputyForSecurity, "Зам. гл. судьи по безопасности", GetAllDisciplineGroups()),
            new(RefereeRoleEnm.DeputyForInformation, "Зам. гл. судьи по информации", GetAllDisciplineGroups()),
            new(RefereeRoleEnm.DeputyForSTS, "Зам. гл. судьи по СТО", GetDistanceOrNordicWalkingDisciplineGroups(),
                description: "Пока отсутствует в Квал. требованиях"), // REMARK: пока отсутствует в Квал. требованиях
            new(RefereeRoleEnm.DeputyForDiscipline, "Зам. гл. судьи по виду", GetTrekDisciplineGroups()),
            new(RefereeRoleEnm.Deputy, "Зам. гл. судьи", GetAllDisciplineGroups()),
            new(RefereeRoleEnm.DeputyChiefSecretary, "Зам. гл. секретаря", GetAllDisciplineGroups()),
            new(RefereeRoleEnm.HeadOfDistance, "Начальник дистанции", GetDistanceOrNordicWalkingDisciplineGroups()),
            new(RefereeRoleEnm.SeniorInspector, "Старший судья-инспектор", GetAllDisciplineGroups()),
            new(RefereeRoleEnm.Inspector, "Судья-инспектор", GetAllDisciplineGroups()),
            new(RefereeRoleEnm.SeniorRefereeForDiscipline, "Старший судья по виду", GetTrekDisciplineGroups()),
            new(RefereeRoleEnm.SeniorExpertReferee, "Старший судья-эксперт", GetTrekDisciplineGroups()),
            new(RefereeRoleEnm.SeniorStartReferee, "Старший судья старта", GetDistanceOrNordicWalkingDisciplineGroups()),
            new(RefereeRoleEnm.SeniorStageReferee, "Старший судья этапа", GetDistanceDisciplineGroups()),
            new(RefereeRoleEnm.SeniorFinishReferee, "Старший судья финиша", GetDistanceOrNordicWalkingDisciplineGroups()),
            new(RefereeRoleEnm.SeniorRefereeController, "Старший cудья-контролёр", GetNordicWalkingDisciplineGroups(),
                description: "Пока отсутствует в Квал. требованиях"), // REMARK: пока отсутствует в Квал. требованиях
            new(RefereeRoleEnm.RefereeForDiscipline, "Судья по виду", GetTrekDisciplineGroups()),
            new(RefereeRoleEnm.ExpertReferee, "Судья-эксперт", GetTrekDisciplineGroups()),
            new(RefereeRoleEnm.InstallerReferee, "Судья-постановщик", GetDistanceOrNordicWalkingDisciplineGroups()),
            new(RefereeRoleEnm.StageReferee, "Судья этапа", GetDistanceDisciplineGroups()),
            new(RefereeRoleEnm.RefereeController, "Судья-контролёр", GetNordicWalkingDisciplineGroups(),
                description: "Пока отсутствует в Квал. требованиях"), // REMARK: пока отсутствует в Квал. требованиях
            new(RefereeRoleEnm.Secretary, "Судья-секретарь", GetAllDisciplineGroups(),
                description: "В Правилах - \"Судья секретарь\", что неправильно"), // REMARK: в Правилах - "Судья секретарь", что неправильно
            new(RefereeRoleEnm.StarterReferee, "Судья-стартер", GetDistanceOrNordicWalkingDisciplineGroups()),
            new(RefereeRoleEnm.TimekeeperReferee, "Судья-хронометрист", GetDistanceOrNordicWalkingDisciplineGroups()),
            new(RefereeRoleEnm.InformationReferee, "Судья по информации", GetDistanceOrNordicWalkingDisciplineGroups()),
            new(RefereeRoleEnm.InsurerReferee, "Судья-страховщик", GetDistanceDisciplineGroups()),
            new(RefereeRoleEnm.AccompanyingReferee, "Судья при участниках", GetDistanceOrNordicWalkingDisciplineGroups()),
            
            new(RefereeRoleEnm.MandateChairman, "Председатель комиссии по допуску", GetAllDisciplineGroups()),
            new(RefereeRoleEnm.RccChairman, "Председатель МКК", GetTrekDisciplineGroups()),
            new(RefereeRoleEnm.TechCommissionChairman, "Председатель техн. комиссии", GetDistanceOrNordicWalkingDisciplineGroups()),
        };

        result = repository.AddRange(refereeRoleLst);
        if (!result)
            return Result<List<RefereeRole>>.Fail(result.Excptn!);
        
        result = await repository.SaveChangesAsync();
        if (!result)
            return Result<List<RefereeRole>>.Fail(result.Excptn!);

        return Result<List<RefereeRole>>.Done(refereeRoleLst);
        

        // Получаем коллекцию всех групп дисциплин
        ICollection<DisciplineGroup> GetAllDisciplineGroups()
            => disciplineGroupLst;

        // Получаем коллекцию групп дисциплин, состоящую только из Маршрута
        ICollection<DisciplineGroup> GetTrekDisciplineGroups()
            => new List<DisciplineGroup> { disciplineGroupLst.Find(item => 
                item.Id == DisciplineGroupEnm.Trek)! };
        
        // Получаем коллекцию групп дисциплин, состоящую только из Дистанции
        ICollection<DisciplineGroup> GetDistanceDisciplineGroups()
            => new List<DisciplineGroup> { disciplineGroupLst.Find(item => 
                item.Id == DisciplineGroupEnm.Distance)! };

        // Получаем коллекцию групп дисциплин, состоящую только из Сев. ходьбы
        ICollection<DisciplineGroup> GetNordicWalkingDisciplineGroups()
            => new List<DisciplineGroup> { disciplineGroupLst.Find(item => 
                item.Id == DisciplineGroupEnm.NordicWalking)! };
        
        // Получаем коллекцию групп дисциплин, состоящую только из Дистанции и Сев. ходьбы
        ICollection<DisciplineGroup> GetDistanceOrNordicWalkingDisciplineGroups()
            => new List<DisciplineGroup> { disciplineGroupLst.Find(item => 
                item.Id == DisciplineGroupEnm.Distance || item.Id == DisciplineGroupEnm.NordicWalking)! };
    }

    /// <summary>
    /// Заполняем доступность судейских должностей.
    /// </summary>
    public static async Task<Result<List<RefereeRoleAvailability>>> FillRefereeRoleAvailabilities(
        IRepository repository)
    {
        // Удаляем старые записи
        var result = repository.RemoveAllQuickly<RefereeRoleAvailability>();
        if (! result.HasValue)
            return Result<List<RefereeRoleAvailability>>.Fail(result.Excptn!);

        var availableRoleLst = new List<RefereeRoleAvailability>();
        DetailedCompetitionStatusEnm[] allowedStatuses;

        // Словарь: должность -> макс. количество судей в зависимости от статуса соревнований
        // Индексы статусов: 
        // 0-RussianChampionship, 1-RussianCup, 2-RussianJuniorChampionship, 
        // 3-OtherRussianCompetition, 4-FederalDistrictChampionship, 5-FederalDistrictJuniorChampionship,
        // 6-RegionalChampionship, 7-RegionalCup, 8-RegionalJuniorChampionship, 9-OtherRegionalCompetition,
        // 10-MunicipalChampionship, 11-MunicipalJuniorChampionship, 12-OtherMunicipalCompetition
        var roleMaxCounts = new Dictionary<RefereeRoleEnm, int[]>
        {
            // Главный судья: везде 1
            [RefereeRoleEnm.ChiefReferee] = [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1],

            // Главный секретарь: везде 1
            [RefereeRoleEnm.ChiefSecretary] = [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1],

            // Зам. главного судьи: везде 2 кроме региональных и муниципальных (там 1)
            [RefereeRoleEnm.Deputy] = [2, 2, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1],

            // Зам. гл. судьи по судейству: везде 1
            [RefereeRoleEnm.DeputyForRefereeing] = [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1],

            // Зам. гл. судьи по безопасности: везде 1
            [RefereeRoleEnm.DeputyForSecurity] = [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1],

            // Зам. гл. судьи по информации: везде 1
            [RefereeRoleEnm.DeputyForInformation] = [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1],

            // Зам. гл. судьи по СТО: как DeputyForInformation (везде 1)
            [RefereeRoleEnm.DeputyForSTS] = [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1],

            // Зам. гл. судьи по виду: везде 1
            [RefereeRoleEnm.DeputyForDiscipline] = [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1],

            // Зам. гл. секретаря: везде 1
            [RefereeRoleEnm.DeputyChiefSecretary] = [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1],

            // Начальник дистанции: 2 для всероссов и федеральных, 1 для региональных и муниципальных
            [RefereeRoleEnm.HeadOfDistance] = [2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1],

            // Старший судья-инспектор: везде 1
            [RefereeRoleEnm.SeniorInspector] = [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1],

            // Судья-инспектор: везде 1
            [RefereeRoleEnm.Inspector] = [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1],

            // Старший судья по виду: 6,6,5,5,3,3,2,2,2,2,1,1,1
            [RefereeRoleEnm.SeniorRefereeForDiscipline] = [6, 6, 5, 5, 3, 3, 2, 2, 2, 2, 1, 1, 1],

            // Старший судья-эксперт: 3,3,2,2,2,2,2,2,2,2,2,1,1
            [RefereeRoleEnm.SeniorExpertReferee] = [3, 3, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 1],

            // Старший судья старта: 2,2,2,2,2,1,1,1,1,1,1,1,1
            [RefereeRoleEnm.SeniorStartReferee] = [2, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1],

            // Старший судья этапа: 12,12,12,12,12,12,12,12,12,10,10,10,10
            [RefereeRoleEnm.SeniorStageReferee] = [12, 12, 12, 12, 12, 12, 12, 12, 12, 10, 10, 10, 10],

            // Старший судья-контролёр: как SeniorStageReferee
            [RefereeRoleEnm.SeniorRefereeController] = [12, 12, 12, 12, 12, 12, 12, 12, 12, 10, 10, 10, 10],

            // Старший судья финиша: 2,2,2,2,2,1,1,1,1,1,1,1,1
            [RefereeRoleEnm.SeniorFinishReferee] = [2, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1],

            // Судья-постановщик: 4,4,4,4,4,4,3,3,3,3,3,3,3
            [RefereeRoleEnm.InstallerReferee] = [4, 4, 4, 4, 4, 4, 3, 3, 3, 3, 3, 3, 3],

            // Судья этапа: 30 везде
            [RefereeRoleEnm.StageReferee] = [30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30],

            // Судья-контролёр: как StageReferee (30 везде)
            [RefereeRoleEnm.RefereeController] = [30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30],

            // Судья по виду: 20,20,15,15,6,6,5,5,5,5,4,4,4
            [RefereeRoleEnm.RefereeForDiscipline] = [20, 20, 15, 15, 6, 6, 5, 5, 5, 5, 4, 4, 4],

            // Судья-эксперт: 12,12,12,12,12,12,6,6,6,6,5,5,5
            [RefereeRoleEnm.ExpertReferee] = [12, 12, 12, 12, 12, 12, 6, 6, 6, 6, 5, 5, 5],

            // Судья-секретарь: 4,4,4,4,3,3,2,2,2,2,1,1,1
            [RefereeRoleEnm.Secretary] = [4, 4, 4, 4, 3, 3, 2, 2, 2, 2, 1, 1, 1],

            // Судья-стартёр: 2,2,2,2,2,2,2,2,1,1,1,1,1
            [RefereeRoleEnm.StarterReferee] = [2, 2, 2, 2, 2, 2, 2, 2, 1, 1, 1, 1, 1],

            // Судья-хронометрист: 2 везде
            [RefereeRoleEnm.TimekeeperReferee] = [2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2],

            // Судья по информации: 3,3,2,2,2,2,1,1,1,1,1,1,1
            [RefereeRoleEnm.InformationReferee] = [3, 3, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1],

            // Судья-страховщик: 10 везде
            [RefereeRoleEnm.InsurerReferee] = [10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10],

            // Судья при участниках: 2 везде
            [RefereeRoleEnm.AccompanyingReferee] = [2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2],
            
            // -----------------------------
            
            // Председатель комиссии по допуску: везде 1
            [RefereeRoleEnm.MandateChairman] = [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1],
            
            // Председатель МКК: везде 1
            [RefereeRoleEnm.RccChairman] = [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1],

            // Председатель технической комиссии: везде 1
            [RefereeRoleEnm.TechCommissionChairman] = [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1],
        };

        // ========== ЮСС, СС3К ==========

        // Муниципальные
        allowedStatuses =
        [
            DetailedCompetitionStatusEnm.MunicipalChampionship,
            DetailedCompetitionStatusEnm.MunicipalJuniorChampionship,
            DetailedCompetitionStatusEnm.OtherMunicipalCompetition
        ];
        foreach (var status in allowedStatuses)
        {
            availableRoleLst.AddRange([
                // REMARK: Старший судья-контролёр пока так (нет в Квал. треб. к СС)
                // REMARK: Судья-контролёр пока так (нет в Квал. треб. к СС)
                Create(RefereeRoleEnm.SeniorRefereeForDiscipline, status, RefereeCategoryEnm.YoungCategory), // Старший судья по виду
                Create(RefereeRoleEnm.SeniorExpertReferee, status, RefereeCategoryEnm.YoungCategory),        // Старший судья-эксперт
                Create(RefereeRoleEnm.SeniorStartReferee, status, RefereeCategoryEnm.YoungCategory),         // Старший судья старта
                Create(RefereeRoleEnm.SeniorStageReferee, status, RefereeCategoryEnm.YoungCategory),         // Старший судья этапа
                Create(RefereeRoleEnm.SeniorFinishReferee, status, RefereeCategoryEnm.YoungCategory),        // Старший судья финиша
                Create(RefereeRoleEnm.SeniorRefereeController, status, RefereeCategoryEnm.YoungCategory),    // Старший судья-контролёр
                Create(RefereeRoleEnm.InstallerReferee, status, RefereeCategoryEnm.YoungCategory),           // Судья-постановщик
                Create(RefereeRoleEnm.StageReferee, status, RefereeCategoryEnm.YoungCategory),               // Судья этапа
                Create(RefereeRoleEnm.RefereeController, status, RefereeCategoryEnm.YoungCategory),          // Судья-контролёр
            ]);
        }

        // Региональные и ниже
        allowedStatuses =
        [
            DetailedCompetitionStatusEnm.RegionalChampionship,
            DetailedCompetitionStatusEnm.RegionalCup,
            DetailedCompetitionStatusEnm.RegionalJuniorChampionship,
            DetailedCompetitionStatusEnm.OtherRegionalCompetition,
            DetailedCompetitionStatusEnm.MunicipalChampionship,
            DetailedCompetitionStatusEnm.MunicipalJuniorChampionship,
            DetailedCompetitionStatusEnm.OtherMunicipalCompetition
        ];
        foreach (var status in allowedStatuses)
        {
            availableRoleLst.AddRange([
                Create(RefereeRoleEnm.RefereeForDiscipline, status, RefereeCategoryEnm.YoungCategory), // Судья по виду
                Create(RefereeRoleEnm.ExpertReferee, status, RefereeCategoryEnm.YoungCategory),        // Судья-эксперт
                Create(RefereeRoleEnm.Secretary, status, RefereeCategoryEnm.YoungCategory),            // Судья-секретарь
                Create(RefereeRoleEnm.StarterReferee, status, RefereeCategoryEnm.YoungCategory),       // Судья-стартёр
                Create(RefereeRoleEnm.TimekeeperReferee, status, RefereeCategoryEnm.YoungCategory),    // Судья-хронометрист
                Create(RefereeRoleEnm.InformationReferee, status, RefereeCategoryEnm.YoungCategory),   // Судья по информации
                Create(RefereeRoleEnm.InsurerReferee, status, RefereeCategoryEnm.YoungCategory),       // Судья-страховщик
                Create(RefereeRoleEnm.AccompanyingReferee, status, RefereeCategoryEnm.YoungCategory),  // Судья при участниках
            ]);
        }
    

        // ========== СС2К ==========

        // Другие оф. муницип. образования
        {
            var status = DetailedCompetitionStatusEnm.OtherMunicipalCompetition;
            availableRoleLst.Add(
                Create(RefereeRoleEnm.SeniorInspector, status, RefereeCategoryEnm.Category2) // Старший судья-инспектор
            );
        }

        // Муниципальные
        allowedStatuses =
        [
            DetailedCompetitionStatusEnm.MunicipalChampionship,
            DetailedCompetitionStatusEnm.MunicipalJuniorChampionship,
            DetailedCompetitionStatusEnm.OtherMunicipalCompetition,
        ];
        foreach (var status in allowedStatuses)
        {
            availableRoleLst.AddRange([
                Create(RefereeRoleEnm.ChiefReferee, status, RefereeCategoryEnm.Category2),        // Главный судья
                Create(RefereeRoleEnm.ChiefSecretary, status, RefereeCategoryEnm.Category2),      // Главный секретарь
                Create(RefereeRoleEnm.DeputyForRefereeing, status, RefereeCategoryEnm.Category2), // Зам. гл. судьи по судейству
                Create(RefereeRoleEnm.DeputyForSecurity, status, RefereeCategoryEnm.Category2),   // Зам. гл. судьи по безопасности
                Create(RefereeRoleEnm.DeputyForDiscipline, status, RefereeCategoryEnm.Category2), // Зам. гл. судьи по виду
                Create(RefereeRoleEnm.Inspector, status, RefereeCategoryEnm.Category2)            // Судья-инспектор
            ]);
        }

        // Кубок субъекта и ниже
        allowedStatuses =
        [
            DetailedCompetitionStatusEnm.RegionalCup,
            DetailedCompetitionStatusEnm.RegionalJuniorChampionship,
            DetailedCompetitionStatusEnm.OtherRegionalCompetition,
            DetailedCompetitionStatusEnm.MunicipalChampionship,
            DetailedCompetitionStatusEnm.MunicipalJuniorChampionship,
            DetailedCompetitionStatusEnm.OtherMunicipalCompetition
        ];
        foreach (var status in allowedStatuses)
        {
            availableRoleLst.AddRange([
                // REMARK: Зам. гл. судьи по СТО пока так (нет в Квал. треб. к СС)
                Create(RefereeRoleEnm.Deputy, status, RefereeCategoryEnm.Category2),               // Зам. гл. судьи
                Create(RefereeRoleEnm.DeputyForInformation, status, RefereeCategoryEnm.Category2), // Зам. гл. судьи по информации
                Create(RefereeRoleEnm.DeputyForSTS, status, RefereeCategoryEnm.Category2),         // Зам. гл. судьи по СТО
                Create(RefereeRoleEnm.DeputyChiefSecretary, status, RefereeCategoryEnm.Category2), // Зам. гл. секретаря
                Create(RefereeRoleEnm.HeadOfDistance, status, RefereeCategoryEnm.Category2)        // Начальник дистанции
            ]);
        }

        // Чемп. субъекта - Другие оф. субъекта
        allowedStatuses =
        [
            DetailedCompetitionStatusEnm.RegionalChampionship,
            DetailedCompetitionStatusEnm.RegionalCup,
            DetailedCompetitionStatusEnm.RegionalJuniorChampionship,
            DetailedCompetitionStatusEnm.OtherRegionalCompetition
        ];
        foreach (var status in allowedStatuses)
        {
            availableRoleLst.AddRange([
                Create(RefereeRoleEnm.SeniorRefereeForDiscipline, status, RefereeCategoryEnm.Category2), // Старший судья по виду
                Create(RefereeRoleEnm.SeniorExpertReferee, status, RefereeCategoryEnm.Category2)         // Старший судья-эксперт
            ]);
        }

        // Чемп. ФО - Другие оф. субъекта
        allowedStatuses =
        [
            DetailedCompetitionStatusEnm.FederalDistrictChampionship,
            DetailedCompetitionStatusEnm.FederalDistrictJuniorChampionship,
            DetailedCompetitionStatusEnm.RegionalChampionship,
            DetailedCompetitionStatusEnm.RegionalCup,
            DetailedCompetitionStatusEnm.RegionalJuniorChampionship,
            DetailedCompetitionStatusEnm.OtherRegionalCompetition
        ];
        foreach (var status in allowedStatuses)
        {
            availableRoleLst.AddRange([
                // REMARK: Старший судья-контролёр пока так (нет в Квал. треб. к СС)
                Create(RefereeRoleEnm.SeniorStartReferee, status, RefereeCategoryEnm.Category2),      // Старший судья старта
                Create(RefereeRoleEnm.SeniorStageReferee, status, RefereeCategoryEnm.Category2),      // Старший судья этапа
                Create(RefereeRoleEnm.SeniorFinishReferee, status, RefereeCategoryEnm.Category2),     // Старший судья финиша
                Create(RefereeRoleEnm.SeniorRefereeController, status, RefereeCategoryEnm.Category2), // Старший судья-контролёр
            ]);
        }

        // Другие оф. субъекта и выше
        allowedStatuses =
        [
            DetailedCompetitionStatusEnm.RussianChampionship,
            DetailedCompetitionStatusEnm.RussianCup,
            DetailedCompetitionStatusEnm.RussianJuniorChampionship,
            DetailedCompetitionStatusEnm.OtherRussianCompetition,
            DetailedCompetitionStatusEnm.FederalDistrictChampionship,
            DetailedCompetitionStatusEnm.FederalDistrictJuniorChampionship,
            DetailedCompetitionStatusEnm.RegionalChampionship,
            DetailedCompetitionStatusEnm.RegionalCup,
            DetailedCompetitionStatusEnm.RegionalJuniorChampionship,
            DetailedCompetitionStatusEnm.OtherRegionalCompetition
        ];
        foreach (var status in allowedStatuses)
        {
            availableRoleLst.AddRange([
                // REMARK: Судья-контролёр пока так (нет в Квал. треб. к СС)
                Create(RefereeRoleEnm.InstallerReferee, status, RefereeCategoryEnm.Category2), // Судья-постановщик
                Create(RefereeRoleEnm.StageReferee, status, RefereeCategoryEnm.Category2),     // Судья этапа
                Create(RefereeRoleEnm.RefereeController, status, RefereeCategoryEnm.Category2) // Судья-контролёр
            ]);
        }

        // Выше чемп. субъекта
        allowedStatuses =
        [
            DetailedCompetitionStatusEnm.RussianChampionship,
            DetailedCompetitionStatusEnm.RussianCup,
            DetailedCompetitionStatusEnm.RussianJuniorChampionship,
            DetailedCompetitionStatusEnm.OtherRussianCompetition,
            DetailedCompetitionStatusEnm.FederalDistrictChampionship,
            DetailedCompetitionStatusEnm.FederalDistrictJuniorChampionship
        ];
        foreach (var status in allowedStatuses)
        {
            availableRoleLst.AddRange([
                Create(RefereeRoleEnm.RefereeForDiscipline, status, RefereeCategoryEnm.Category2), // Судья по виду
                Create(RefereeRoleEnm.ExpertReferee, status, RefereeCategoryEnm.Category2),        // Судья-эксперт
                Create(RefereeRoleEnm.Secretary, status, RefereeCategoryEnm.Category2),            // Судья-секретарь
                Create(RefereeRoleEnm.StarterReferee, status, RefereeCategoryEnm.Category2),       // Судья-стартёр
                Create(RefereeRoleEnm.TimekeeperReferee, status, RefereeCategoryEnm.Category2),    // Судья-хронометрист
                Create(RefereeRoleEnm.InformationReferee, status, RefereeCategoryEnm.Category2),   // Судья по информации
                Create(RefereeRoleEnm.InsurerReferee, status, RefereeCategoryEnm.Category2),       // Судья-страховщик
                Create(RefereeRoleEnm.AccompanyingReferee, status, RefereeCategoryEnm.Category2)   // Судья при участниках
            ]);
        }

        // ========== СС1К ==========

        // Чемп. ФО - Другие оф. субъекта
        allowedStatuses =
        [
            DetailedCompetitionStatusEnm.FederalDistrictChampionship,
            DetailedCompetitionStatusEnm.FederalDistrictJuniorChampionship,
            DetailedCompetitionStatusEnm.RegionalChampionship,
            DetailedCompetitionStatusEnm.RegionalCup,
            DetailedCompetitionStatusEnm.RegionalJuniorChampionship,
            DetailedCompetitionStatusEnm.OtherRegionalCompetition
        ];
        foreach (var status in allowedStatuses)
        {
            availableRoleLst.AddRange([
                Create(RefereeRoleEnm.ChiefReferee, status, RefereeCategoryEnm.Category1),        // Главный судья
                Create(RefereeRoleEnm.ChiefSecretary, status, RefereeCategoryEnm.Category1),      // Главный секретарь
                Create(RefereeRoleEnm.DeputyForRefereeing, status, RefereeCategoryEnm.Category1), // Зам. гл. судьи по судейству
                Create(RefereeRoleEnm.DeputyForSecurity, status, RefereeCategoryEnm.Category1),   // Зам. гл. судьи по безопасности
                Create(RefereeRoleEnm.DeputyForDiscipline, status, RefereeCategoryEnm.Category1), // Зам. гл. судьи по виду
                Create(RefereeRoleEnm.Inspector, status, RefereeCategoryEnm.Category1)            // Судья-инспектор
            ]);
        }

        // Чемп. ФО - выше других оф. муницип. образования
        allowedStatuses =
        [
            DetailedCompetitionStatusEnm.FederalDistrictChampionship,
            DetailedCompetitionStatusEnm.FederalDistrictJuniorChampionship,
            DetailedCompetitionStatusEnm.RegionalChampionship,
            DetailedCompetitionStatusEnm.RegionalCup,
            DetailedCompetitionStatusEnm.RegionalJuniorChampionship,
            DetailedCompetitionStatusEnm.OtherRegionalCompetition,
            DetailedCompetitionStatusEnm.MunicipalChampionship,
            DetailedCompetitionStatusEnm.MunicipalJuniorChampionship
        ];
        foreach (var status in allowedStatuses)
        {
            availableRoleLst.Add(
                Create(RefereeRoleEnm.SeniorInspector, status, RefereeCategoryEnm.Category1) // Старший судья-инспектор
            );
        }

        // Чемп. субъекта и выше
        allowedStatuses =
        [
            DetailedCompetitionStatusEnm.RussianChampionship,
            DetailedCompetitionStatusEnm.RussianCup,
            DetailedCompetitionStatusEnm.RussianJuniorChampionship,
            DetailedCompetitionStatusEnm.OtherRussianCompetition,
            DetailedCompetitionStatusEnm.FederalDistrictChampionship,
            DetailedCompetitionStatusEnm.FederalDistrictJuniorChampionship,
            DetailedCompetitionStatusEnm.RegionalChampionship,
        ];
        foreach (var status in allowedStatuses)
        {
            availableRoleLst.AddRange([
                // REMARK: Зам. гл. судьи по СТО пока так (нет в Квал. треб. к СС)
                Create(RefereeRoleEnm.Deputy, status, RefereeCategoryEnm.Category1),               // Зам. гл. судьи
                Create(RefereeRoleEnm.DeputyForInformation, status, RefereeCategoryEnm.Category1), // Зам. гл. судьи по информации
                Create(RefereeRoleEnm.DeputyForSTS, status, RefereeCategoryEnm.Category1),         // Зам. гл. судьи по СТО
                Create(RefereeRoleEnm.DeputyChiefSecretary, status, RefereeCategoryEnm.Category1), // Зам. гл. секретаря
                Create(RefereeRoleEnm.HeadOfDistance, status, RefereeCategoryEnm.Category1)        // Начальник дистанции
            ]);
        }

        // Выше чемп. субъекта
        allowedStatuses =
        [
            DetailedCompetitionStatusEnm.RussianChampionship,
            DetailedCompetitionStatusEnm.RussianCup,
            DetailedCompetitionStatusEnm.RussianJuniorChampionship,
            DetailedCompetitionStatusEnm.OtherRussianCompetition,
            DetailedCompetitionStatusEnm.FederalDistrictChampionship,
            DetailedCompetitionStatusEnm.FederalDistrictJuniorChampionship,
        ];
        foreach (var status in allowedStatuses)
        {
            availableRoleLst.AddRange([
                Create(RefereeRoleEnm.SeniorRefereeForDiscipline, status, RefereeCategoryEnm.Category1), // Старший судья по виду
                Create(RefereeRoleEnm.SeniorExpertReferee, status, RefereeCategoryEnm.Category1)         // Старший судья-эксперт
            ]);
        }

        // Всероссы
        allowedStatuses =
        [
            DetailedCompetitionStatusEnm.RussianChampionship,
            DetailedCompetitionStatusEnm.RussianCup,
            DetailedCompetitionStatusEnm.RussianJuniorChampionship,
            DetailedCompetitionStatusEnm.OtherRussianCompetition
        ];
        foreach (var status in allowedStatuses)
        {
            availableRoleLst.AddRange([
                // REMARK: Старший судья-контролёр пока так (нет в Квал. треб. к СС)
                Create(RefereeRoleEnm.SeniorStartReferee, status, RefereeCategoryEnm.Category1),      // Старший судья старта
                Create(RefereeRoleEnm.SeniorStageReferee, status, RefereeCategoryEnm.Category1),      // Старший судья этапа
                Create(RefereeRoleEnm.SeniorFinishReferee, status, RefereeCategoryEnm.Category1),     // Старший судья финиша
                Create(RefereeRoleEnm.SeniorRefereeController, status, RefereeCategoryEnm.Category1), // Старший судья-контролёр
            ]);
        }

        // ========== ССВК ==========

        // Всероссы
        allowedStatuses =
        [
            DetailedCompetitionStatusEnm.RussianChampionship,
            DetailedCompetitionStatusEnm.RussianCup,
            DetailedCompetitionStatusEnm.RussianJuniorChampionship,
            DetailedCompetitionStatusEnm.OtherRussianCompetition,
        ];
        foreach (var status in allowedStatuses)
        {
            availableRoleLst.AddRange([
                Create(RefereeRoleEnm.ChiefReferee, status, RefereeCategoryEnm.AllRussCategory),        // Главный судья
                Create(RefereeRoleEnm.ChiefSecretary, status, RefereeCategoryEnm.AllRussCategory),      // Главный секретарь
                Create(RefereeRoleEnm.DeputyForRefereeing, status, RefereeCategoryEnm.AllRussCategory), // Зам. гл. судьи по судейству
                Create(RefereeRoleEnm.DeputyForSecurity, status, RefereeCategoryEnm.AllRussCategory),   // Зам. гл. судьи по безопасности
                Create(RefereeRoleEnm.DeputyForDiscipline, status, RefereeCategoryEnm.AllRussCategory), // Зам. гл. судьи по виду
                Create(RefereeRoleEnm.SeniorInspector, status, RefereeCategoryEnm.AllRussCategory),     // Старший судья-инспектор
                Create(RefereeRoleEnm.Inspector, status, RefereeCategoryEnm.AllRussCategory)            // Судья-инспектор
            ]);
        }
        
        // Для "Не судейских" должностей - в количестве 1 для всех статусов и мин. категорией ЮСС
        foreach (var status in Enum.GetValues<DetailedCompetitionStatusEnm>())
        {
            availableRoleLst.AddRange([
                Create(RefereeRoleEnm.MandateChairman, status, RefereeCategoryEnm.YoungCategory),
                Create(RefereeRoleEnm.RccChairman, status, RefereeCategoryEnm.YoungCategory),
                Create(RefereeRoleEnm.TechCommissionChairman, status, RefereeCategoryEnm.YoungCategory),
            ]);
        }

        result = repository.AddRange(availableRoleLst);
        if (! result)
            return Result<List<RefereeRoleAvailability>>.Fail(result.Excptn!);

        result = await repository.SaveChangesAsync();
        if (! result)
            return Result<List<RefereeRoleAvailability>>.Fail(result.Excptn!);

        return Result<List<RefereeRoleAvailability>>.Done(availableRoleLst);

        // Внутренняя функция для создания RefereeRoleAvailability
        RefereeRoleAvailability Create(RefereeRoleEnm role, DetailedCompetitionStatusEnm status, RefereeCategoryEnm category) =>
            new(role, status, category, GetMaxCount(role, status));

        // Внутренняя функция для получения количества судей
        int GetMaxCount(RefereeRoleEnm role, DetailedCompetitionStatusEnm status)
        {
            return ! roleMaxCounts.TryGetValue(role, out var counts) 
                ? 1 
                : counts[status.ToInt() - 1];
        }
    }

    /// <summary>
    /// Заполняем варианты пола.
    /// </summary>
    public static async Task<Result<List<Sex>>> FillSexes(IRepository repository)
    {
        // Удаляем подгруппы дисциплин
        var result = repository.RemoveAllQuickly<Sex>();
        if (!result.HasValue)
            return Result<List<Sex>>.Fail(result.Excptn!);

        var sexLst = new List<Sex>
        {
            new(SexEnm.Male, "м", "мужской", "мужские",
                "мужчина", "мужчины"),
            new(SexEnm.Female, "ж", "женский", "женские", 
                "женщина", "женщины"),
            new(SexEnm.Mixed, "см", "смешанный", "смешанные"),
        };

        result = repository.AddRange(sexLst);
        if (!result)
            return Result<List<Sex>>.Fail(result.Excptn!);
        
        result = await repository.SaveChangesAsync();
        if (!result)
            return Result<List<Sex>>.Fail(result.Excptn!);

        return Result<List<Sex>>.Done(sexLst);
    }
    
    /// <summary>
    /// Заполняем типы спортивных юнитов.
    /// </summary>
    public static async Task<Result<List<SportUnitType>>> FillSportUnitType(IRepository repository)
    {
        // Удаляем типы спортивных юнитов
        var result = repository.RemoveAllQuickly<SportUnitType>();
        if (!result.HasValue)
            return Result<List<SportUnitType>>.Fail(result.Excptn!);

        var sportUnitTypeLst = new List<SportUnitType>
        {
            new(SportUnitTypeEnm.TrekGroup, "Группа", "Группа", "Маршрут"),
            new(SportUnitTypeEnm.Athlete, "Спортсмен", "Спортсмен", "Индивидуальное прохождение"),
            new(SportUnitTypeEnm.Bunch, "Связка", "Связка"),
            new(SportUnitTypeEnm.Group, "Группа", "Группа"),
            new(SportUnitTypeEnm.KayakCrew, "Экипаж каяка", "Каяк"),
            new(SportUnitTypeEnm.TandemKayakCrew, "Экипаж байдарки", "Байдарка"),
            new(SportUnitTypeEnm.Catamaran2Crew, "Экипаж катамарана 2-ки", "Кат 2-ка"),
            new(SportUnitTypeEnm.Catamaran4Crew, "Экипаж катамарана 4-ки", "Кат 4-ка"),
            new(SportUnitTypeEnm.WaterTeam, "Команда", "Совокупность судов", "Дистанция - водная"),
            new(SportUnitTypeEnm.Team, "Команда", "Команда", "Дистанция - комбинированная"),
            new(SportUnitTypeEnm.SailboatCrew, "Экипаж парусного судна", "Экипаж", "Дистанция - парусная"),
            new(SportUnitTypeEnm.SailboatCrew2, "Экипаж 2-го парусного судна", "Экипаж-2", "Дистанция - парусная"),
            new(SportUnitTypeEnm.SailboatCrew3, "Экипаж 3-го парусного судна", "Экипаж-3", "Дистанция - парусная"),
            new(SportUnitTypeEnm.SailboatCrew4, "Экипаж 4-го парусного судна", "Экипаж-4", "Дистанция - парусная"),
            new(SportUnitTypeEnm.SailboatTeam, "Группа", "Совокупность судов", "Дистанция - парусная"),
            
            // TODO: Возможно добавятся экипажи авто-мото
        };

        result = repository.AddRange(sportUnitTypeLst);
        if (!result)
            return Result<List<SportUnitType>>.Fail(result.Excptn!);
        
        result = await repository.SaveChangesAsync();
        if (!result)
            return Result<List<SportUnitType>>.Fail(result.Excptn!);

        return Result<List<SportUnitType>>.Done(sportUnitTypeLst);
    }
    
    /// <summary>
    /// Заполняем трудности.
    /// </summary>
    public static async Task<Result<List<Difficulty>>> FillDifficulties(IRepository repository)
    {
        // Удаляем трудности
        var result = repository.RemoveAllQuickly<Difficulty>();
        if (!result.HasValue)
            return Result<List<Difficulty>>.Fail(result.Excptn!);

        var difficultyLst = new List<Difficulty>
        {
            new(DifficultyEnm.LowThird, DisciplineGroupEnm.Trek, "3 ст.с.", "3 степень сложности", "3 степени сложности"),
            new(DifficultyEnm.LowSecond, DisciplineGroupEnm.Trek, "2 ст.с.", "2 степень сложности", "2 степени сложности"),
            new(DifficultyEnm.LowFirst, DisciplineGroupEnm.Trek, "1 ст.с.", "1 степень сложности", "1 степени сложности"),
            new(DifficultyEnm.First, DisciplineGroupEnm.Trek, "1 к.с.", "1 категория сложности", "1 категории сложности"),
            new(DifficultyEnm.Second, DisciplineGroupEnm.Trek, "2 к.с.", "2 категория сложности", "2 категории сложности"),
            new(DifficultyEnm.Third, DisciplineGroupEnm.Trek, "3 к.с.", "3 категория сложности", "3 категории сложности"),
            new(DifficultyEnm.Fourth, DisciplineGroupEnm.Trek, "4 к.с.", "4 категория сложности", "4 категории сложности"),
            new(DifficultyEnm.Fifth, DisciplineGroupEnm.Trek, "5 к.с.", "5 категория сложности", "5 категории сложности"),
            new(DifficultyEnm.Sixth, DisciplineGroupEnm.Trek, "6 к.с.", "6 категория сложности", "6 категории сложности"),
            
            new(DifficultyEnm.First, DisciplineGroupEnm.Distance, "1 класс", "1 класс", "1 класса"),
            new(DifficultyEnm.Second, DisciplineGroupEnm.Distance, "2 класс", "2 класс", "2 класса"),
            new(DifficultyEnm.Third, DisciplineGroupEnm.Distance, "3 класс", "3 класс", "3 класса"),
            new(DifficultyEnm.Fourth, DisciplineGroupEnm.Distance, "4 класс", "4 класс", "4 класса"),
            new(DifficultyEnm.Fifth, DisciplineGroupEnm.Distance, "5 класс", "5 класс", "5 класса"),
            new(DifficultyEnm.Sixth, DisciplineGroupEnm.Distance, "6 класс", "6 класс", "6 класса"),
            
            new(DifficultyEnm.First, DisciplineGroupEnm.NordicWalking, "1 класс", "1 класс", "1 класса"),
            new(DifficultyEnm.Second, DisciplineGroupEnm.NordicWalking, "2 класс", "2 класс", "2 класса"),
            new(DifficultyEnm.Third, DisciplineGroupEnm.NordicWalking, "3 класс", "3 класс", "3 класса"),
            new(DifficultyEnm.Fourth, DisciplineGroupEnm.NordicWalking, "4 класс", "4 класс", "4 класса"),
            new(DifficultyEnm.Fifth, DisciplineGroupEnm.NordicWalking, "5 класс", "5 класс", "5 класса"),
            new(DifficultyEnm.Sixth, DisciplineGroupEnm.NordicWalking, "6 класс", "6 класс", "6 класса"),
        };

        result = repository.AddRange(difficultyLst);
        if (!result)
            return Result<List<Difficulty>>.Fail(result.Excptn!);
        
        result = await repository.SaveChangesAsync();
        if (!result)
            return Result<List<Difficulty>>.Fail(result.Excptn!);

        return Result<List<Difficulty>>.Done(difficultyLst);
    }
    
    /// <summary>
    /// Заполняем возрастные группы.
    /// </summary>
    public static async Task<Result<List<AgeGroup>>> FillAgeGroups(IRepository repository)
    {
        // Удаляем возрастные группы
        var result = repository.RemoveAllQuickly<AgeGroup>();
        if (!result.HasValue)
            return Result<List<AgeGroup>>.Fail(result.Excptn!);

        var ageGroupLst = new List<AgeGroup>();
        for (var disciplineSubGroupId = DisciplineSubGroupEnm.Trek; 
             disciplineSubGroupId <= DisciplineSubGroupEnm.NordicWalking; 
             disciplineSubGroupId++)
        {
            switch (disciplineSubGroupId)
            {
                case DisciplineSubGroupEnm.Trek:
                    // Маршрут
                    ageGroupLst.Add(new AgeGroup(AgeGroupEnm.Children, disciplineSubGroupId, "Мальчики, девочки", 10, 12));
                    ageGroupLst.Add(new AgeGroup(AgeGroupEnm.Youths, disciplineSubGroupId, "Юноши, девушки", 13, 16));
                    ageGroupLst.Add(new AgeGroup(AgeGroupEnm.Juniors, disciplineSubGroupId, "Юниоры, юниорки", 17, 21, 
                        17, 25));
                    ageGroupLst.Add(new AgeGroup(AgeGroupEnm.Adults, disciplineSubGroupId, "Мужчины, женщины", 22, null));
                    ageGroupLst.Add(new AgeGroup(AgeGroupEnm.Veterans, disciplineSubGroupId, "Ветераны", 60, null));
                    break;
                case DisciplineSubGroupEnm.Ski or DisciplineSubGroupEnm.Hiking:
                    // Лыжная, пешеходная
                    ageGroupLst.Add(new AgeGroup(AgeGroupEnm.Children, disciplineSubGroupId, "Мальчики, девочки", 8, 13));
                    ageGroupLst.Add(new AgeGroup(AgeGroupEnm.Youths, disciplineSubGroupId, "Юноши, девушки", 14, 15));
                    ageGroupLst.Add(new AgeGroup(AgeGroupEnm.Juniors, disciplineSubGroupId, "Юниоры, юниорки", 16, 21, 
                        17, 25));
                    ageGroupLst.Add(new AgeGroup(AgeGroupEnm.Adults, disciplineSubGroupId, "Мужчины, женщины", 22, null));
                    ageGroupLst.Add(new AgeGroup(AgeGroupEnm.Veterans, disciplineSubGroupId, "Ветераны", 60, null));
                    break;
                case DisciplineSubGroupEnm.Speleo:
                    // Спелео
                    ageGroupLst.Add(new AgeGroup(AgeGroupEnm.Children, disciplineSubGroupId, "Мальчики, девочки", 10, 13));
                    ageGroupLst.Add(new AgeGroup(AgeGroupEnm.Youths, disciplineSubGroupId, "Юноши, девушки", 14, 15));
                    ageGroupLst.Add(new AgeGroup(AgeGroupEnm.Juniors, disciplineSubGroupId, "Юниоры, юниорки", 16, 21, 
                        18, 25));
                    ageGroupLst.Add(new AgeGroup(AgeGroupEnm.Adults, disciplineSubGroupId, "Мужчины, женщины", 22, null));
                    ageGroupLst.Add(new AgeGroup(AgeGroupEnm.Veterans, disciplineSubGroupId, "Ветераны", 60, null));
                    break;
                case DisciplineSubGroupEnm.NordicWalking:
                    // Северная ходьба
                    ageGroupLst.Add(new AgeGroup(AgeGroupEnm.Adults, disciplineSubGroupId, "Мужчины, женщины", 18, null,
                        18, 25));
                    ageGroupLst.Add(new AgeGroup(AgeGroupEnm.Veterans, disciplineSubGroupId, "Ветераны", 60, null));
                    break;
                default:
                    // Остальные
                    ageGroupLst.Add(new AgeGroup(AgeGroupEnm.Children, disciplineSubGroupId, "Мальчики, девочки", 10, 13));
                    ageGroupLst.Add(new AgeGroup(AgeGroupEnm.Youths, disciplineSubGroupId, "Юноши, девушки", 14, 15));
                    ageGroupLst.Add(new AgeGroup(AgeGroupEnm.Juniors, disciplineSubGroupId, "Юниоры, юниорки", 16, 21, 
                        17, 25));
                    ageGroupLst.Add(new AgeGroup(AgeGroupEnm.Adults, disciplineSubGroupId, "Мужчины, женщины", 22, null));
                    ageGroupLst.Add(new AgeGroup(AgeGroupEnm.Veterans, disciplineSubGroupId, "Ветераны", 60, null));
                    break;
            }
        }

        result = repository.AddRange(ageGroupLst);
        if (!result)
            return Result<List<AgeGroup>>.Fail(result.Excptn!);
        
        result = await repository.SaveChangesAsync();
        if (!result)
            return Result<List<AgeGroup>>.Fail(result.Excptn!);

        return Result<List<AgeGroup>>.Done(ageGroupLst);
    }
}