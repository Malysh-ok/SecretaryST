using System.Collections.Generic;
using ProblemDomain.Entities._Contracts;
// ReSharper disable InvalidXmlDocComment
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace ProblemDomain.Entities.CommonEntities;

/// <summary>
/// Делегация.
/// </summary>
public sealed class Delegation : AbstractEntity<int>, INumberedEntity, IEntityCloneable, IEntityCopyable
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
    private Delegation(Delegation source)
        : this(
            source.Number,
            source.Name,
            source.Region,
            source.Description
        )
    {
        RepresentativeId = source.RepresentativeId;
        CompetitionDataId = source.CompetitionDataId;
    }
    
    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <inheritdoc />
    /// <param name="representative">Представитель.</param>
    public Delegation(
        int number, 
        string name, 
        string region, 
        Representative representative, 
        CompetitionData competitionData, 
        string? description = null) : this(number, name, region, description)
    {
        Representative = representative;
        CompetitionData = competitionData;
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
    /// Связь с соревнованием (объектом-владельцем).
    /// </summary>
    public int CompetitionDataId { get; set; }

    /// <inheritdoc cref="CompetitionDataId"/>
    public CompetitionData CompetitionData { get; set; } = null!;

    /// <summary>
    /// Коллекция спортсменов.
    /// </summary>
    public ICollection<Athlete> Athletes { get; set; } = new HashSet<Athlete>();
    
    /// <summary>
    /// Клонирование.
    /// </summary>
    public Delegation Clone()
        => new(this);
    
    /// <inheritdoc />
    object IEntityCloneable.Clone() {
        return Clone();
    }
    
    /// <inheritdoc cref="IEntityCopyable.Copy"/>
    public void Copy(Delegation destination)
    {
        destination.Number = Number;
        destination.Name = Name;
        destination.Region = Region;
        destination.Description = Description;
    }

    /// <inheritdoc />
    void IEntityCopyable.Copy(IAbstractEntity destination)
    {
        Copy((Delegation)destination);
    }
}