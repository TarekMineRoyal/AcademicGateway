using AcademicGateway.Application.Common.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.ProjectTemplates.Commands.CreateTemplate;

/// <summary>
/// Validates incoming arguments for the <see cref="CreateProjectTemplateCommand"/> before handler routing occurs.
/// Enforces relational metadata consistency checks and string length constraints.
/// </summary>
public class CreateProjectTemplateCommandValidator : AbstractValidator<CreateProjectTemplateCommand>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Initializes business and validation rule constraints for corporate project template drafting.
    /// </summary>
    /// <param name="context">The relational database context mapping configuration dependencies.</param>
    public CreateProjectTemplateCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        // Enforce provider tracking session context presence
        RuleFor(x => x.ProviderId)
            .NotEmpty().WithMessage("Provider ID is required.");

        // Title validation constraints matching baseline domain criteria
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Project title is required.")
            .MinimumLength(5).WithMessage("Project title must be at least 5 characters long.")
            .MaximumLength(100).WithMessage("Project title cannot exceed 100 characters.");

        // Description validation constraints to ensure scope clarity
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Project description is required.")
            .MinimumLength(20).WithMessage("Project description must provide at least 20 characters of detail.")
            .MaximumLength(2000).WithMessage("Project description cannot exceed 2000 characters.");

        // Skill requirements collection criteria checks
        RuleFor(x => x.SkillIds)
            .NotEmpty().WithMessage("At least one required skill must be specified for the project template.")
            .Must(skills => skills != null && skills.Count <= 10).WithMessage("You cannot assign more than 10 required skills to a single template.")
            .MustAsync(SkillsMustExistInDatabase).WithMessage("One or more selected skills do not exist within the system directory.");
    }

    /// <summary>
    /// Asynchronously validates that all provided skill identifiers correspond to active domain database rows.
    /// </summary>
    /// <param name="skillIds">The immutable array of unique identifiers requested by the command wrapper.</param>
    /// <param name="cancellationToken">Propagates notification that network operations should be canceled.</param>
    /// <returns><c>true</c> if every requested skill is verified in the persistent lookup directory; otherwise, <c>false</c>.</returns>
    private async Task<bool> SkillsMustExistInDatabase(
        IReadOnlyCollection<Guid> skillIds,
        CancellationToken cancellationToken)
    {
        if (skillIds == null || !skillIds.Any())
        {
            return true;
        }

        var existingSkillCount = await _context.Skills
            .Where(s => skillIds.Contains(s.Id))
            .CountAsync(cancellationToken);

        return existingSkillCount == skillIds.Count;
    }
}