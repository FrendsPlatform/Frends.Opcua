using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;

namespace Frends.Opcua.Read.Attributes;

/// <summary>
/// Validates that a property is required if another property has a specific value.
/// If a property is null, empty, or white space only, validation fails.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
internal class RequiredToExistIfAttribute(string dependentProperty, bool file, params object[] targetValues) : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var field = validationContext.ObjectType.GetProperty(dependentProperty);

        if (field == null)
            return new ValidationResult($"Unknown property: {dependentProperty}");

        var dependentValue = field.GetValue(validationContext.ObjectInstance);

        if (!targetValues.Contains(dependentValue)) return ValidationResult.Success;

        if (value == null || (value is string s && string.IsNullOrWhiteSpace(s)))
            return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} is required.");

        if (file && !File.Exists(value as string))
            return new ValidationResult(ErrorMessage ?? $"Certificate inside parameter {validationContext.DisplayName} needs to exists.");

        return ValidationResult.Success;
    }
}
