using System;
using System.Diagnostics.CodeAnalysis;
using ProblemDomain.Entities._Contracts;
using ProblemDomain.Entities.DistanceEntities;
using ProblemDomain.Entities.LibraryEntities;
using ProblemDomain.Entities.LibraryEntities.Enums;
// ReSharper disable InvalidXmlDocComment

namespace ProblemDomain.Entities.CommonEntities;

/// <summary>
/// Спортсмен.
/// </summary>
public sealed class Athlete
    : AbstractPersonalityEntity, ICloneable, ICopy
{
    /// <summary>
    /// Конструктор для EF.
    /// </summary>
    /// <inheritdoc />
    /// <param name="birthDate">День рождения.</param>
    private Athlete(string lastName, string firstName, DateTime birthDate,
        string? patronymic = null, string? description = null) 
        : base(lastName, firstName, patronymic, description)
    {
        BirthDate = birthDate;
    }
    
    /// <summary>
    /// Конструктор на основе готового экземпляра.
    /// </summary>
    private Athlete(Athlete delegation)
        : this(
            delegation.LastName,
            delegation.FirstName,
            delegation.BirthDate,
            delegation.Sex,
            delegation.Delegation,
            delegation.SportUnit,
            delegation.Patronymic,
            delegation.Description
        )
    {
    }
    
    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <inheritdoc />
    /// <param name="sex">Пол.</param>
    /// <param name="delegation">Делегация.</param>
    /// <param name="sportUnit">Спортивный юнит.</param>
    [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
    public Athlete(string lastName, string firstName, DateTime birthDate, 
        Sex sex, Delegation delegation, SportUnit sportUnit,
        string? patronymic = null, string? description = null) 
        : this(lastName, firstName, birthDate, patronymic, description)
    {
        Sex = sex;
        Delegation = delegation;
        SportUnit = sportUnit;
        
        // TODO: Как-то проверять кол-во спортсменов и (возможно) пол для конкретного sportUnit.
    }

    /// <summary>
    /// Дата рождения.
    /// </summary>
    public DateTime BirthDate { get; set; }
    
    /// <summary>
    /// Связь с полом (объектом-владельцем).
    /// </summary>
    public SexEnm SexId { get; set; }

    /// <inheritdoc cref="SexId"/>
    public Sex Sex { get; set; } = null!;
    
    /// <summary>
    /// Связь с делегацией (объектом-владельцем).
    /// </summary>
    public int DelegationId { get; set; }
    /// <summary>
    /// <inheritdoc cref="DelegationId"/>
    /// </summary>
    public Delegation Delegation { get; set; } = null!;
    
    /// <summary>
    /// Связь со спортивным юнитом (объектом-владельцем).
    /// </summary>
    public int SportUnitId { get; set; }
    /// <inheritdoc cref="SportUnitId"/>
    public SportUnit SportUnit { get; set; } = null!;
    
    /// <summary>
    /// Клонирование.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public Athlete Clone()
        => new(this);
    
    /// <inheritdoc />
    object ICloneable.Clone() {
        return Clone();
    }
    
    /// <inheritdoc cref="ICopy.Copy"/>
    public void Copy(Athlete destination)
    {
        destination.LastName = LastName;
        destination.FirstName = FirstName;
        destination.Patronymic = Patronymic;
        destination.Description = Description;
        destination.BirthDate = BirthDate;
        destination.Sex = Sex;
        destination.Delegation = Delegation;
        destination.SportUnit = SportUnit;
    }
    
    /// <inheritdoc />
    void ICopy.Copy(IAbstractEntity destination)
    {
        Copy((Athlete)destination);
    }
}
