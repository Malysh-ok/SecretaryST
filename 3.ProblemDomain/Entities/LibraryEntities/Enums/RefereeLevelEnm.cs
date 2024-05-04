using System.Diagnostics.CodeAnalysis;

namespace ProblemDomain.Entities.LibraryEntities.Enums;

/// <summary>
/// Судейские категории.
/// </summary>
[SuppressMessage("ReSharper", "CommentTypo")]
public enum RefereeLevelEnm
{
    /// <summary>
    /// ЮСС.
    /// </summary>
    YoungCategory = 1,
    
    /// <summary>
    /// СС3К.
    /// </summary>
    Category3,
    
    /// <summary>
    /// СС2К.
    /// </summary>
    Category2,
    
    /// <summary>
    /// СС1К.
    /// </summary>
    Category1,
    
    /// <summary>
    /// ССВК.
    /// </summary>
    AllRussCategory
}