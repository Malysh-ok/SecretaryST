using System;
using System.Collections.Generic;
using Common.BaseExtensions.ValueTypes;
using ProblemDomain.Entities._Contracts;
using ProblemDomain.Entities.DistanceEntities;
using ProblemDomain.Entities.LibraryEntities.Enums;
// ReSharper disable InvalidXmlDocComment

namespace ProblemDomain.Entities.LibraryEntities;

/// <summary>
/// Трудность - категория сложности маршрута или класс дистанции.
/// </summary>
public sealed class Difficulty
    : AbstractEntity, ICloneable, ICopyEntity
{
    /// <summary>
    /// Конструктор на основе готового экземпляра.
    /// </summary>
    private Difficulty(Difficulty difficulty)
        : this(
            difficulty.Id,
            difficulty.DisciplineGroupId,
            difficulty.Name,
            difficulty.FullName,
            difficulty.FullNameGenitive,
            difficulty.Description
        )
    {
    }

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="id">Первичный идентификатор.</param>
    /// <param name="disciplineGroupId">Вторичный идентификатор (идентификатор группы дисциплин).</param>
    /// <param name="fullName">Полное наименование.</param>
    /// <param name="fullNameGenitive">Полное наименование в родительном падеже.</param>
    /// <inheritdoc cref="AbstractEntity(string,string?)"/>
    public Difficulty(DifficultyEnm id, DisciplineGroupEnm disciplineGroupId, string name, 
        string fullName, string fullNameGenitive, string? description = null) 
        : base(name, description)
    {
        Id = id;
        DisciplineGroupId = disciplineGroupId;
        FullName = fullName;
        FullNameGenitive = fullNameGenitive;
    }
    
    /// <inheritdoc cref="AbstractEntity.Id"/>
    public new DifficultyEnm Id { get; set; }

    /// <inheritdoc cref="DisciplineGroup"/>
    /// <remarks>
    /// Вторичный идентификатор.
    /// </remarks>
    public DisciplineGroupEnm DisciplineGroupId { get; set; }

    /// <summary>
    /// Связь с группой дисциплин (объектом-владельцем).
    /// </summary>
    // ReSharper disable once PropertyCanBeMadeInitOnly.Global
    public DisciplineGroup DisciplineGroup { get; set; } = null!;
    
    /// <summary>
    /// Полное наименование.
    /// </summary>
    public string FullName { get; set; }

    /// <summary>
    /// Полное наименование в родительном падеже.
    /// </summary>
    public string FullNameGenitive { get; set; }

    /// <summary>
    /// Коллекция видов программы.
    /// </summary>
    public ICollection<SportEvent> SportEvents { get; set; } = new HashSet<SportEvent>();
    
    /// <summary>
    /// Численное значение трудности.
    /// </summary>
    /// <remarks>
    /// 1 - 6 к.с. для маршрутов, 1 - 6 класс дистанций и сев. ходьбы,
    /// -3 - -1 ст.с. для маршрутов.
    /// </remarks>
    public int GetDifficultyValue()
    {
        return Id.ToInt();
    }
    
    /// <summary>
    /// Клонирование.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public Difficulty Clone()
        => new(this);
    
    /// <inheritdoc />
    object ICloneable.Clone() {
        return Clone();
    }

    /// <inheritdoc cref="ICopyEntity.Copy"/>
    // ReSharper disable once MemberCanBePrivate.Global
    public void Copy(Difficulty destination)
    {
        destination.Id = Id;
        destination.DisciplineGroupId = DisciplineGroupId;
        destination.Name = Name;
        destination.FullName = FullName;
        destination.FullNameGenitive = FullNameGenitive;
        destination.Description = Description;
    }
    
    /// <inheritdoc />
    void ICopyEntity.Copy(IAbstractEntity destination)
    {
        Copy((Difficulty)destination);
    }
    
    /// <inheritdoc />
    public override string ToString()
        => Name;
}