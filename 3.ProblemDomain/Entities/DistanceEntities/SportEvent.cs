using System;
using System.Collections.Generic;
using ProblemDomain.Entities._Contracts;
using ProblemDomain.Entities.LibraryEntities;
using ProblemDomain.Entities.LibraryEntities.Enums;
// ReSharper disable InvalidXmlDocComment

namespace ProblemDomain.Entities.DistanceEntities;

/// <summary>
/// Вид программы.
/// </summary>
public sealed class SportEvent
    : AbstractEntity, ICloneable, ICopy
{
    /// <summary>
    /// Конструктор для EF.
    /// </summary>
    /// <inheritdoc />
    /// <param name="isShort">Признак короткой дистанции.</param>
    /// <param name="difficulty">Категория сложности маршрута или класс дистанции.</param>
    private SportEvent(string name, bool? isShort, Difficulty.IdEnm difficulty, string? description = null) 
        : base(name, description)
    {
        IsShort = isShort;
        Difficulty = difficulty;
    }

    /// <summary>
    /// Конструктор на основе готового экземпляра.
    /// </summary>
    private SportEvent(SportEvent sportEvent)
        : this(
            sportEvent.Name,
            sportEvent.IsShort,
            sportEvent.Difficulty,
            sportEvent.Discipline,
            sportEvent.Description
        )
    {
    }
    
    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <inheritdoc />
    /// <param name="discipline">Дисциплина.</param>
    public SportEvent(string name, bool? isShort, Difficulty.IdEnm difficulty, Discipline discipline,
        string? description = null) 
        : this(name, isShort, difficulty, description)
    {
        Discipline = discipline;
    }

    /// <summary>
    /// Признак короткой дистанции.
    /// </summary>
    public bool? IsShort { get; set; }

    /// <summary>
    /// Категория сложности маршрута или класс дистанции.
    /// </summary>
    public Difficulty.IdEnm Difficulty { get; set; }


    /// <summary>
    /// Связь с дисциплиной (объектом-владельцем).
    /// </summary>
    public DisciplineEnm DisciplineId { get; set; }

    /// <inheritdoc cref="DisciplineId"/>
    public Discipline Discipline { get; set; } = null!;
    
    /// <summary>
    /// Коллекция спортивных юнитов.
    /// </summary>
    // ReSharper disable once CollectionNeverUpdated.Global
    public ICollection<SportUnit> SportUnits { get; set; } = new HashSet<SportUnit>();
    
    /// <summary>
    /// Клонирование.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public SportEvent Clone()
        => new(this);
    
    /// <inheritdoc />
    object ICloneable.Clone() {
        return Clone();
    }
    
    /// <inheritdoc cref="ICopy.Copy"/>
    public void Copy(SportEvent destination)
    {
        destination.Name = Name;
        destination.IsShort = IsShort;
        destination.Difficulty = Difficulty;
        destination.Discipline = Discipline;
        destination.Description = Description;
    }
    
    /// <inheritdoc />
    void ICopy.Copy(IAbstractEntity destination)
    {
        Copy((SportEvent)destination);
    }
}