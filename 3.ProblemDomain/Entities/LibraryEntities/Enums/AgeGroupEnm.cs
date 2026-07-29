namespace ProblemDomain.Entities.LibraryEntities.Enums;

/// <summary>
/// Перечисление, описывающее возрастные группы.
/// </summary>
public enum AgeGroupEnm
{
    /// <summary>
    /// Мальчики, девочки.
    /// </summary>
    Children = 1, 
    
    /// <summary>
    /// Юноши, девушки.
    /// </summary>
    Youths, 
    
    /// <summary>
    /// Юниоры, юниорки.
    /// </summary>
    Juniors, 
    
    /// <summary>
    /// Мужчины, женщины.
    /// </summary>
    Adults, 
    
    /// <summary>
    /// Ветераны.
    /// </summary>
    /// <remarks>
    /// Неофициальная группа.
    /// </remarks>
    Veterans
}