using System.ComponentModel.DataAnnotations;
using Presentation.ViewModels.Shared.Models;

namespace Presentation.ViewModels.Shared.Validation;

/// <summary>
/// Валидация судейской категории.
/// </summary>
public class RefereeCategoryAvailabilityAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var ro = (RefereeObservable)validationContext.ObjectInstance;

        if (value is not RefereeCategoryObservable rco || 
            ro.AvailableCategories.FirstOrDefault(item => item.Equals(rco)) is not { IsAvailable: true })
        {
            return new ValidationResult("Ошибка: недопустимое значение категории.");
        }

        return ValidationResult.Success;
    }
}