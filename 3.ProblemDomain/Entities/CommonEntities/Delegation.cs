using System;
using System.Collections.Generic;
using ProblemDomain.Entities._Contracts;
// ReSharper disable InvalidXmlDocComment
// ReSharper disable MemberCanBePrivate.Global

namespace ProblemDomain.Entities.CommonEntities;

/// <summary>
/// Делегация.
/// </summary>
public sealed class Delegation 
    : AbstractEntity, IEntityWithNumber, ICloneable, ICopy
{
    /// <summary>
    /// Конструктор для EF.
    /// </summary>
    /// <inheritdoc />
    /// <param name="number">Номер.</param>
    /// <param name="region">Территория.</param>
    private Delegation(int number, string name, string region, string? description = null) 
        : base(name, description)
    {
        Number = number;
        Region = region;
    }
    
    /// <summary>
    /// Конструктор на основе готового экземпляра.
    /// </summary>
    private Delegation(Delegation delegation)
        : this(
            delegation.Number,
            delegation.Name,
            delegation.Region,
            delegation.Representative,
            delegation.Description
        )
    {
    }
    
    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <inheritdoc />
    /// <param name="representative">Представитель.</param>
    public Delegation(int number, string name, string region, Representative representative, string? description = null) 
        : this(number, name, region, description)
    {
        Representative = representative;
    }

    /// <inheritdoc />
    public int Number { get; set; }
    
    /// <summary>
    /// Территория.
    /// </summary>
    public string Region { get; set; }
    
    /// <summary>
    /// Связь с представителем (объектом-владельцем).
    /// </summary>
    public int RepresentativeId { get; set; }

    /// <inheritdoc cref="RepresentativeId"/>
    public Representative Representative { get; set; } = null!;
    
    /// <summary>
    /// Список спортсменов.
    /// </summary>
    // ReSharper disable once CollectionNeverUpdated.Global
    public ICollection<Athlete> Athletes { get; set; } = new HashSet<Athlete>();
    
    /// <summary>
    /// Клонирование.
    /// </summary>
    public Delegation Clone()
        => new(this);
    
    /// <inheritdoc />
    object ICloneable.Clone() {
        return Clone();
    }
    
    /// <inheritdoc cref="ICopy.Copy"/>
    public void Copy(Delegation destination)
    {
        destination.Number = Number;
        destination.Name = Name;
        destination.Description = Description;
        destination.Region = Region;
        destination.Representative = Representative;
    }

    /// <inheritdoc />
    void ICopy.Copy(IAbstractEntity destination)
    {
        Copy((Delegation)destination);
    }
}