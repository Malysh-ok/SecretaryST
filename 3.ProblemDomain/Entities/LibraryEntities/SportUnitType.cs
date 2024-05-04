using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Common.BaseExtensions.Collections;
using ProblemDomain.Entities._Contracts;
using ProblemDomain.Entities.DistanceEntities;
using ProblemDomain.Entities.LibraryEntities.Enums;
// ReSharper disable InvalidXmlDocComment

namespace ProblemDomain.Entities.LibraryEntities;

/// <summary>
/// Тип спортивного юнита.
/// </summary>
public sealed class SportUnitType
    : AbstractEntity, ICloneable, ICopy
{
    /// <summary>
    /// Конструктор на основе готового экземпляра.
    /// </summary>
    private SportUnitType(SportUnitType sportUnitType)
        : this(
            sportUnitType.Id,
            sportUnitType.Name,
            sportUnitType.AuxName,
            sportUnitType.Description
        )
    {
    }
    
    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="id">Идентификатор.</param>
    /// <inheritdoc />
    /// <param name="auxName">Вспомогательное наименование.</param>
    [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
    public SportUnitType(SportUnitTypeEnm id, string name, 
        string? auxName = null, string? description = null) 
        : base(name, description)
    {
        Id = id;
        AuxName = auxName;
    }
    
    
    /// <inheritdoc cref="AbstractEntity.Id"/>
    public new SportUnitTypeEnm Id { get; set; }
    
    /// <summary>
    /// Вспомогательное наименование.
    /// </summary>
    public string? AuxName { get; set; }
    
    /// <summary>
    /// Возвращает множество валидных значений типов спортивных юнитов
    /// для конкретной подгруппы дисциплин <paramref name="disciplineSubGroupEnm"/>.
    /// </summary>
    public static HashSet<SportUnitTypeEnm> GetValidSportUnitType(DisciplineSubGroupEnm disciplineSubGroupEnm)
    {
        var validSportUnitType = new HashSet<SportUnitTypeEnm>();

        validSportUnitType.AddRange(disciplineSubGroupEnm switch
        {
            // Водная
            DisciplineSubGroupEnm.Water => 
                new List<SportUnitTypeEnm>
                {
                    SportUnitTypeEnm.KayakCrew,
                    SportUnitTypeEnm.TandemKayakCrew,
                    SportUnitTypeEnm.Catamaran2Crew,
                    SportUnitTypeEnm.Catamaran4Crew,
                    SportUnitTypeEnm.WaterTeam
                },
            
            // Горная
            DisciplineSubGroupEnm.Mountain =>
                new List<SportUnitTypeEnm>
                {
                    SportUnitTypeEnm.Bunch,
                    SportUnitTypeEnm.Group,
                },
            
            // Комбинированная
            DisciplineSubGroupEnm.Combined =>
                new List<SportUnitTypeEnm>
                {
                    SportUnitTypeEnm.Team,
                },
            
            // Лыжная, пешеходная, спелео
            DisciplineSubGroupEnm.Ski or DisciplineSubGroupEnm.Hiking or DisciplineSubGroupEnm.Speleo  => 
                new List<SportUnitTypeEnm>
                {
                    SportUnitTypeEnm.Athlete,
                    SportUnitTypeEnm.Bunch,
                    SportUnitTypeEnm.Group,
                },
            
            // На средствах передвижения
            DisciplineSubGroupEnm.OnMovementItems =>
                new List<SportUnitTypeEnm>
                {
                    SportUnitTypeEnm.Athlete,
                    SportUnitTypeEnm.Group,
                },
            
            // Парусная
            DisciplineSubGroupEnm.Sailing =>
                new List<SportUnitTypeEnm>
                {
                    SportUnitTypeEnm.SailboatCrew,
                    SportUnitTypeEnm.SailboatCrew2,
                    SportUnitTypeEnm.SailboatCrew3,
                    SportUnitTypeEnm.SailboatCrew4,
                    SportUnitTypeEnm.SailboatTeam,
                },
            
            // Северная ходьба
            DisciplineSubGroupEnm.NordicWalking =>
                new List<SportUnitTypeEnm>
                {
                    SportUnitTypeEnm.Athlete,
                },
            
            _ => new List<SportUnitTypeEnm>()
        });

        return validSportUnitType;
    }

    /// <summary>
    /// Возвращает множество валидных значений типов дочерних спортивных юнитов,
    /// для юнита с конкретным типом <paramref name="sportUnitTypeEnm"/>.
    /// </summary>
    public static HashSet<SportUnitTypeEnm> GetValidChildSportUnitType(SportUnitTypeEnm sportUnitTypeEnm)
    {
        var validSportUnitType = new HashSet<SportUnitTypeEnm>();

        validSportUnitType.AddRange(sportUnitTypeEnm switch
        {
            // Совокупность судов (команда) на водных дистанциях
            SportUnitTypeEnm.WaterTeam =>
                new List<SportUnitTypeEnm>
                {
                    SportUnitTypeEnm.KayakCrew,
                    SportUnitTypeEnm.TandemKayakCrew,
                    SportUnitTypeEnm.Catamaran2Crew,
                    SportUnitTypeEnm.Catamaran4Crew,
                },
            
            // Совокупность судов (команда) на парусных дистанциях
            SportUnitTypeEnm.SailboatTeam =>
                new List<SportUnitTypeEnm>
                {
                    SportUnitTypeEnm.SailboatCrew,
                    SportUnitTypeEnm.SailboatCrew2,
                    SportUnitTypeEnm.SailboatCrew3,
                    SportUnitTypeEnm.SailboatCrew4
                },
            
            _ => new List<SportUnitTypeEnm>()
        });
        
        return validSportUnitType;
    }

    /// <summary>
    /// Список спортивных юнитов.
    /// </summary>
    // ReSharper disable once CollectionNeverUpdated.Global
    public ICollection<SportUnit> SportUnits { get; set; } = new HashSet<SportUnit>();
    
    /// <summary>
    /// Клонирование.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public SportUnitType Clone()
        => new(this);
    
    /// <inheritdoc />
    object ICloneable.Clone() {
        return Clone();
    }
    
    /// <inheritdoc cref="ICopy.Copy"/>
    // ReSharper disable once MemberCanBePrivate.Global
    public void Copy(SportUnitType destination)
    {
        destination.Id = Id;
        destination.Name = Name;
        destination.AuxName = AuxName;
        destination.Description = Description;
    }
    
    /// <inheritdoc />
    void ICopy.Copy(IAbstractEntity destination)
    {
        Copy((SportUnitType)destination);
    }    
}