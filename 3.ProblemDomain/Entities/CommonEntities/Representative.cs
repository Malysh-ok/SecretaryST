using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ProblemDomain.Entities._Contracts;
using ProblemDomain.Entities.LibraryEntities;
using ProblemDomain.Entities.LibraryEntities.Enums;

namespace ProblemDomain.Entities.CommonEntities;

/// <summary>
/// Представитель.
/// </summary>
public sealed class Representative
    : AbstractPersonalityEntity, IEquatable<Representative>, ICloneable, ICopy
{
    /// <summary>
    /// Конструктор для EF.
    /// </summary>
    /// <inheritdoc />
    /// <param name="phoneNumber">Номер телефона.</param>
    /// <param name="email">E-mail.</param>
    [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
    private Representative(string lastName, string firstName, string? patronymic = null, string? description = null,
        string? phoneNumber = null, string? email = null) 
        : base(lastName, firstName, patronymic, description)
    {
        PhoneNumber = phoneNumber;
        Email = email;
    }
    
    /// <summary>
    /// Конструктор на основе готового экземпляра.
    /// </summary>
    private Representative(Representative representative)
        : this(
            representative.LastName,
            representative.FirstName,
            representative.Patronymic,
            representative.Description,
            representative.PhoneNumber,
            representative.Email,
            representative.Sex
        )
    {
    }
    
    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <inheritdoc />
    /// <param name="phoneNumber">Номер телефона.</param>
    /// <param name="email">E-mail.</param>
    [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
    public Representative(string lastName, string firstName, string? patronymic = null, string? description = null,
        string? phoneNumber = null, string? email = null, Sex? sex = null) 
        : this(lastName, firstName, patronymic, description, phoneNumber, email)
    {
        Sex = sex;
    }

    /// <summary>
    /// Номер телефона.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// E-mail.
    /// </summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// Связь с полом (объектом-владельцем).
    /// </summary>
    public SexEnm? SexId { get; set; }
    /// <inheritdoc cref="SexId"/>
    public Sex? Sex { get; set; }

    
    /// <summary>
    /// Коллекция делегаций.
    /// </summary>
    // ReSharper disable once CollectionNeverUpdated.Global
    public ICollection<Delegation> Delegations { get; set; } = new HashSet<Delegation>();

    // TODO: Подумать, насчет правильного сравнения Representative
    
    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType()) return false;

        return Equals((Representative)obj);
    }

    /// <inheritdoc />
    public bool Equals(Representative? other)
        => string.Equals(FirstName, other?.FirstName, StringComparison.OrdinalIgnoreCase)
           && string.Equals(LastName, other?.LastName, StringComparison.OrdinalIgnoreCase)
           && string.Equals(Patronymic, other?.Patronymic, StringComparison.OrdinalIgnoreCase);
    
    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(FirstName, ShortName, Patronymic);
    }

    /// <summary>
    /// Клонирование.
    /// </summary>
    public Representative Clone()
        => new(this);
    
    /// <inheritdoc />
    object ICloneable.Clone() {
        return Clone();
    }

    /// <inheritdoc cref="ICopy.Copy"/>
    public void Copy(Representative destination)
    {
        destination.LastName = LastName;
        destination.FirstName = FirstName;
        destination.Patronymic = Patronymic;
        destination.Description = Description;
        destination.PhoneNumber = PhoneNumber;
        destination.Email = Email;
        destination.Sex = Sex;
    }
    
    /// <inheritdoc />
    void ICopy.Copy(IAbstractEntity destination)
    {
        Copy((Representative)destination);
    }
}