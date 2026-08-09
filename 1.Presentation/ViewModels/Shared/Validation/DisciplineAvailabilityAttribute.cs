using System.ComponentModel.DataAnnotations;
using Presentation.ViewModels.Shared.Models;
using ProblemDomain.Entities.LibraryEntities;

namespace Presentation.ViewModels.Shared.Validation;

public class DisciplineAvailabilityAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var vm = (SportEventObservable)validationContext.ObjectInstance;
        if (value == null || ! vm.AvailableDisciplines.Contains((Discipline)value))
        {
            return new ValidationResult("Ошибка: недопустимое значение дисциплины.");
        }

        return ValidationResult.Success;
    }
}