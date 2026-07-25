using System.Collections.Generic;
using Common.BaseExtensions.Collections;
using ProblemDomain.Entities._Contracts;
using ProblemDomain.Entities.CommonEntities;
using ProblemDomain.Entities.DistanceEntities;
using ProblemDomain.Entities.LibraryEntities.Enums;
// ReSharper disable InvalidXmlDocComment
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace ProblemDomain.Entities.LibraryEntities;

/// <summary>
/// Пол.
/// </summary>
public sealed class Sex : AbstractEntity<SexEnm>, IEntityCopyable
{
    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="id">Идентификатор.</param>
    /// <inheritdoc />
    /// <param name="teamName">Наименование, используемое для спортивного юнита с количеством человек больше одного.</param>
    /// <param name="teamNamePlural">Наименование, используемое для спортивного юнита с количеством человек больше одного, во множественном числе.</param>
    /// <param name="personalityName">Наименование, используемое для личности (человека).</param>
    /// <param name="personalityNamePlural">Наименование, используемое для личности (человека), во множественном числе.</param>
    public Sex(
        SexEnm id, 
        string name, 
        string teamName, 
        string teamNamePlural,
        string? personalityName = null, 
        string? personalityNamePlural = null, 
        string? description = null) : base(name, description)
    {
        Id = id;
        PersonalityName = personalityName;
        PersonalityNamePlural = personalityNamePlural;
        TeamName = teamName;
        TeamNamePlural = teamNamePlural;
        Description = description;
    }

    /// <summary>
    /// Наименование, используемое для личности (человека).
    /// </summary>
    public string? PersonalityName { get; set; }

    /// <summary>
    /// Наименование, используемое для личности (человека), во множественном числе.
    /// </summary>
    public string? PersonalityNamePlural { get; set; }

    /// <summary>
    /// Наименование, используемое для спортивного юнита с количеством человек больше одного.
    /// </summary>
    public string TeamName { get; set; }

    /// <summary>
    /// Наименование, используемое для спортивного юнита с количеством человек больше одного, во множественном числе.
    /// </summary>
    public string TeamNamePlural { get; set; }

    /// <summary>
    /// Возвращает множество валидных значений пола для личности (человека). 
    /// </summary>
    public HashSet<SexEnm> GetValidSexForPersonality()
    {
        return
        [
            SexEnm.Male,
            SexEnm.Female
        ];
    }

    /// <summary>
    /// Возвращает множество валидных значений пола
    /// для конкретного типа спортивного юнита. 
    /// </summary>
    public HashSet<SexEnm> GetValidSex(SportUnitTypeEnm sportUnitTypeEnm)
    {
        return sportUnitTypeEnm switch
        {
            SportUnitTypeEnm.Athlete or SportUnitTypeEnm.KayakCrew =>
            [
                SexEnm.Male,
                SexEnm.Female
            ],
            
            _ => new HashSet<SexEnm>().FromEnum()
        };
    }

    /// <summary>
    /// Список представителей.
    /// </summary>
    // ReSharper disable once CollectionNeverUpdated.Global
    public ICollection<Representative> Representatives { get; set; } = new HashSet<Representative>();

    /// <summary>
    /// Список спортсменов.
    /// </summary>
    // ReSharper disable once CollectionNeverUpdated.Global
    public ICollection<Athlete> Athletes { get; set; } = new HashSet<Athlete>();

    /// <summary>
    /// Список спортивных юнитов.
    /// </summary>
    // ReSharper disable once CollectionNeverUpdated.Global
    public ICollection<SportUnit> SportUnits { get; set; } = new HashSet<SportUnit>();

    /// <inheritdoc cref="IEntityCopyable.Copy"/>
    // ReSharper disable once MemberCanBePrivate.Global
    public void Copy(Sex destination)
    {
        destination.Name = Name;
        destination.Description = Description;
        destination.TeamName = TeamName;
        destination.TeamNamePlural = TeamNamePlural;
        destination.PersonalityName = PersonalityName;
        destination.PersonalityNamePlural = PersonalityNamePlural;
    }

    /// <inheritdoc />
    void IEntityCopyable.Copy(IAbstractEntity destination)
    {
        Copy((Sex)destination);
    }    
}