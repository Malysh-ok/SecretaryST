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
public sealed class SportEvent : AbstractEntity<int>, INumberedEntity, IEntityCloneable, IEntityCopyable
{
    /// <summary>
    /// Конструктор для EF.
    /// </summary>
    /// <inheritdoc />
    /// <param name="number">Номер.</param>
    /// <param name="isShort">Признак короткой дистанции.</param>
    /// <param name="difficulty">Категория сложности маршрута или класс дистанции.</param>
    private SportEvent(string name, int number, bool? isShort = null, string? description = null) 
        : base(name, description)
    {
        Number = number;
        IsShort = isShort;
    }

    /// <summary>
    /// Конструктор на основе готового экземпляра.
    /// </summary>
    private SportEvent(SportEvent source)
        : this(
            source.Name,
            source.Number,
            source.IsShort,
            source.Description
        )
    {
        DifficultyId = source.DifficultyId;
        DisciplineId = source.DisciplineId;
        CompetitionDataId = source.CompetitionDataId;
    }

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <inheritdoc />
    /// <param name="discipline">Дисциплина.</param>
    /// <param name="competitionData">Соревнование.</param>
    public SportEvent(
        string name,
        int number,
        bool? isShort,
        Difficulty difficulty,
        Discipline discipline,
        AgeGroup ageGroup,
        CompetitionData competitionData,
        string? description = null) : this(name, number, isShort, description)
    {
        Difficulty = difficulty;
        Discipline = discipline;
        AgeGroup = ageGroup;
        DisciplineGroupId = Discipline.DisciplineGroupId;
        DisciplineSubGroupId = Discipline.DisciplineSubGroupId;
        CompetitionData = competitionData;
    }
    
    /// <inheritdoc />
    public int Number { get; set; }
    
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
            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
            DisciplineGroupId = Discipline?.DisciplineGroupId ?? default;
            DisciplineSubGroupId = Discipline?.DisciplineSubGroupId ?? default;
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
    /// Связь с подгруппой дисциплин.
    /// </summary>
    /// <remarks>
    /// Необходимо для составного внешнего ключа к возрастной группе ({ AgeGroupId, DisciplineSubGroupId } -> <see cref="AgeGroup"/>)
    /// </remarks>
    public DisciplineSubGroupEnm DisciplineSubGroupId { get; set; }

    /// <inheritdoc cref="AgeGroupId"/>
    public AgeGroup AgeGroup { get; set; } = null!;

    /// <summary>
    /// Связь с возрастной группой (объектом-владельцем).
    /// </summary>
    public AgeGroupEnm AgeGroupId { get; set; }
    
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
    object IEntityCloneable.Clone() {
        return Clone();
    }
    
    /// <inheritdoc cref="IEntityCopyable.Copy"/>
    // ReSharper disable once MemberCanBePrivate.Global
    public void Copy(SportEvent destination)
    {
        destination.Name = Name;
        destination.Number = Number;
        destination.IsShort = IsShort;
        destination.Description = Description;
    }
    
    /// <inheritdoc />
    void IEntityCopyable.Copy(IAbstractEntity destination)
    {
        Copy((SportEvent)destination);
    }
}