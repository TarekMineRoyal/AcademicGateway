using AcademicGateway.Application.Common.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.Providers.Commands.SubmitApplication;

/// <summary>
/// Validates incoming arguments for the <see cref="SubmitProviderApplicationCommand"/> before handler routing occurs.
/// Ensures the active presence of secure upload routes, structural content minimums, and target profile existence.
/// </summary>
public class SubmitProviderApplicationCommandValidator : AbstractValidator<SubmitProviderApplicationCommand>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Initializes functional format constraints and database lookups for provider compliance application submissions.
    /// </summary>
    /// <param name="context">The relational database context mapping system data dependencies.</param>
    public SubmitProviderApplicationCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        // Ensure ProviderId is present and matches a legitimate profile record in our system
        // Architectural Optimization: Removed MaximumLength(128) because ProviderId is now a strongly typed Guid.
        RuleFor(x => x.ProviderId)
            .NotEmpty().WithMessage("Provider ID is required.")
            .MustAsync(ProviderMustExist).WithMessage("The specified Provider profile does not exist.");

        // Ensure corporate profiles provide substantial operational text data
        RuleFor(x => x.CompanyDetails)
            .NotEmpty().WithMessage("Company details are required.")
            .MinimumLength(10).WithMessage("Company details must be at least 10 characters to be considered viable.")
            .MaximumLength(2000).WithMessage("Company details cannot exceed 2000 characters.");

        // Enforce secure asset locations for uploaded compliance auditing documentation
        RuleFor(x => x.VerificationDocumentsUrl)
            .NotEmpty().WithMessage("Verification documents URL is required.")
            .Matches(@"^https?://").WithMessage("Verification documents URL must start with a valid http:// or https:// protocol prefix.")
            .MaximumLength(500).WithMessage("Verification documents URL cannot exceed 500 characters.");
    }

    /// <summary>
    /// Asynchronously validates that the provided tracking identifier maps to an active provider registry record.
    /// </summary>
    private async Task<bool> ProviderMustExist(Guid providerId, CancellationToken cancellationToken)
    {
        if (providerId == Guid.Empty)
        {
            return false;
        }

        // Fixed: Updated lookup target expression from p.UserId to match the primary Guid key field (p.Id)
        return await _context.Providers.AsNoTracking().AnyAsync(p => p.Id == providerId, cancellationToken);
    }
}