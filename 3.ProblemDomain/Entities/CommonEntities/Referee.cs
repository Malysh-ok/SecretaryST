using System;
using System.Collections.Generic;
using ProblemDomain.Entities._Contracts;
using ProblemDomain.Entities.LibraryEntities;
using ProblemDomain.Entities.LibraryEntities.Enums;
// ReSharper disable InvalidXmlDocComment
// ReSharper disable MemberCanBePrivate.Global

namespace ProblemDomain.Entities.CommonEntities;

/// <summary>
/// Судья.
/// </summary>
public sealed class Referee
    : AbstractPersonalityEntity, ICloneable, ICopy
{
    /// <inheritdoc cref="AbstractEntity.Id"/>
    public new RefereeJobTitleEnm Id { get; set; }
    
    /// <summary>
    /// Конструктор для EF.
    /// </summary>
    /// <param name="id">Идентификатор.</param>
    /// <inheritdoc />
    /// <param name="domicile">Место жительства.</param>
    private Referee(RefereeJobTitleEnm id, string lastName, string firstName, string domicile,
        string? patronymic = null, string? description = null) 
        : base(lastName, firstName, patronymic, description)
    {
        Id = id;
        Domicile = domicile;
    }
    
    /// <summary>
    /// Конструктор на основе готового экземпляра.
    /// </summary>
    private Referee(Referee referee)
        : this(
            referee.Id,
            referee.LastName,
            referee.FirstName,
            referee.Domicile,
            referee.RefereeLevel,
            referee.RefereeJobTitle,
            referee.Patronymic,
            referee.Description
        )
    {
    }
    
    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <inheritdoc />
    /// <param name="refereeLevel">Судейская категория.</param>
    /// <param name="refereeJobTitle">Судейская должность.</param>
    public Referee(RefereeJobTitleEnm id, string lastName, string firstName, string domicile, 
        RefereeLevel refereeLevel, RefereeJobTitle refereeJobTitle, 
        string? patronymic = null, string? description = null) 
        : this(id, lastName, firstName, domicile, patronymic, description)
    {
        RefereeLevel = refereeLevel;
        RefereeJobTitle = refereeJobTitle;
    }
    
    /// <summary>
    /// Место жительства.
    /// </summary>
    public string Domicile { get; set; }
    
    /// <summary>
    /// Связь с судейской категорией (объектом-владельцем).
    /// </summary>
    public RefereeLevelEnm RefereeLevelId { get; set; }

    /// <inheritdoc cref="RefereeLevelId"/>
    public RefereeLevel RefereeLevel { get; set; } = null!;
    
    /// <summary>
    /// Связь с судейской должностью (объектом-владельцем).
    /// </summary>
    public RefereeJobTitleEnm RefereeRefereeJobTitleId { get; set; }

    /// <inheritdoc cref="RefereeRefereeJobTitleId"/>
    public RefereeJobTitle RefereeJobTitle { get; set; } = null!;
    
    /// <summary>
    /// Клонирование.
    /// </summary>
    public Referee Clone()
        => new(this);
    
    /// <inheritdoc />
    object ICloneable.Clone() {
        return Clone();
    }
    
    /// <inheritdoc cref="ICopy.Copy"/>
    public void Copy(Referee referee)
    {
        referee.Id = Id;
        referee.LastName = LastName;
        referee.FirstName = FirstName;
        referee.Patronymic = Patronymic;
        referee.Description = Description;
        referee.Domicile = Domicile;
        referee.RefereeLevel = RefereeLevel;
        referee.RefereeJobTitle = RefereeJobTitle;
    }
    
    /// <inheritdoc />
    void ICopy.Copy(IAbstractEntity destination)
    {
        Copy((Referee)destination);
    }
}