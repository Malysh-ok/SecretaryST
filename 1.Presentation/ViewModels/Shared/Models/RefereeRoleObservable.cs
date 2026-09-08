using CommunityToolkit.Mvvm.ComponentModel;
using ProblemDomain.Entities.LibraryEntities;
using ProblemDomain.Entities.LibraryEntities.Enums;

namespace Presentation.ViewModels.Shared.Models;

public class RefereeRoleObservable : ObservableObject, IEquatable<RefereeRoleObservable>
{
    /// <summary>
    /// Конструктор.
    /// </summary>
    public RefereeRoleObservable(RefereeRole role)
    {
        Role = role;
    }
    
    /// <summary>
    /// Текущая должность (основное свойство).
    /// </summary>
    public RefereeRole Role
    {
        get;
        private init => SetProperty(ref field, value);
    }
    
    /// <summary>
    /// Идентификатор.
    /// </summary>
    public RefereeRoleEnm Id => Role.Id;

    /// <summary>
    /// Наименование.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public string Name => Role.Name;
    
    /// <summary>
    /// Признак доступности должности.
    /// </summary>
    public bool IsAvailable
    {
        get;
        set => SetProperty(ref field, value);
    }
    
    /// <inheritdoc />
    public bool Equals(RefereeRoleObservable? other)
    {
        if (other is null || GetType() != other.GetType()) return false;

        return Role.Id == other.Role.Id;
    }
    
    /// <inheritdoc />
    public override bool Equals(object? obj)
        => Equals(obj as RefereeRoleObservable);
    
    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(Role.Id);
    }
    
    /// <inheritdoc />
    public override string ToString()
        => Name;
}