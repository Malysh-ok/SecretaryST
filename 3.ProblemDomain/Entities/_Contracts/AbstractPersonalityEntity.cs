using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ProblemDomain.Entities._Contracts;

/// <summary>
/// Абстрактная сущность личности (человека).
/// </summary>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public abstract class AbstractPersonalityEntity 
    : IAbstractEntity, IEntityWithDescription, IPersonalityEntity
{
    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="lastName">Фамилия.</param>
    /// <param name="firstName">Имя.</param>
    /// <param name="patronymic">Отчество.</param>
    /// <param name="description">Описание.</param>
    protected AbstractPersonalityEntity(string lastName, string firstName, 
        string? patronymic = null, string? description = null)
    {
        FirstName = firstName;
        LastName = lastName;
        Patronymic = patronymic;
        Description = description;
    }

    /// <inheritdoc />
    public int Id { get; set; }
    
    /// <inheritdoc />
    public string? Description { get; set; }

    /// <inheritdoc />
    public string FirstName { get; set; }
    
    /// <inheritdoc />
    public string LastName { get; set; }
    
    /// <inheritdoc />
    public string? Patronymic { get; set; }

    /// <summary>
    /// Полное имя.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Короткое имя (фамилия, инициалы).
    /// </summary>
    // ReSharper disable once MemberCanBeProtected.Global
    public string ShortName
    {
        get
        {
            var arrName = Name.Split(" ");
            return arrName.Length switch
            {
                1 => arrName[0],
                2 => $"{arrName[0]} " +
                     $"{(arrName[1].Length > 0 ? ' ' + arrName[1][..1] + '.' : string.Empty)}",
                3 => $"{arrName[0]}" +
                          $"{(arrName[1].Length > 0 ? ' ' + arrName[1][..1] + '.' : string.Empty)}" +
                          $"{(arrName[2].Length > 0 ? ' ' + arrName[2][..1] + '.' : string.Empty)}",
                _ => string.Empty
            };
        }
    }
}