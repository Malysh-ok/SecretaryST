using AppDomain.UseCases._Contracts;
using Common.BaseComponents.Components;
using ProblemDomain.Entities.LibraryEntities;
using ProblemDomain.Entities.LibraryEntities.Enums;

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
            new(DisciplineEnm.TrekWater, "Маршрут - водный (1-6 категория)",
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.Trek)!,
                disciplineSubGroupLst.Find(dsg => dsg.Id == DisciplineSubGroupEnm.Trek)!),

            new(DisciplineEnm.TrekMountain, "Маршрут - горный (1-6 категория)",
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.Trek)!,
                disciplineSubGroupLst.Find(dsg => dsg.Id == DisciplineSubGroupEnm.Trek)!),

            new(DisciplineEnm.TrekCombined, "Маршрут - комбинированный (1-6 категория)",
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.Trek)!,
                disciplineSubGroupLst.Find(dsg => dsg.Id == DisciplineSubGroupEnm.Trek)!),

            new(DisciplineEnm.TrekSki, "Маршрут - лыжный (1-6 категория)",
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.Trek)!,
                disciplineSubGroupLst.Find(dsg => dsg.Id == DisciplineSubGroupEnm.Trek)!),

            new(DisciplineEnm.TrekOnMovementItems, "Маршрут - на средствах передвижения (1-6 категория)",
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.Trek)!,
                disciplineSubGroupLst.Find(dsg => dsg.Id == DisciplineSubGroupEnm.Trek)!),

            new(DisciplineEnm.TrekSailing, "Маршрут - парусный (1-6 категория)",
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.Trek)!,
                disciplineSubGroupLst.Find(dsg => dsg.Id == DisciplineSubGroupEnm.Trek)!),

            new(DisciplineEnm.TrekHiking, "Маршрут - пешеходный (1-6 категория)",
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.Trek)!,
                disciplineSubGroupLst.Find(dsg => dsg.Id == DisciplineSubGroupEnm.Trek)!),

            new(DisciplineEnm.TrekSpeleo, "Маршрут - спелео (1-6 категория)",
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.Trek)!,
                disciplineSubGroupLst.Find(dsg => dsg.Id == DisciplineSubGroupEnm.Trek)!),


            new(DisciplineEnm.DistanceWaterKayak, "Дистанция - водная - каяк",
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.Distance)!,
                disciplineSubGroupLst.Find(dsg => dsg.Id == DisciplineSubGroupEnm.Water)!),

            new(DisciplineEnm.DistanceWaterTandemKayak, "Дистанция - водная - байдарка",
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.Distance)!,
                disciplineSubGroupLst.Find(dsg => dsg.Id == DisciplineSubGroupEnm.Water)!),

            new(DisciplineEnm.DistanceWaterCatamaran2, "Дистанция - водная - катамаран 2",
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.Distance)!,
                disciplineSubGroupLst.Find(dsg => dsg.Id == DisciplineSubGroupEnm.Water)!),

            new(DisciplineEnm.DistanceWaterCatamaran4, "Дистанция - водная - катамаран 4",
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.Distance)!,
                disciplineSubGroupLst.Find(dsg => dsg.Id == DisciplineSubGroupEnm.Water)!),

            new(DisciplineEnm.DistanceWaterTeamRace, "Дистанция - водная - командная гонка",
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.Distance)!,
                disciplineSubGroupLst.Find(dsg => dsg.Id == DisciplineSubGroupEnm.Water)!),


            new(DisciplineEnm.DistanceMountainGroup, "Дистанция - горная - группа",
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.Distance)!,
                disciplineSubGroupLst.Find(dsg => dsg.Id == DisciplineSubGroupEnm.Mountain)!),

            new(DisciplineEnm.DistanceMountainBunch, "Дистанция - горная - связка",
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.Distance)!,
                disciplineSubGroupLst.Find(dsg => dsg.Id == DisciplineSubGroupEnm.Mountain)!),


            new(DisciplineEnm.DistanceCombined, "Дистанция - комбинированная",
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.Distance)!,
                disciplineSubGroupLst.Find(dsg => dsg.Id == DisciplineSubGroupEnm.Combined)!),


            new(DisciplineEnm.DistanceSkiGroup, "Дистанция - лыжная - группа",
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.Distance)!,
                disciplineSubGroupLst.Find(dsg => dsg.Id == DisciplineSubGroupEnm.Ski)!),

            new(DisciplineEnm.DistanceSkiBunch, "Дистанция - лыжная - связка",
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.Distance)!,
                disciplineSubGroupLst.Find(dsg => dsg.Id == DisciplineSubGroupEnm.Ski)!),

            new(DisciplineEnm.DistanceSki, "Дистанция - лыжная",
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.Distance)!,
                disciplineSubGroupLst.Find(dsg => dsg.Id == DisciplineSubGroupEnm.Ski)!),


            new(DisciplineEnm.DistanceOnMovementItemsGroup, "Дистанция - на средствах передвижения - группа",
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.Distance)!,
                disciplineSubGroupLst.Find(dsg => dsg.Id == DisciplineSubGroupEnm.OnMovementItems)!),

            new(DisciplineEnm.DistanceOnMovementItems, "Дистанция - на средствах передвижения",
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.Distance)!,
                disciplineSubGroupLst.Find(dsg => dsg.Id == DisciplineSubGroupEnm.OnMovementItems)!),


            new(DisciplineEnm.DistanceSailing, "Дистанция - парусная",
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.Distance)!,
                disciplineSubGroupLst.Find(dsg => dsg.Id == DisciplineSubGroupEnm.Sailing)!),


            new(DisciplineEnm.DistanceHikingGroup, "Дистанция - пешеходная - группа",
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.Distance)!,
                disciplineSubGroupLst.Find(dsg => dsg.Id == DisciplineSubGroupEnm.Hiking)!),

            new(DisciplineEnm.DistanceHikingBunch, "Дистанция - пешеходная - связка",
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.Distance)!,
                disciplineSubGroupLst.Find(dsg => dsg.Id == DisciplineSubGroupEnm.Hiking)!),

            new(DisciplineEnm.DistanceHiking, "Дистанция - пешеходная",
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.Distance)!,
                disciplineSubGroupLst.Find(dsg => dsg.Id == DisciplineSubGroupEnm.Hiking)!),


            new(DisciplineEnm.DistanceSpeleoGroup, "Дистанция - спелео - группа",
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.Distance)!,
                disciplineSubGroupLst.Find(dsg => dsg.Id == DisciplineSubGroupEnm.Speleo)!),

            new(DisciplineEnm.DistanceSpeleoBunch, "Дистанция - спелео - связка",
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.Distance)!,
                disciplineSubGroupLst.Find(dsg => dsg.Id == DisciplineSubGroupEnm.Speleo)!),

            new(DisciplineEnm.DistanceSpeleo, "Дистанция - спелео",
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.Distance)!,
                disciplineSubGroupLst.Find(dsg => dsg.Id == DisciplineSubGroupEnm.Speleo)!),


            new(DisciplineEnm.NordicWalking, "Северная ходьба",
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.NordicWalking)!,
                disciplineSubGroupLst.Find(dsg => dsg.Id == DisciplineSubGroupEnm.NordicWalking)!),
        };

        result = repository.AddRange(disciplineLst);
        if (!result)
            return Result<List<Discipline>>.Fail(result.Excptn!);
        
        result = await repository.SaveChangesAsync();
        if (!result)
            return Result<List<Discipline>>.Fail(result.Excptn!);

        return Result<List<Discipline>>.Done(disciplineLst);
    }
    
    /// <summary>
    /// Заполняем судейские категории.
    /// </summary>
    public static async Task<Result<List<RefereeLevel>>> FillRefereeLevels(IRepository repository)
    {
        // Удаляем судейские категории
        var result = repository.RemoveAllQuickly<RefereeLevel>();
        if (!result)
            return Result<List<RefereeLevel>>.Fail(result.Excptn!);

        var refereeLevelLst = new List<RefereeLevel>
        {
            new(RefereeLevelEnm.YoungCategory, "ЮСС", "Юный спортивный судья"),
            new(RefereeLevelEnm.Category3, "СС3К", "Спортивный судья третьей категории"),
            new(RefereeLevelEnm.Category2, "СС2К", "Спортивный судья второй категории"),
            new(RefereeLevelEnm.Category1, "СС1К", "Спортивный судья первой категории"),
            new(RefereeLevelEnm.AllRussCategory, "ССВК", "Спортивный судья всероссийской категории"),
        };

        result = repository.AddRange(refereeLevelLst);
        if (!result)
            return Result<List<RefereeLevel>>.Fail(result.Excptn!);
        
        result = await repository.SaveChangesAsync();
        if (!result)
            return Result<List<RefereeLevel>>.Fail(result.Excptn!);

        return Result<List<RefereeLevel>>.Done(refereeLevelLst);
    }
    
    /// <summary>
    /// Заполняем судейские должности.
    /// </summary>
    public static async Task<Result<List<RefereeJobTitle>>> FillRefereeJobTitles(IRepository repository)
    {
        // Удаляем судейские должности
        var result = repository.RemoveAllQuickly<RefereeJobTitle>();
        if (!result)
            return Result<List<RefereeJobTitle>>.Fail(result.Excptn!);

        var refereeJobTitlesLst = new List<RefereeJobTitle>
        {
            new(RefereeJobTitleEnm.ChiefReferee, "Главный судья"),
            new(RefereeJobTitleEnm.ChiefSecretary, "Главный секретарь"),
            new(RefereeJobTitleEnm.DeputyForRefereeing, "Зам. гл. судьи по судейству"),
            new(RefereeJobTitleEnm.DeputyForSecurity, "Зам. гл. судьи по безопасности"),
            new(RefereeJobTitleEnm.DeputyForInformation, "Зам. гл. судьи по информации"),
            new(RefereeJobTitleEnm.DeputyForSTS, "Зам. гл. судьи по СТО"),
            new(RefereeJobTitleEnm.DeputyChiefSecretary, "Зам. гл. секретаря"),
            new(RefereeJobTitleEnm.Deputy, "Зам. гл. судьи"),
            new(RefereeJobTitleEnm.HeadOfDistance, "Начальник дистанции"),
            new(RefereeJobTitleEnm.MajorInspector, "Старший судья-инспектор"),
            new(RefereeJobTitleEnm.Inspector, "Судья-инспектор"),
            new(RefereeJobTitleEnm.MajorStartReferee, "Старший судья старта"),
            new(RefereeJobTitleEnm.MajorStageReferee, "Старший судья этапа"),
            new(RefereeJobTitleEnm.MajorFinishReferee, "Старший судья финиша"),
            new(RefereeJobTitleEnm.StageReferee, "Судья этапа"),
            new(RefereeJobTitleEnm.InstallerReferee, "Судья-постановщик"),
            new(RefereeJobTitleEnm.Secretary, "Судья-секретарь"),           // REMARK: в Правилах - "Судья секретарь", что неправильно
            new(RefereeJobTitleEnm.StarterReferee, "Судья-стартер"),
            new(RefereeJobTitleEnm.TimekeeperReferee, "Судья-хронометрист"),
            new(RefereeJobTitleEnm.InformationReferee, "Судья по информации"),
            new(RefereeJobTitleEnm.InsurerReferee, "Судья-страховщик"),
            new(RefereeJobTitleEnm.AccompanyingReferee, "Судья при участниках"),
            
            new(RefereeJobTitleEnm.MandateChairman, "Предс. комиссии по допуску"),
            new(RefereeJobTitleEnm.TechCommissionChairman, "Предс. техн. комиссии"),
        };

        result = repository.AddRange(refereeJobTitlesLst);
        if (!result)
            return Result<List<RefereeJobTitle>>.Fail(result.Excptn!);
        
        result = await repository.SaveChangesAsync();
        if (!result)
            return Result<List<RefereeJobTitle>>.Fail(result.Excptn!);

        return Result<List<RefereeJobTitle>>.Done(refereeJobTitlesLst);
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
}