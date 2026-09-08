using CommunityToolkit.Mvvm.ComponentModel;
using ProblemDomain.Entities.LibraryEntities;
using ProblemDomain.Entities.LibraryEntities.Enums;

namespace Presentation.ViewModels.Shared.Models;

public class RefereeCategoryObservable : ObservableObject, IEquatable<RefereeCategoryObservable>
{
    /// <summary>
    /// Конструктор.
    /// </summary>
    public RefereeCategoryObservable(RefereeCategory category)
    {
        Category = category;
    }
    
    /// <summary>
    /// Текущая категория (основное свойство).
    /// </summary>
    public RefereeCategory Category
    {
        get;
        private init => SetProperty(ref field, value);
    }
    
    /// <summary>
    /// Идентификатор.
    /// </summary>
    public RefereeCategoryEnm Id => Category.Id;

    /// <summary>
    /// Наименование.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public string Name => Category.Name;
    
    /// <summary>
    /// Признак доступности категории.
    /// </summary>
    public bool IsAvailable
    {
        get;
        set => SetProperty(ref field, value);
    }
    
    /// <inheritdoc />
    public bool Equals(RefereeCategoryObservable? other)
    {
        if (other is null || GetType() != other.GetType()) return false;

        return Category.Id == other.Category.Id;
    }
    
    /// <inheritdoc />
    public override bool Equals(object? obj)
        => Equals(obj as RefereeCategoryObservable);
    
    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(Category.Id);
    }
    
    /// <inheritdoc />
    public override string ToString()
        => Name;
}