using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Common.Validations;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.Students.Commands.RegisterStudent;

/// <summary>
/// Validates incoming arguments for the <see cref="RegisterStudentCommand"/> before handler routing occurs.
/// Enforces business rule constraints, input sanitization boundaries, and cross-relational lookup verification.
/// </summary>
public class RegisterStudentCommandValidator : AbstractValidator<RegisterStudentCommand>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Initializes functional format constraints and cross-cutting system validation filters for student registration.
    /// </summary>
    /// <param name="context">The relational database context mapping system data dependencies.</param>
    public RegisterStudentCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        // Base Identity Domain Validations (DRY)
        RuleFor(x => x.Email).ValidIdentityEmail();
        RuleFor(x => x.Username).ValidIdentityUsername();
        RuleFor(x => x.Password).ValidIdentityPassword();

        // Enforce Core Profile Integrity Constraints
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Student profile display full name cannot be empty or whitespace.")
            .MaximumLength(150).WithMessage("Full name description details cannot exceed 150 characters.");

        // Relational Validations - Selection Payload Limits
        RuleFor(x => x.MajorIds)
            .NotEmpty().WithMessage("You must select at least one core academic Major program.")
            .Must(list => list != null && list.Count <= 3).WithMessage("You can select a maximum of 3 academic majors simultaneously.");

        RuleFor(x => x.SpecialtyIds)
            .Must(list => list == null || list.Count <= 5).WithMessage("You can select a maximum of 5 educational sub-specialties.")
            .MustAsync(async (command, specialtyIds, cancellationToken) => await SpecialtiesMustBelongToSelectedMajors(command, specialtyIds, cancellationToken))
            .WithMessage("One or more selected specialties do not belong to your chosen academic majors.");

        RuleFor(x => x.SkillIds)
            .Must(list => list == null || list.Count <= 20).WithMessage("You cannot assign more than 20 capability skills to your profile inventory.")
            .MustAsync(SkillsMustExistInDatabase).WithMessage("One or more selected technical capability skills do not exist within the system directory.");
    }

    /// <summary>
    /// Asynchronously validates that all selected sub-specialty track identifiers are child entries mapped to the requested majors.
    /// </summary>
    private async Task<bool> SpecialtiesMustBelongToSelectedMajors(
        RegisterStudentCommand command,
        IReadOnlyCollection<Guid> specialtyIds,
        CancellationToken cancellationToken)
    {
        if (specialtyIds == null || !specialtyIds.Any())
        {
            return true;
        }

        var validSpecialtyIdsForMajors = await _context.Specialties
            .Where(s => command.MajorIds.Contains(s.MajorId))
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);

        return specialtyIds.All(id => validSpecialtyIdsForMajors.Contains(id));
    }

    /// <summary>
    /// Asynchronously validates that all provided technical capability identifiers correspond to active system records.
    /// </summary>
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