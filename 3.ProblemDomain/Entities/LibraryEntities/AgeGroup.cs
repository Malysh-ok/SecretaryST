using System;
using System.Collections.Generic;
using ProblemDomain.Entities._Contracts;
using ProblemDomain.Entities.DistanceEntities;
using ProblemDomain.Entities.LibraryEntities.Enums;
// ReSharper disable InvalidXmlDocComment
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace ProblemDomain.Entities.LibraryEntities;

/// <summary>
/// Возрастная группа.
/// </summary>
public sealed class AgeGroup : AbstractEntity<AgeGroupEnm>, IEntityCopyable, IEquatable<AgeGroup>
{
    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="id">Первичный идентификатор.</param>
    /// <param name="disciplineId">Вторичный идентификатор (идентификатор группы дисциплин).</param>
    /// <param name="minAge">Минимальный возраст.</param>
    /// <param name="maxAge">Максимальный возраст.</param>
    /// <param name="minStudentAge">Минимальный возраст на соревнованиях студентов.</param>
    /// <param name="maxStudentAge">Максимальный возраст на соревнованиях студентов.</param>
    /// <inheritdoc />
    public AgeGroup(
        AgeGroupEnm id, 
        DisciplineSubGroupEnm disciplineSubGroupId, 
        string name,
        int minAge,
        int? maxAge,
        int? minStudentAge = null,
        int? maxStudentAge = null,
        bool isStudentCompetition = false,
        string? description = null) : base(name, description)
    {
        Id = id;
        DisciplineSubGroupId = disciplineSubGroupId;
        MinAge = minAge;
        MaxAge = maxAge;
        MinStudentAge = minStudentAge;
        MaxStudentAge = maxStudentAge;
        IsStudentCompetition = isStudentCompetition;
    }

    /// <inheritdoc cref="Discipline"/>
    /// <remarks>
    /// Вторичный идентификатор.
    /// </remarks>
    public DisciplineSubGroupEnm DisciplineSubGroupId { get; init; }
    
    /// <summary>
    /// Связь с подгруппой дисциплин (объектом-владельцем).
    /// </summary>
    // ReSharper disable once PropertyCanBeMadeInitOnly.Global
    public DisciplineSubGroup DisciplineSubGroup { get; set; } = null!;

    /// <summary>
    /// Минимальный возраст.
    /// </summary>
    public int MinAge { get; set; }
    
    /// <summary>
    /// Максимальный возраст.
    /// </summary>
    public int? MaxAge { get; set; }

    /// <summary>
    /// Минимальный возраст на соревнованиях студентов.
    /// </summary>
    public int? MinStudentAge { get; set; }
    
    /// <summary>
    /// Максимальный возраст на соревнованиях студентов.
    /// </summary>
    public int? MaxStudentAge { get; set; }

    /// <summary>
    /// Признак того, что соревнования студенческие.
    /// </summary>
    public bool IsStudentCompetition { get; set; }
    
    /// <summary>
    /// Полное наименование (включая границы возраста).
    /// </summary>
    public string FullName
    {
        get
        {
            if (IsStudentCompetition)
            {
                // соревнования студентов
                return MinStudentAge == null || MaxStudentAge == null 
                    ? Name    
                    : $"{Name} ({MinStudentAge}–{MaxStudentAge} {GetYearsWord(MaxStudentAge.Value)})";
            }
            
            return MaxAge == null
                ? $"{Name} ({MinAge} {GetYearsWord(MinAge)} и старше)"
                : $"{Name} ({MinAge}–{MaxAge} {GetYearsWord(MaxAge.Value)})";
        }
    }

    /// <summary>
    /// Коллекция видов программы.
    /// </summary>
    public ICollection<SportEvent> SportEvents { get; set; } = new HashSet<SportEvent>();

    /// <summary>
    /// Возвращает правильное склонение слова "год" в зависимости от числа.
    /// </summary>
    /// <param name="years">Количество лет.</param>
    /// <returns>Одно из: "год", "года", "лет".</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public static string GetYearsWord(int years)
    {
        // Берем последние две цифры для проверки исключений
        var lastTwoDigits = Math.Abs(years % 100);
        // Исключение: 11, 12, 13, 14
        if (lastTwoDigits is >= 11 and <= 14)
            return "лет";

        // Берем последнюю цифру для остальных случаев
        var lastDigit = Math.Abs(years % 10);
        return lastDigit switch
        {
            1           => "год",
            2 or 3 or 4 => "года",
            _           => "лет"
        };
    }

    /// <inheritdoc cref="IEntityCopyable.Copy"/>
    // ReSharper disable once MemberCanBePrivate.Global
    public void Copy(AgeGroup destination)
    {
        destination.Name = Name;
        destination.MinAge = MinAge;
        destination.MaxAge = MaxAge;
        destination.Description = Description;
    }
    
    /// <inheritdoc />
    void IEntityCopyable.Copy(IAbstractEntity destination)
    {
        Copy((AgeGroup)destination);
    }
    
    /// <summary>
    /// Определяет, равен ли текущий объект другому объекту того же типа.
    /// </summary>
    /// <remarks>
    /// Отличается от стандартного, т.к. здесь составной ключ.
    /// </remarks>
    public bool Equals(AgeGroup? other)
    {
        if (other is null || GetType() != other.GetType()) return false;

        return ReferenceEquals(this, other) 
               || (Id == other.Id && DisciplineSubGroupId == other.DisciplineSubGroupId);     // сравнение составного ключа
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => Equals(obj as AgeGroup);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        // Комбинируем хеш-коды обоих полей ключа
        return HashCode.Combine(Id, DisciplineSubGroupId);
    }

    /// <inheritdoc />
    public override string ToString()
        => FullName;
}