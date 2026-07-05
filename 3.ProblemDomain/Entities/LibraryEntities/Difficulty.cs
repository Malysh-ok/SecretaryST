using System;
using Common.BaseExtensions.ValueTypes;
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
        /// 3 ст. сложности.
        /// </summary>
        LowThird = -3,
        
        /// <summary>
        /// 3 ст. сложности.
        /// </summary>
        LowSecond,
        
        /// <summary>
        /// 3 ст. сложности.
        /// </summary>
        LowFirst,
        
        /// <summary>
        /// 1 класс дистанции (1 кат. сложности).
        /// </summary>
        First = 1,
        
        /// <summary>
        /// 2 класс дистанции (2 кат. сложности).
        /// </summary>
        Second,
        
        /// <summary>
        /// 3 класс дистанции (3 кат. сложности).
        /// </summary>
        Third,
        
        /// <summary>
        /// 4 класс дистанции (4 кат. сложности).
        /// </summary>
        Fourth,
        
        /// <summary>
        /// 5 класс дистанции (5 кат. сложности).
        /// </summary>
        Fifth,
        
        /// <summary>
        /// 6 класс дистанции (6 кат. сложности).
        /// </summary>
        Sixth
    }

    /// <summary>
    /// Численное значение трудности.
    /// </summary>
    /// <remarks>
    /// 1 - 6 к.с. для маршрутов, 1 - 6 класс дистанций (в том числе для сев. ходьбы),
    /// -3 - -1 ст.с. для маршрутов.
    /// </remarks>
    /// <param name="id">Вариант трудности.</param>
    public static int GetValue(this IdEnm id)
    {
        return id.ToInt();
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

