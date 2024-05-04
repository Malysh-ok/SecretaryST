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
        var competitionsStatusLst = new List<CompetitionsStatus>
        {
            new(CompetitionsStatusEnm.AllRussian, "Всероссийский", "Всероссийские"),
            new(CompetitionsStatusEnm.Interregional, "Межрегиональный", "Межрегиональные"),
            new(CompetitionsStatusEnm.Regional, "Региональный", "Региональные"),
            new(CompetitionsStatusEnm.Municipal, "Муниципальный", "Муниципальные"),
        };
        var isExisting = await repository.IsExistingAsync(competitionsStatusLst);
        if (!isExisting)
        {
            var result = await repository.AddRangeAsync(competitionsStatusLst);
            if (!result)
                return Result<List<CompetitionsStatus>>.Fail(result.Excptn);
        }
        else
        {
            var result = await repository.UpdateRangeAsync(competitionsStatusLst);
            if (!result)
                return Result<List<CompetitionsStatus>>.Fail(result.Excptn);
        }

        return Result<List<CompetitionsStatus>>.Done(competitionsStatusLst);
    }

    /// <summary>
    /// Заполняем статусы и наименования спортивных соревнований.
    /// </summary>
    public static async Task<Result<List<DetailedCompetitionStatus>>> FillDetailedCompetitionStatuses(IRepository repository,
        List<CompetitionsStatus> competitionsStatusLst)
    {
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
            
            new(DetailedCompetitionStatusEnm.RegionalChampionship, "Чемпионат субъекта Российской Федерации,",
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
        var isExisting = await repository.IsExistingAsync(detailedCompetitionsStatusLst);
        if (!isExisting)
        {
            var result = await repository.AddRangeAsync(detailedCompetitionsStatusLst);
            if (!result)
                return Result<List<DetailedCompetitionStatus>>.Fail(result.Excptn);
        }
        else
        {
            var result = await repository.UpdateRangeAsync(detailedCompetitionsStatusLst);
            if (!result)
                return Result<List<DetailedCompetitionStatus>>.Fail(result.Excptn);
        }

        return Result<List<DetailedCompetitionStatus>>.Done(detailedCompetitionsStatusLst);
    }
    
    /// <summary>
    /// Заполняем группы дисциплин.
    /// </summary>
    public static async Task<Result<List<DisciplineGroup>>> FillDisciplineGroups(IRepository repository)
    {
        var disciplineGroupLst = new List<DisciplineGroup>
        {
            new(DisciplineGroupEnm.Trek, "Маршрут"),
            new(DisciplineGroupEnm.Distance, "Дистанция"),
            new(DisciplineGroupEnm.NordicWalking, "Северная ходьба")
        };
        var isExisting = await repository.IsExistingAsync(disciplineGroupLst);
        if (!isExisting)
        {
            var result = await repository.AddRangeAsync(disciplineGroupLst);
            if (!result)
                return Result<List<DisciplineGroup>>.Fail(result.Excptn);
        }
        else
        {
            var result = await repository.UpdateRangeAsync(disciplineGroupLst);
            if (!result)
                return Result<List<DisciplineGroup>>.Fail(result.Excptn);
        }

        return Result<List<DisciplineGroup>>.Done(disciplineGroupLst);
    }
    
    /// <summary>
    /// Заполняем подгруппы дисциплин.
    /// </summary>
    public static async Task<Result<List<DisciplineSubGroup>>> FillDisciplineSubGroups(IRepository repository,
        List<DisciplineGroup> disciplineGroupLst)
    {
        var disciplineSubGroupLst = new List<DisciplineSubGroup>
        {
            new(DisciplineSubGroupEnm.Trek, "",
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.Trek)!),

            new(DisciplineSubGroupEnm.Water, "Водная",
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.Distance)!),
            new(DisciplineSubGroupEnm.Mountain, "Горная",
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.Distance)!),
            new(DisciplineSubGroupEnm.Combined, "Комбинированная",
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.Distance)!),
            new(DisciplineSubGroupEnm.Ski, "Лыжная",
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.Distance)!),
            new(DisciplineSubGroupEnm.OnMovementItems, "На средствах передвижения",
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.Distance)!),
            new(DisciplineSubGroupEnm.Sailing, "Парусная",
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.Distance)!),
            new(DisciplineSubGroupEnm.Hiking, "Пешеходная",
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.Distance)!),
            new(DisciplineSubGroupEnm.Speleo, "Спелео",
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.Distance)!),

            new(DisciplineSubGroupEnm.NordicWalking, "",
                disciplineGroupLst.Find(dg => dg.Id == DisciplineGroupEnm.NordicWalking)!),
        };
        var isExisting = await repository.IsExistingAsync(disciplineSubGroupLst);
        if (!isExisting)
        {
            var result = await repository.AddRangeAsync(disciplineSubGroupLst);
            if (!result)
                return Result<List<DisciplineSubGroup>>.Fail(result.Excptn);
        }
        else
        {
            var result = await repository.UpdateRangeAsync(disciplineSubGroupLst);
            if (!result)
                return Result<List<DisciplineSubGroup>>.Fail(result.Excptn);
        }

        return Result<List<DisciplineSubGroup>>.Done(disciplineSubGroupLst);
    }
    
    /// <summary>
    /// Заполняем дисциплины.
    /// </summary>
    public static async Task<Result<List<Discipline>>> FillDisciplines(IRepository repository,
        List<DisciplineGroup> disciplineGroupLst, List<DisciplineSubGroup> disciplineSubGroupLst)
    {
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
        var isExisting = await repository.IsExistingAsync(disciplineLst);
        if (!isExisting)
        {
            var result = await repository.AddRangeAsync(disciplineLst);
            if (!result)
                return Result<List<Discipline>>.Fail(result.Excptn);
        }
        else
        {
            var result = await repository.UpdateRangeAsync(disciplineLst);
            if (!result)
                return Result<List<Discipline>>.Fail(result.Excptn);
        }

        return Result<List<Discipline>>.Done(disciplineLst);
    }
    
    /// <summary>
    /// Заполняем судейские категории.
    /// </summary>
    public static async Task<Result<List<RefereeLevel>>> FillRefereeLevels(IRepository repository)
    {
        var refereeLevelLst = new List<RefereeLevel>
        {
            new(RefereeLevelEnm.YoungCategory, "ЮСС", "Юный спортивный судья"),
            new(RefereeLevelEnm.Category3, "СС3К", "Спортивный судья третьей категории"),
            new(RefereeLevelEnm.Category2, "СС2К", "Спортивный судья второй категории"),
            new(RefereeLevelEnm.Category1, "СС1К", "Спортивный судья первой категории"),
            new(RefereeLevelEnm.AllRussCategory, "ССВК", "Спортивный судья всероссийской категории"),
        };
        var isExisting = await repository.IsExistingAsync(refereeLevelLst);
        if (!isExisting)
        {
            var result = await repository.AddRangeAsync(refereeLevelLst);
            if (!result)
                return Result<List<RefereeLevel>>.Fail(result.Excptn);
        }
        else
        {
            var result = await repository.UpdateRangeAsync(refereeLevelLst);
            if (!result)
                return Result<List<RefereeLevel>>.Fail(result.Excptn);
        }

        return Result<List<RefereeLevel>>.Done(refereeLevelLst);
    }
    
    /// <summary>
    /// Заполняем судейские должности.
    /// </summary>
    public static async Task<Result<List<RefereeJobTitle>>> FillRefereeJobTitles(IRepository repository)
    {
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
            new(RefereeJobTitleEnm.Secretary, "Судья-секретарь"),
            new(RefereeJobTitleEnm.StarterReferee, "Судья-стартер"),
            new(RefereeJobTitleEnm.TimekeeperReferee, "Судья-хронометрист"),
            new(RefereeJobTitleEnm.InformationReferee, "Судья по информации"),
            new(RefereeJobTitleEnm.InsurerReferee, "Судья-страховщик"),
            new(RefereeJobTitleEnm.AccompanyingReferee, "Судья при участниках"),
            
            new(RefereeJobTitleEnm.MandateChairman, "Председатель комиссии по допуску"),
            new(RefereeJobTitleEnm.TechCommissionChairman, "Председатель технической комиссии"),
        };
        var isExisting = await repository.IsExistingAsync(refereeJobTitlesLst);
        if (!isExisting)
        {
            var result = await repository.AddRangeAsync(refereeJobTitlesLst);
            if (!result)
                return Result<List<RefereeJobTitle>>.Fail(result.Excptn);
        }
        else
        {
            var result = await repository.UpdateRangeAsync(refereeJobTitlesLst);
            if (!result)
                return Result<List<RefereeJobTitle>>.Fail(result.Excptn);
        }

        return Result<List<RefereeJobTitle>>.Done(refereeJobTitlesLst);
    }
    
    /// <summary>
    /// Заполняем варианты пола.
    /// </summary>
    public static async Task<Result<List<Sex>>> FillSexes(IRepository repository)
    {
        var sexLst = new List<Sex>
        {
            new(SexEnm.Male, "м", "мужской", "мужские",
                "мужчина", "мужчины"),
            new(SexEnm.Female, "ж", "женский", "женские", 
                "женщина", "женщины"),
            new(SexEnm.Mixed, "см", "смешанный", "смешанные"),
        };
        var isExisting = await repository.IsExistingAsync(sexLst);
        if (!isExisting)
        {
            var result = await repository.AddRangeAsync(sexLst);
            if (!result)
                return Result<List<Sex>>.Fail(result.Excptn);
        }
        else
        {
            var result = await repository.UpdateRangeAsync(sexLst);
            if (!result)
                return Result<List<Sex>>.Fail(result.Excptn);
        }

        return Result<List<Sex>>.Done(sexLst);
    }
    
    /// <summary>
    /// Заполняем типы спортивных юнитов.
    /// </summary>
    public static async Task<Result<List<SportUnitType>>> FillSportUnitType(IRepository repository)
    {
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
        var isExisting = await repository.IsExistingAsync(sportUnitTypeLst);
        if (!isExisting)
        {
            var result = await repository.AddRangeAsync(sportUnitTypeLst);
            if (!result)
                return Result<List<SportUnitType>>.Fail(result.Excptn);
        }
        else
        {
            var result = await repository.UpdateRangeAsync(sportUnitTypeLst);
            if (!result)
                return Result<List<SportUnitType>>.Fail(result.Excptn);
        }

        return Result<List<SportUnitType>>.Done(sportUnitTypeLst);
    }
    
}