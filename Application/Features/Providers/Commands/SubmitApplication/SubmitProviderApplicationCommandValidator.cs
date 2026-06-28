using AcademicGateway.Application.Common.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.Providers.Commands.SubmitApplication;

public class SubmitProviderApplicationCommandValidator : AbstractValidator<SubmitProviderApplicationCommand>
{
    private readonly IApplicationDbContext _context;

    public SubmitProviderApplicationCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        // Ensure ProviderId is present and matches a legitimate profile record in our system
        RuleFor(x => x.ProviderId)
            .NotEmpty().WithMessage("Provider ID is required.")
            .MustAsync(ProviderMustExist).WithMessage("The specified Provider profile does not exist.");

        // Ensure corporate profiles provide substantial operational text data
        RuleFor(x => x.CompanyDetails)
            .NotEmpty().WithMessage("Company details are required.")
            .MinimumLength(10).WithMessage("Company details must be at least 10 characters.");

        // Enforce secure asset locations for uploaded compliance auditing documentation
        RuleFor(x => x.VerificationDocumentsUrl)
            .NotEmpty().WithMessage("Verification documents URL is required.")
            .Matches(@"^https?://").WithMessage("Verification documents URL must start with http:// or https://");
    }

    // Database Cross-Reference Rule
    private async Task<bool> ProviderMustExist(string providerId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(providerId)) return false;

        return await _context.Providers.AnyAsync(p => p.UserId == providerId, cancellationToken);
    }
}