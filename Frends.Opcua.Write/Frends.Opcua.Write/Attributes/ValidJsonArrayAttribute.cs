using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Linq;

namespace Frends.Opcua.Write.Attributes;

/// <summary>
/// Validates that a string property is a valid JSON array where each element
/// contains the required fields.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
internal class ValidJsonArrayAttribute(params string[] requiredFields) : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        // Skip if null/empty — let [Required] or [RequiredIf] handle that
        if (value is not string str || string.IsNullOrWhiteSpace(str))
            return ValidationResult.Success;

        JArray parsed;
        try
        {
            parsed = JArray.Parse(str);
        }
        catch (Exception ex)
        {
            return new ValidationResult($"{validationContext.DisplayName} is not valid JSON: {ex.Message}");
        }

        if (parsed.Count == 0)
            return new ValidationResult($"{validationContext.DisplayName} must contain at least one entry.");

        foreach (var token in parsed)
        {
            foreach (var field in requiredFields)
            {
                if (token[field] == null)
                    return new ValidationResult($"{validationContext.DisplayName}: each entry must have a '{field}' field. Offending entry: {token}");
            }
        }

        return ValidationResult.Success;
    }
}