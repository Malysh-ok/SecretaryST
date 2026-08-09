using System.ComponentModel.DataAnnotations;
using Presentation.ViewModels.Shared.Models;
using ProblemDomain.Entities.LibraryEntities;

namespace Presentation.ViewModels.Shared.Validation;

public class DifficultyAvailabilityAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var vm = (SportEventObservable)validationContext.ObjectInstance;
        if (value == null || ! vm.AvailableDifficulties.Contains((Difficulty)value))
        {
            return new ValidationResult("Ошибка: недопустимое значение трудности.");
        }

        return ValidationResult.Success;
    }
}