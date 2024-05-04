namespace ProblemDomain.Entities.LibraryEntities.Enums;

/// <summary>
/// Типы спортивных юнитов.
/// </summary>
public enum SportUnitTypeEnm
{
    /// <summary>
    /// Группа (маршрут).
    /// </summary>
    TrekGroup = 1,

    /// <summary>
    /// Спортсмен (индивидуальное прохождение).
    /// </summary>
    Athlete,
    
    /// <summary>
    /// Связка.
    /// </summary>
    Bunch,
    
    /// <summary>
    /// Группа.
    /// </summary>
    Group,
    
    /// <summary>
    /// Экипаж каяка.
    /// </summary>
    KayakCrew,
    
    /// <summary>
    /// Экипаж байдарки.
    /// </summary>
    TandemKayakCrew,
    
    /// <summary>
    /// Экипаж катамарана 2-ки.
    /// </summary>
    Catamaran2Crew,
    
    /// <summary>
    /// Экипаж катамарана 4-ки.
    /// </summary>
    Catamaran4Crew,
    
    /// <summary>
    /// Совокупность судов (команда) на водных дистанциях.
    /// </summary>
    WaterTeam,
    
    /// <summary>
    /// Команда (Дистанция - комбинированная).
    /// </summary>
    Team,

    /// <summary>
    /// Экипаж парусного судна (Дистанция - парусная).
    /// </summary>
    SailboatCrew,
    
    /// <summary>
    /// Экипаж 2-го парусного судна (Дистанция - парусная).
    /// </summary>
    SailboatCrew2,
    
    /// <summary>
    /// Экипаж 3-го парусного судна (Дистанция - парусная).
    /// </summary>
    SailboatCrew3,
    
    /// <summary>
    /// Экипаж 4-го парусного судна (Дистанция - парусная).
    /// </summary>
    SailboatCrew4,

    /// <summary>
    /// Совокупность судов (команда) на парусных дистанциях.
    /// </summary>
    SailboatTeam,

    // /// <summary>
    // /// Одиночный экипаж автомобиля (Дистанция - на средствах передвижения).
    // /// </summary>
    // Car1Crew,
}