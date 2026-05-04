using System.ComponentModel.DataAnnotations;

namespace ExamManagement.Api.Helpers;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class StrongPasswordAttribute : ValidationAttribute
{
    public StrongPasswordAttribute()
        : base("Password must be at least 8 characters and contain uppercase, lowercase, and number characters.")
    {
    }

    public override bool IsValid(object? value)
    {
        if (value is not string password)
        {
            return false;
        }

        return password.Length >= 8
            && password.Any(char.IsUpper)
            && password.Any(char.IsLower)
            && password.Any(char.IsDigit);
    }
}
