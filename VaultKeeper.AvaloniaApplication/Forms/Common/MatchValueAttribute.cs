using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace VaultKeeper.AvaloniaApplication.Forms.Common;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public class MatchValueAttribute(string otherPropertyName) : ValidationAttribute
{
    public string OtherPropertyName { get; } = otherPropertyName;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        string memberName = validationContext.MemberName ?? string.Empty;

        PropertyInfo? otherProperty = validationContext.ObjectType.GetProperty(OtherPropertyName, BindingFlags.Instance | BindingFlags.Public);
        if (otherProperty == null)
            return new($"Object type {validationContext.ObjectType} does not have a public property named \"{OtherPropertyName}\" to validate with.", [memberName]);

        object? otherValue = otherProperty.GetValue(validationContext.ObjectInstance);
        if (value?.Equals(otherValue) != true)
            return new(ErrorMessage ?? $"\"{memberName}\" does not have the same value as \"{OtherPropertyName}\".");

        return null;
    }
}
