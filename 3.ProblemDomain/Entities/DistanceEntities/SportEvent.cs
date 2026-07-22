using System;
using System.Collections.Generic;
using ProblemDomain.Entities._Contracts;
using ProblemDomain.Entities.CommonEntities;
using ProblemDomain.Entities.LibraryEntities;
using ProblemDomain.Entities.LibraryEntities.Enums;
// ReSharper disable InvalidXmlDocComment
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace ProblemDomain.Entities.DistanceEntities;

/// <summary>
/// Вид программы.
/// </summary>
public sealed class SportEvent
    : AbstractEntity, ICloneable, ICopyEntity
{
    /// <summary>
    /// Конструктор для EF.
    /// </summary>
    /// <inheritdoc />
    /// <param name="isShort">Признак короткой дистанции.</param>
    /// <param name="difficulty">Категория сложности маршрута или класс дистанции.</param>
    private SportEvent(string name, bool? isShort, string? description = null) 
        : base(name, description)
    {
        IsShort = isShort;
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
            sportEvent.CompetitionData,
            sportEvent.Description
        )
    {
    }

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <inheritdoc />
    /// <param name="discipline">Дисциплина.</param>
    /// <param name="competitionData">Соревнование.</param>
    public SportEvent(string name, bool? isShort, Difficulty difficulty, Discipline discipline,
            CompetitionData competitionData, string? description = null) 
        : this(name, isShort, description)
    {
        Difficulty = difficulty;
        Discipline = discipline;
        DisciplineGroupId = Discipline.DisciplineGroupId;
        CompetitionData = competitionData;
    }

    /// <summary>
    /// Признак короткой дистанции.
    /// </summary>
    public bool? IsShort { get; set; }

    /// <summary>
    /// Связь с трудностью (объектом-владельцем).
    /// </summary>
    /// <remarks>
    /// Категория сложности маршрута или класс дистанции.
    /// </remarks>
    public DifficultyEnm DifficultyId { get; set; }
    
    /// <inheritdoc cref="DisciplineId"/>
    public Difficulty Difficulty { get; set; } = null!;

    /// <summary>
    /// Связь с дисциплиной (объектом-владельцем).
    /// </summary>
    public DisciplineEnm DisciplineId { get; set; }

    /// <inheritdoc cref="DisciplineId"/>
    public Discipline Discipline
    {
        get;
        set
        {
            field = value;
            DisciplineGroupId = Discipline.DisciplineGroupId;
        }
    } = null!;

    /// <summary>
    /// Связь с группой дисциплин.
    /// </summary>
    /// <remarks>
    /// Необходимо для составного внешнего ключа к трудности ({ DifficultyId, DisciplineGroupId } -> <see cref="Difficulty"/>)
    /// </remarks>
    public DisciplineGroupEnm DisciplineGroupId { get; set; }
    
    /// <summary>
    /// Связь с группой дисциплин.
    /// </summary>
    /// <remarks>
    /// Необходимо для составного внешнего ключа к трудности ({ DifficultyId, DisciplineGroupId } -> <see cref="Difficulty"/>)
    /// </remarks>
    public DisciplineGroupEnm DisciplineGroupId { get; set; }
    
    /// <summary>
    /// Связь с соревнованием (объектом-владельцем).
    /// </summary>
    public int CompetitionDataId { get; set; }

    /// <inheritdoc cref="CompetitionDataId"/>
    public CompetitionData CompetitionData { get; set; } = null!;

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
    
    /// <inheritdoc cref="ICopyEntity.Copy"/>
    // ReSharper disable once MemberCanBePrivate.Global
    public void Copy(SportEvent destination)
    {
        destination.Name = Name;
        destination.IsShort = IsShort;
        destination.Difficulty = Difficulty;
        destination.Discipline = Discipline;
        destination.CompetitionData = CompetitionData;
        destination.Description = Description;
    }
    
    /// <inheritdoc />
    void ICopyEntity.Copy(IAbstractEntity destination)
    {
        Copy((SportEvent)destination);
    }
}