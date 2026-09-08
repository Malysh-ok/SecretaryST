using System;
using ProblemDomain.Entities._Contracts;
using ProblemDomain.Entities.LibraryEntities;
using ProblemDomain.Entities.LibraryEntities.Enums;
// ReSharper disable InvalidXmlDocComment
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace ProblemDomain.Entities.CommonEntities;

/// <summary>
/// Судья.
/// </summary>
public sealed class Referee : AbstractPersonalityEntity, INumberedEntity, IEntityCloneable, IEntityCopyable
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
            referee.Patronymic,
            referee.Description
        )
    {
        CategoryId = referee.CategoryId;
        RoleId = referee.RoleId;
        CompetitionId = referee.CompetitionId;
    }
    
    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <inheritdoc />
    /// <param name="category">Судейская категория.</param>
    /// <param name="role">Судейская должность.</param>
    /// <param name="competition">Соревнование.</param>
    public Referee(
        int number, 
        string lastName, 
        string firstName, 
        string domicile, 
        RefereeCategory category, 
        RefereeRole role, 
        Competition competition,
        string? patronymic = null, 
        string? description = null) : this(number, lastName, firstName, domicile, patronymic, description)
    {
        Category = category;
        Role = role;
        Competition = competition;
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
    public RefereeCategoryEnm CategoryId { get; set; }

    /// <inheritdoc cref="CategoryId"/>
    public RefereeCategory Category { get; set; } = null!;
    
    /// <summary>
    /// Дата присвоения/подтверждения категории.
    /// </summary>
    public DateTime? CategoryGrantedDate { get; set; }
    
    /// <summary>
    /// Связь с судейской должностью (объектом-владельцем).
    /// </summary>
    public RefereeRoleEnm RoleId { get; set; }

    /// <inheritdoc cref="RoleId"/>
    public RefereeRole Role { get; set; } = null!;

    /// <summary>
    /// Связь с соревнованием (объектом-владельцем).
    /// </summary>
    public int CompetitionId { get; set; }

    /// <inheritdoc cref="CompetitionId"/>
    public Competition Competition { get; set; } = null!;
    
    /// <summary>
    /// Клонирование.
    /// </summary>
    public Referee Clone()
        => new(this);
    
    /// <inheritdoc />
    object IEntityCloneable.Clone() {
        return Clone();
    }
    
    /// <inheritdoc cref="IEntityCopyable.Copy"/>
    public void Copy(Referee referee)
    {
        referee.Number = Number;
        referee.LastName = LastName;
        referee.FirstName = FirstName;
        referee.Domicile = Domicile;
        referee.Patronymic = Patronymic;
        referee.Description = Description;
    }
    
    /// <inheritdoc />
    void IEntityCopyable.Copy(IAbstractEntity destination)
    {
        Copy((Referee)destination);
    }

    /// <inheritdoc />
    public override string ToString()
        => Name;
}