using System;
using System.Collections.Generic;
using ProblemDomain.Entities._Contracts;
using ProblemDomain.Entities.LibraryEntities.Enums;
// ReSharper disable InvalidXmlDocComment

namespace ProblemDomain.Entities.LibraryEntities;

/// <summary>
/// Подгруппа дисциплин.
/// </summary>
public sealed class DisciplineSubGroup
    : AbstractEntity, ICloneable, ICopy
{
    /// <summary>
    /// Конструктор для EF.
    /// </summary>
    /// <param name="id">Идентификатор.</param>
    /// <inheritdoc />
    private DisciplineSubGroup(DisciplineSubGroupEnm id, string name, string? description = null)
        : base(name, description)
    {
        Id = id;
    }

    /// <summary>
    /// Конструктор на основе готового экземпляра.
    /// </summary>
    private DisciplineSubGroup(DisciplineSubGroup competitionsStatus)
        : this(
            competitionsStatus.Id,
            competitionsStatus.Name,
            competitionsStatus.DisciplineGroup,
            competitionsStatus.Description
        )
    {
    }

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <inheritdoc />
    /// <param name="disciplineGroup">Группа дисциплин.</param>
    public DisciplineSubGroup(DisciplineSubGroupEnm id, string name, DisciplineGroup disciplineGroup,
        string? description = null)
        : this(id, name, description)
    {
        DisciplineGroup = disciplineGroup;
    }

    /// <inheritdoc cref="AbstractEntity.Id"/>
    public new DisciplineSubGroupEnm Id { get; set; }

    /// <summary>
    /// Связь с группой дисциплин (объектом-владельцем).
    /// </summary>
    public DisciplineGroupEnm DisciplineGroupId { get; set; }

    /// <inheritdoc cref="DisciplineGroupId"/>
    public DisciplineGroup DisciplineGroup { get; set; } = null!;

    /// <summary>
    /// Коллекция дисциплин.
    /// </summary>
    // ReSharper disable once CollectionNeverUpdated.Global
    public ICollection<Discipline> Disciplines { get; set; } = new HashSet<Discipline>();

    /// <summary>
    /// Клонирование.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public DisciplineSubGroup Clone()
        => new(this);

    /// <inheritdoc />
    object ICloneable.Clone()
    {
        return Clone();
    }

    /// <inheritdoc cref="ICopy.Copy"/>
    // ReSharper disable once MemberCanBePrivate.Global
    public void Copy(DisciplineSubGroup destination)
    {
        destination.Id = Id;
        destination.Name = Name;
        destination.DisciplineGroup = DisciplineGroup;
        destination.Description = Description;
    }

    /// <inheritdoc />
    void ICopy.Copy(IAbstractEntity destination)
    {
        Copy((DisciplineSubGroup)destination);
    }
}