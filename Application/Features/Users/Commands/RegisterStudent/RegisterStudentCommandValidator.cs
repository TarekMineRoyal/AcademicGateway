using AcademicGateway.Application.Common.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AcademicGateway.Application.Features.Users.Commands.RegisterStudent;

public class RegisterStudentCommandValidator : AbstractValidator<RegisterStudentCommand>
{
    private readonly IApplicationDbContext _context;

    public RegisterStudentCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        // Basic Validations
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email is required.");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters.");

        // Relational Validations
        RuleFor(x => x.MajorIds)
            .NotEmpty().WithMessage("You must select at least one Major.");

        RuleFor(x => x.SpecialtyIds)
            .MustAsync(SpecialtiesMustBelongToSelectedMajors)
            .WithMessage("One or more selected specialties do not belong to your chosen majors.");

        RuleFor(x => x.SkillIds)
        .MustAsync(SkillsMustExistInDatabase)
        .WithMessage("One or more selected skills do not exist.");
    }

    // Custom Database Cross-Reference Rule
    private async Task<bool> SpecialtiesMustBelongToSelectedMajors(
        RegisterStudentCommand command,
        List<Guid> specialtyIds,
        CancellationToken cancellationToken)
    {
        // If they didn't pick any specialties, that's perfectly fine
        if (specialtyIds == null || !specialtyIds.Any())
            return true;

        // Get the IDs of all valid specialties that belong to the Majors the user selected
        var validSpecialtyIdsForMajors = await _context.Specialties
            .Where(s => command.MajorIds.Contains(s.MajorId))
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);

        // Ensure EVERY specialty the user picked exists in the valid list
        return specialtyIds.All(id => validSpecialtyIdsForMajors.Contains(id));
    }

    private async Task<bool> SkillsMustExistInDatabase(
    List<Guid> skillIds,
    CancellationToken cancellationToken)
    {
        if (skillIds == null || !skillIds.Any())
            return true;

        var existingSkillCount = await _context.Skills
            .Where(s => skillIds.Contains(s.Id))
            .CountAsync(cancellationToken);

        return existingSkillCount == skillIds.Count;
    }
}