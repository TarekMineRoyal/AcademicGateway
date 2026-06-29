using FluentValidation;

namespace AcademicGateway.Application.Common.Validations;

public static class IdentityValidationExtensions
{
    public static IRuleBuilderOptions<T, string> ValidIdentityEmail<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email format is required.")
            .MaximumLength(256).WithMessage("Email cannot exceed 256 characters.");
    }

    public static IRuleBuilderOptions<T, string> ValidIdentityUsername<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Username is required.")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters.")
            .MaximumLength(50).WithMessage("Username cannot exceed 50 characters.");
    }

    public static IRuleBuilderOptions<T, string> ValidIdentityPassword<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .MaximumLength(100).WithMessage("Password cannot exceed 100 characters.")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one numeric digit.")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");
    }
}