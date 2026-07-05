using System;
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
    : AbstractPersonalityEntity, INumberedEntity, ICloneable, ICopyEntity
{
    /// <summary>
    /// Конструктор для EF.
    /// </summary>
    /// <param name="number">Номер.</param>
    /// <inheritdoc />
    /// <param name="domicile">Место жительства.</param>
    private Referee(int number, string lastName, string firstName, string domicile,
        string? patronymic = null, string? description = null) 
        : base(lastName, firstName, patronymic, description)
    {
        Number = number;
        Domicile = domicile;
    }
    
    /// <summary>
    /// Конструктор на основе готового экземпляра.
    /// </summary>
    private Referee(Referee referee)
        : this(
            referee.Number,
            referee.LastName,
            referee.FirstName,
            referee.Domicile,
            referee.RefereeLevel,
            referee.RefereeJobTitle,
            referee.CompetitionData,
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
    /// <param name="competitionData">Соревнование.</param>
    public Referee(int number, string lastName, string firstName, string domicile, 
        RefereeLevel refereeLevel, RefereeJobTitle refereeJobTitle, 
        CompetitionData competitionData,
        string? patronymic = null, string? description = null) 
        : this(number, lastName, firstName, domicile, patronymic, description)
    {
        RefereeLevel = refereeLevel;
        RefereeJobTitle = refereeJobTitle;
        CompetitionData = competitionData;
    }
    
    /// <inheritdoc />
    public int Number { get; set; }
    
    /// <summary>
    /// Место жительства (территория).
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
    public RefereeJobTitleEnm RefereeJobTitleId { get; set; }

    /// <inheritdoc cref="RefereeJobTitleId"/>
    public RefereeJobTitle RefereeJobTitle { get; set; } = null!;
        
    /// <summary>
    /// Связь с соревнованием (объектом-владельцем).
    /// </summary>
    public int CompetitionDataId { get; set; }

    /// <inheritdoc cref="CompetitionDataId"/>
    public CompetitionData CompetitionData { get; set; } = null!;

    /// <summary>
    /// Клонирование.
    /// </summary>
    public Referee Clone()
        => new(this);
    
    /// <inheritdoc />
    object ICloneable.Clone() {
        return Clone();
    }
    
    /// <inheritdoc cref="ICopyEntity.Copy"/>
    public void Copy(Referee referee)
    {
        referee.Number = Number;
        referee.LastName = LastName;
        referee.FirstName = FirstName;
        referee.Patronymic = Patronymic;
        referee.Description = Description;
        referee.Domicile = Domicile;
        referee.RefereeLevel = RefereeLevel;
        referee.RefereeJobTitle = RefereeJobTitle;
    }
    
    /// <inheritdoc />
    void ICopyEntity.Copy(IAbstractEntity destination)
    {
        Copy((Referee)destination);
    }

    /// <inheritdoc />
    public override string ToString()
        => Name;
}