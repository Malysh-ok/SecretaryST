using System;
using System.Collections.Generic;
using ProblemDomain.Entities._Contracts;
using ProblemDomain.Entities.CommonEntities;
using ProblemDomain.Entities.LibraryEntities;
using ProblemDomain.Entities.LibraryEntities.Enums;
// ReSharper disable InvalidXmlDocComment

namespace ProblemDomain.Entities.DistanceEntities;

/// <summary>
/// Спортивный юнит.
/// </summary>
public sealed class SportUnit
    : AbstractEntity, ICloneable, ICopy
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
    private SportUnit(SportUnit sportUnit)
        : this(
            sportUnit.Name,
            sportUnit.Sex,
            sportUnit.SportUnitType,
            sportUnit.SportEvent,
            sportUnit.Description,
            sportUnit.ParentSportUnit
        )
    {
    }
    
    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <inheritdoc />
    /// <param name="sex">Пол.</param>
    /// <param name="sportUnitType">Тип спортивного юнита.</param>
    /// <param name="sportEvent">Вид программы.</param>
    /// <param name="parentSportUnit">Родительский спортивный юнит.</param>
    public SportUnit(string name, Sex sex, SportUnitType sportUnitType, SportEvent sportEvent, 
        string? description = null, SportUnit? parentSportUnit = null) 
        : this(name, description)
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
    object ICloneable.Clone() {
        return Clone();
    }
    
    /// <inheritdoc cref="ICopy.Copy"/>
    // ReSharper disable once MemberCanBePrivate.Global
    public void Copy(SportUnit destination)
    {
        destination.Name = Name;
        destination.Sex = Sex;
        destination.SportUnitType = SportUnitType;
        destination.SportEvent = SportEvent;
        destination.Description = Description;
        destination.ParentSportUnit = ParentSportUnit;
    }
    
    /// <inheritdoc />
    void ICopy.Copy(IAbstractEntity destination)
    {
        Copy((SportUnit)destination);
    }
}