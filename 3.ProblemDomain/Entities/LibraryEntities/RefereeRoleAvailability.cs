using ProblemDomain.Entities._Contracts;
using ProblemDomain.Entities.LibraryEntities.Enums;
// ReSharper disable InvalidXmlDocComment

namespace ProblemDomain.Entities.LibraryEntities;

/// <summary>
/// Доступность судейских должностей в зависимости от категории судьи и статуса соревнования.
/// </summary>
public class RefereeRoleAvailability : AbstractEntity<RefereeRoleEnm>, IEntityCopyable
{
    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="id">Первичный идентификатор (идентификатор судейской должности).</param>
    /// <param name="detailedCompetitionStatusId">Вторичный идентификатор (идентификатор детального статуса соревнования).</param>
    /// <param name="refereeCategoryId">Идентификатор категории судьи.</param>
    /// <param name="maxRefereesPerRole">Максимальное количество судей на данную должность.</param>
    /// <inheritdoc />
    public RefereeRoleAvailability(
        RefereeRoleEnm id, 
        DetailedCompetitionStatusEnm detailedCompetitionStatusId,
        RefereeCategoryEnm refereeCategoryId,
        int maxRefereesPerRole = 1,
        string? name = null!,
        string? description = null) : base(name ?? string.Empty, description)
    {
        Id = id;
        DetailedCompetitionStatusId = detailedCompetitionStatusId;
        RefereeCategoryId = refereeCategoryId;
        MaxRefereesPerRole = maxRefereesPerRole;
    }

    /// <summary>
    /// Идентификатор статуса соревнования (часть составного ключа).
    /// </summary>
    public DetailedCompetitionStatusEnm DetailedCompetitionStatusId { get; private set; }

    /// <summary>
    /// Идентификатор категории судьи.
    /// </summary>
    public RefereeCategoryEnm RefereeCategoryId { get; private set; }

    /// <summary>
    /// Максимальное количество судей на данную должность.
    /// </summary>
    public int MaxRefereesPerRole { get; private set; }
    
    /// <inheritdoc cref="IEntityCopyable.Copy"/>
    // ReSharper disable once MemberCanBePrivate.Global
    public void Copy(RefereeRoleAvailability destination)
    {
        destination.Name = Name;
        destination.Description = Description;
        destination.RefereeCategoryId = RefereeCategoryId;
        destination.MaxRefereesPerRole = MaxRefereesPerRole;
    }

    /// <inheritdoc />
    void IEntityCopyable.Copy(IAbstractEntity destination)
    {
        Copy((RefereeRoleAvailability)destination);
    }

    /// <inheritdoc />
    public override string ToString()
        => $"{Id}-{DetailedCompetitionStatusId}";
}