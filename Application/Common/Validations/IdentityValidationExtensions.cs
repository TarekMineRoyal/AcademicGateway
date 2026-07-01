using FluentValidation;

namespace AcademicGateway.Application.Common.Validations;

/// <summary>
/// Provides centralized, reusable FluentValidation extension rules to enforce baseline identity profile parameters.
/// Guarantees architectural DRY compliance for credentials across registration and authentication boundaries.
/// </summary>
public static class IdentityValidationExtensions
{
    /// <summary>
    /// Chains standard validation policies for a security account email address.
    /// Enforces presence, structural email formatting boundaries, and identity-store data length scales.
    /// </summary>
    /// <typeparam name="T">The type of the target command or request executing validation filters.</typeparam>
    /// <param name="ruleBuilder">The incoming validation rule constructor instance tracking the target string entry.</param>
    /// <returns>The extended <see cref="IRuleBuilderOptions{T, String}"/> context mapping to chained rule constraints.</returns>
    public static IRuleBuilderOptions<T, string> ValidIdentityEmail<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Email address is required and cannot be empty.")
            .EmailAddress().WithMessage("A legitimate, standard email address structure format is required.")
            .MaximumLength(256).WithMessage("Email address cannot exceed the database boundary scale limit of 256 characters.");
    }

    /// <summary>
    /// Chains validation scale filters for a profile identification username.
    /// Enforces character min/max lengths to comply with central platform authentication gates.
    /// </summary>
    /// <typeparam name="T">The type of the target command or request executing validation filters.</typeparam>
    /// <param name="ruleBuilder">The incoming validation rule constructor instance tracking the target string entry.</param>
    /// <returns>The extended <see cref="IRuleBuilderOptions{T, String}"/> context mapping to chained rule constraints.</returns>
    public static IRuleBuilderOptions<T, string> ValidIdentityUsername<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Username credentials are required and cannot be empty.")
            .MinimumLength(3).WithMessage("Username scale parameters require at least 3 characters.")
            .MaximumLength(50).WithMessage("Username profile details cannot exceed 50 characters.");
    }

    /// <summary>
    /// Chains strict entropy complexity filters for plain-text account security passwords.
    /// Enforces uppercase, lowercase, numeric digits, non-alphanumeric special symbols, and scale bounds.
    /// </summary>
    /// <typeparam name="T">The type of the target command or request executing validation filters.</typeparam>
    /// <param name="ruleBuilder">The incoming validation rule constructor instance tracking the target string entry.</param>
    /// <returns>The extended <see cref="IRuleBuilderOptions{T, String}"/> context mapping to chained rule constraints.</returns>
    public static IRuleBuilderOptions<T, string> ValidIdentityPassword<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Account security password is required.")
            .MinimumLength(8).WithMessage("Password complexity requires at least 8 characters.")
            .MaximumLength(100).WithMessage("Password parameters cannot exceed an upper limit of 100 characters.")
            .Matches(@"[A-Z]").WithMessage("Password complexity rules require at least one uppercase letter (A-Z).")
            .Matches(@"[a-z]").WithMessage("Password complexity rules require at least one lowercase letter (a-z).")
            .Matches(@"[0-9]").WithMessage("Password complexity rules require at least one structural numeric digit (0-9).")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Password complexity rules require at least one custom non-alphanumeric special character.");
    }
}