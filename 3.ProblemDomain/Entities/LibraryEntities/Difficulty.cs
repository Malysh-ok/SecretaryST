using System;
using ProblemDomain.Entities.LibraryEntities.Enums;

namespace ProblemDomain.Entities.LibraryEntities;

/// <summary>
/// Трудность - категория сложности маршрута или класс дистанции.
/// </summary>
/// <summary>
/// Не используется в EF и не сохраняется в репозитории.
/// </summary>
public static class Difficulty
{
    /// <summary>
    /// Перечисление, описывающее варианты трудности.
    /// </summary>
    public enum IdEnm
    {
        /// <summary>
        /// 3 ст. с.
        /// </summary>
        LowThird = -3,
        
        /// <summary>
        /// 3 ст. с.
        /// </summary>
        LowSecond,
        
        /// <summary>
        /// 3 ст. с.
        /// </summary>
        LowFirst,
        
        First = 1,
        
        Second,
        
        Third,
        
        Fourth,
        
        Fifth,
        
        Sixth
    }

    /// <summary>
    /// Численное значение трудности.
    /// </summary>
    /// <remarks>
    /// 1 - 6 к.с. для маршрутов, 1 - 6 класс дистанций (в том числе для сев. ходьбы),
    /// 1 - 3 ст.с. для маршрутов.
    /// </remarks>
    /// <param name="id">Вариант трудности.</param>
    public static int GetValue(this IdEnm id)
    {
        return Math.Abs((int)id);
    }
    
    /// <summary>
    /// Признак валидности варианта трудности, в зависимости от группы дисциплин.
    /// </summary>
    /// <param name="id">Вариант трудности.</param>
    /// <param name="disciplineGroupId">Группа дисциплин.</param>
    public static bool IsValid(this IdEnm id, DisciplineGroupEnm disciplineGroupId)
    {
        var minValue = disciplineGroupId == DisciplineGroupEnm.Trek 
            ? IdEnm.LowThird 
            : IdEnm.First;

        return id >= minValue && id <= IdEnm.Sixth && (int)id != 0;
    }
}

