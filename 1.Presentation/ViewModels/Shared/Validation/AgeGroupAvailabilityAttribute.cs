using System.ComponentModel.DataAnnotations;
using Presentation.ViewModels.Shared.Models;
using ProblemDomain.Entities.LibraryEntities;

namespace Presentation.ViewModels.Shared.Validation;

/// <summary>
/// Проверяет, что выбранная возрастная группа != null и присутствует в коллекции доступных возрастных групп.
/// </summary>
public class AgeGroupAvailabilityAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var vm = (SportEventObservable)validationContext.ObjectInstance;
        if (value == null || ! vm.AvailableAgeGroups.Contains((AgeGroup)value))
        {
            return new ValidationResult("Ошибка: недопустимое значение возрастной группы.");
        }

        return ValidationResult.Success;
    }
}