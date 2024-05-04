namespace ProblemDomain.Entities.LibraryEntities.Enums;

/// <summary>
/// Дисциплины.
/// </summary>
public enum DisciplineEnm
{
    #region [----- Маршрут -----]

    /// <summary>
    /// Маршрут - водный (1-6 категория).
    /// </summary>
    TrekWater = 1,
    
    /// <summary>
    /// Маршрут - горный (1-6 категория).
    /// </summary>
    TrekMountain,
    
    /// <summary>
    /// Маршрут - комбинированный (1-6 категория).
    /// </summary>
    TrekCombined,
    
    /// <summary>
    /// Маршрут - лыжный (1-6 категория).
    /// </summary>
    TrekSki,
    
    /// <summary>
    /// Маршрут - на средствах передвижения (1-6 категория).
    /// </summary>
    TrekOnMovementItems,
    
    /// <summary>
    /// Маршрут - парусный (1-6 категория).
    /// </summary>
    TrekSailing,
    
    /// <summary>
    /// Маршрут - пешеходный (1-6 категория).
    /// </summary>
    TrekHiking,
    
    /// <summary>
    /// Маршрут - спелео (1-6 категория).
    /// </summary>
    TrekSpeleo,
    
    #endregion
    
    #region [----- Дистанция -----]
    
    /// <summary>
    /// Дистанция - водная - каяк.
    /// </summary>
    DistanceWaterKayak,
    
    /// <summary>
    /// Дистанция - водная - байдарка.
    /// </summary>
    DistanceWaterTandemKayak,

    /// <summary>
    /// Дистанция - водная - катамаран 2.
    /// </summary>
    DistanceWaterCatamaran2,

    /// <summary>
    /// Дистанция - водная - катамаран 4.
    /// </summary>
    DistanceWaterCatamaran4,

    /// <summary>
    /// Дистанция - водная - командная гонка.
    /// </summary>
    DistanceWaterTeamRace,
    
    
    /// <summary>
    /// Дистанция - горная - группа.
    /// </summary>
    DistanceMountainGroup,
    
    /// <summary>
    /// Дистанция - горная - связка.
    /// </summary>
    DistanceMountainBunch,
    
    
    /// <summary>
    /// Дистанция - комбинированная.
    /// </summary>
    DistanceCombined,
    
    
    /// <summary>
    /// Дистанция - лыжная - группа.
    /// </summary>
    DistanceSkiGroup,
    
    /// <summary>
    /// Дистанция - лыжная - связка.
    /// </summary>
    DistanceSkiBunch,
    
    /// <summary>
    /// Дистанция - лыжная.
    /// </summary>
    DistanceSki,
    
    
    /// <summary>
    /// Дистанция - на средствах передвижения - группа.
    /// </summary>
    DistanceOnMovementItemsGroup,
    
    /// <summary>
    /// Дистанция - на средствах передвижения.
    /// </summary>
    DistanceOnMovementItems,
    
    
    /// <summary>
    /// Дистанция - парусная.
    /// </summary>
    DistanceSailing,
    
    
    /// <summary>
    /// Дистанция - пешеходная - группа.
    /// </summary>
    DistanceHikingGroup,
    
    /// <summary>
    /// Дистанция - пешеходная - связка.
    /// </summary>
    DistanceHikingBunch,
    
    /// <summary>
    /// Дистанция - пешеходная.
    /// </summary>
    DistanceHiking,
    
    
    /// <summary>
    /// Дистанция - спелео - группа.
    /// </summary>
    DistanceSpeleoGroup,
    
    /// <summary>
    /// Дистанция - спелео - связка.
    /// </summary>
    DistanceSpeleoBunch,
    
    /// <summary>
    /// Дистанция - спелео.
    /// </summary>
    DistanceSpeleo,

    #endregion
    
    #region [----- Ходьба -----]
    
    /// <summary>
    /// Северная ходьба.
    /// </summary>
    NordicWalking
    
    #endregion
}