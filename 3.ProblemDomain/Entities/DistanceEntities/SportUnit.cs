using System.Collections.Generic;
using ProblemDomain.Entities._Contracts;
using ProblemDomain.Entities.CommonEntities;
using ProblemDomain.Entities.LibraryEntities;
using ProblemDomain.Entities.LibraryEntities.Enums;
// ReSharper disable InvalidXmlDocComment
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace ProblemDomain.Entities.DistanceEntities;

/// <summary>
/// Спортивный юнит.
/// </summary>
public sealed class SportUnit : AbstractEntity<int>, IEntityCloneable, IEntityCopyable
{
    /// <summary>
    /// Конструктор для EF.
    /// </summary>
    /// <inheritdoc />
    private SportUnit(string name, string? description = null) 
        : base(name, description)
    {
        Sex = null!;
        SportUnitType = null!;
        SportEvent = null!;
    }

    /// <summary>
    /// Конструктор на основе готового экземпляра.
    /// </summary>
    private SportUnit(SportUnit source)
        : this(
            source.Name,
            source.Description
        )
    {
        SexId = source.SexId;
        SportUnitTypeId = source.SportUnitTypeId;
        SportEventId = source.SportEventId;
        ParentSportUnitId = source.ParentSportUnitId;
    }
    
    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <inheritdoc />
    /// <param name="sex">Пол.</param>
    /// <param name="sportUnitType">Тип спортивного юнита.</param>
    /// <param name="sportEvent">Вид программы.</param>
    /// <param name="parentSportUnit">Родительский спортивный юнит.</param>
    public SportUnit(
        string name, 
        Sex sex, 
        SportUnitType sportUnitType, 
        SportEvent sportEvent, 
        SportUnit? parentSportUnit = null,
        string? description = null) : this(name, description)
    {
        Sex = sex;
        SportUnitType = sportUnitType;
        SportEvent = sportEvent;
        ParentSportUnit = parentSportUnit;
    }
    
    /// <summary>
    /// Связь с полом (объектом-владельцем).
    /// </summary>
    public SexEnm SexId { get; set; }
    /// <inheritdoc cref="SexId"/>
    public Sex Sex { get; set; }
    
    /// <summary>
    /// Связь с типом спортивных юнитов (объектом-владельцем).
    /// </summary>
    public SportUnitTypeEnm SportUnitTypeId { get; set; }
    /// <inheritdoc cref="SportUnitTypeId"/>
    public SportUnitType SportUnitType { get; set; }
    
    /// <summary>
    /// Связь с видом программы (объектом-владельцем).
    /// </summary>
    public int SportEventId { get; set; }
    /// <inheritdoc cref="SportEventId"/>
    public SportEvent SportEvent { get; set; }
    
    /// <summary>
    /// Связь с родительским спортивным юнитом (объектом-владельцем).
    /// </summary>
    public int? ParentSportUnitId { get; set; }         // обязательно должен быть nullable!
    /// <inheritdoc cref="ParentSportUnitId"/>
    public SportUnit? ParentSportUnit { get; set; }
    
    /// <summary>
    /// Список спортсменов.
    /// </summary>
    // ReSharper disable once CollectionNeverUpdated.Global
    public ICollection<Athlete> Athletes { get; set; } = new HashSet<Athlete>();
    
    /// <summary>
    /// Список дочерних спортивных юнитов.
    /// </summary>
    public ICollection<SportUnit> ChildSportUnits { get; set; } = new HashSet<SportUnit>();
    
    /// <summary>
    /// Клонирование.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public SportUnit Clone()
        => new(this);
    
    /// <inheritdoc />
    object IEntityCloneable.Clone() {
        return Clone();
    }
    
    /// <inheritdoc cref="IEntityCopyable.Copy"/>
    // ReSharper disable once MemberCanBePrivate.Global
    public void Copy(SportUnit destination)
    {
        destination.Name = Name;
        destination.Description = Description;
    }
    
    /// <inheritdoc />
    void IEntityCopyable.Copy(IAbstractEntity destination)
    {
        Copy((SportUnit)destination);
    }
}