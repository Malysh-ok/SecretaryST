using System;
using System.Collections.Generic;
using Common.BaseExtensions.ValueTypes;
using ProblemDomain.Entities._Contracts;
using ProblemDomain.Entities.DistanceEntities;
using ProblemDomain.Entities.LibraryEntities.Enums;
// ReSharper disable InvalidXmlDocComment
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace ProblemDomain.Entities.LibraryEntities;

/// <summary>
/// Трудность - категория сложности маршрута или класс дистанции.
/// </summary>
public sealed class Difficulty : AbstractEntity<DifficultyEnm>, IEntityCopyable, IEquatable<Difficulty>
{
    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="id">Первичный идентификатор.</param>
    /// <param name="disciplineGroupId">Вторичный идентификатор (идентификатор группы дисциплин).</param>
    /// <inheritdoc />
    /// <param name="fullName">Полное наименование.</param>
    /// <param name="fullNameGenitive">Полное наименование в родительном падеже.</param>
    public Difficulty(
        DifficultyEnm id, 
        DisciplineGroupEnm disciplineGroupId, 
        string name, 
        string fullName, 
        string fullNameGenitive, 
        string? description = null) : base(name, description)
    {
        Id = id;
        DisciplineGroupId = disciplineGroupId;
        FullName = fullName;
        FullNameGenitive = fullNameGenitive;
    }

    /// <inheritdoc cref="DisciplineGroup"/>
    /// <remarks>
    /// Вторичный идентификатор.
    /// </remarks>
    public DisciplineGroupEnm DisciplineGroupId { get; init; }

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

    /// <inheritdoc cref="IEntityCopyable.Copy"/>
    // ReSharper disable once MemberCanBePrivate.Global
    public void Copy(Difficulty destination)
    {
        destination.Name = Name;
        destination.FullName = FullName;
        destination.FullNameGenitive = FullNameGenitive;
        destination.Description = Description;
    }
    
    /// <inheritdoc />
    void IEntityCopyable.Copy(IAbstractEntity destination)
    {
        Copy((Difficulty)destination);
    }
    
    /// <summary>
    /// Определяет, равен ли текущий объект другому объекту того же типа.
    /// </summary>
    /// <remarks>
    /// Отличается от стандартного, т.к. здесь составной ключ.
    /// </remarks>
    public bool Equals(Difficulty? other)
    {
        if (other is null || GetType() != other.GetType()) return false;

        return ReferenceEquals(this, other) 
               || (Id == other.Id && DisciplineGroupId == other.DisciplineGroupId);     // сравнение составного ключа
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => Equals(obj as Difficulty);
    
    /// <summary>
    /// Сравнение на равенство.
    /// </summary>
    public static bool operator ==(Difficulty? left, Difficulty? right)
    {
        return left?.Equals(right) ?? right is null;
    }

    /// <summary>
    /// Сравнение на неравенство.
    /// </summary>
    public static bool operator !=(Difficulty? left, Difficulty? right)
    {
        return ! (left == right);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        // Комбинируем хеш-коды обоих полей ключа
        return HashCode.Combine(Id, DisciplineGroupId);
    }

    /// <inheritdoc />
    public override string ToString()
        => Name;
}