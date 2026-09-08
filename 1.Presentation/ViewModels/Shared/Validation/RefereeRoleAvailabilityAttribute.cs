using System.ComponentModel.DataAnnotations;
using Presentation.ViewModels.Shared.Models;

namespace Presentation.ViewModels.Shared.Validation;

/// <summary>
/// Валидация судейской должности.
/// </summary>
public class RefereeRoleAvailabilityAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var vm = (RefereeObservable)validationContext.ObjectInstance;
        // var isRoleValid = vm.IsRoleValidAsync().GetAwaiter().GetResult();
        var isRoleValid = Task.Run(async () => await vm.GetIsRoleValidAsync()).GetAwaiter().GetResult();
        if (value == null || ! isRoleValid)
        {
            return new ValidationResult("Ошибка: недопустимое значение должности.");
        }

        return ValidationResult.Success;
    }
}