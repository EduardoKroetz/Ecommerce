namespace Ecommerce.Application.Policies;

/// <summary>
/// Source of truth for password rules. Consumed by request DTOs (early 400) and by the Identity configuration.
/// </summary>
public static class PasswordPolicy
{
    public const int MinLength = 6;
    public const int RequiredUniqueChars = 1;
    public const bool RequireDigit = false;
    public const bool RequireLowercase = true;
    public const bool RequireUppercase = false;
    public const bool RequireNonAlphanumeric = false;
}
