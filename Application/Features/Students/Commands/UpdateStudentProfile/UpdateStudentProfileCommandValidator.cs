using AcademicGateway.Application.Common.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.Students.Commands.UpdateStudentProfile;

/// <summary>
/// Validates incoming arguments for the <see cref="UpdateStudentProfileCommand"/> before handler routing occurs.
/// Enforces business rule constraints, length limits, and cross-relational lookup verification against active databases.
/// </summary>
public class UpdateStudentProfileCommandValidator : AbstractValidator<UpdateStudentProfileCommand>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Initializes format constraints and asynchronous directory filters for student profile mutations.
    /// </summary>
    /// <param name="context">The relational database context tracking academic majors, specialties, and technical capability directories.</param>
    public UpdateStudentProfileCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        // Enforce Legal Identity Profile Boundaries
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Student profile display full name cannot be empty or whitespace.")
            .MaximumLength(150).WithMessage("Full name description details cannot exceed 150 characters.");

        // Relational Validations - Core Selection Payload Limits
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
    /// Asynchronously validates that all selected sub-specialty track identifiers are children of the requested majors.
    /// </summary>
    private async Task<bool> SpecialtiesMustBelongToSelectedMajors(
        UpdateStudentProfileCommand command,
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
    /// Asynchronously validates that all provided technical capability identifiers correspond to active system catalog records.
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