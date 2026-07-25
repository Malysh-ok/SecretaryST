using System.Collections.Generic;
using ProblemDomain.Entities._Contracts;
using ProblemDomain.Entities.LibraryEntities;
using ProblemDomain.Entities.LibraryEntities.Enums;
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable InvalidXmlDocComment

namespace ProblemDomain.Entities.CommonEntities;

/// <summary>
/// Представитель.
/// </summary>
public sealed class Representative : AbstractPersonalityEntity, IEntityCloneable, IEntityCopyable
{
    /// <summary>
    /// Конструктор для EF.
    /// </summary>
    /// <inheritdoc />
    /// <param name="phoneNumber">Номер телефона.</param>
    /// <param name="email">E-mail.</param>
    private Representative(string lastName, string firstName, string? patronymic = null,
        string? phoneNumber = null, string? email = null, string? description = null) 
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
            representative.PhoneNumber,
            representative.Email,
            representative.Description
        )
    {
        SexId = representative.SexId;
    }
    
    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <inheritdoc />
    /// <param name="phoneNumber">Номер телефона.</param>
    /// <param name="email">E-mail.</param>
    public Representative(
        string lastName, 
        string firstName, 
        string? patronymic = null, 
        string? phoneNumber = null, 
        string? email = null, 
        Sex? sex = null,
        string? description = null) : this(lastName, firstName, patronymic, phoneNumber, email, description)
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
    /// <remarks>
    /// REMARK: С точки зрения Правил, одной делегации должен соответствовать один представитель.
    /// </remarks>
    public ICollection<Delegation> Delegations { get; set; } = new HashSet<Delegation>();

    /// <summary>
    /// Проверяет, является ли другой представитель тем же человеком.
    /// </summary>
    public bool IsSamePerson(Representative? other)
    {
        if (other is null) return false;
        
        return LastName == other.LastName &&
               FirstName == other.FirstName &&
               Patronymic == other.Patronymic &&
               PhoneNumber == other.PhoneNumber &&
               Email == other.Email &&
               SexId == other.SexId;
    }
    
    /// <summary>
    /// Клонирование.
    /// </summary>
    public Representative Clone()
        => new(this);
    
    /// <inheritdoc />
    object IEntityCloneable.Clone() {
        return Clone();
    }

    /// <inheritdoc cref="IEntityCopyable.Copy"/>
    public void Copy(Representative destination)
    {
        destination.LastName = LastName;
        destination.FirstName = FirstName;
        destination.Patronymic = Patronymic;
        destination.Description = Description;
        destination.PhoneNumber = PhoneNumber;
        destination.Email = Email;
    }
    
    /// <inheritdoc />
    void IEntityCopyable.Copy(IAbstractEntity destination)
    {
        Copy((Representative)destination);
    }
}