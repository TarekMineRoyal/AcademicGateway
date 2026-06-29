using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Common.Validations;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.Users.Commands.RegisterStudent;

public class RegisterStudentCommandValidator : AbstractValidator<RegisterStudentCommand>
{
    private readonly IApplicationDbContext _context;

    public RegisterStudentCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        // Base Identity Validations (DRY)
        RuleFor(x => x.Email).ValidIdentityEmail();
        RuleFor(x => x.Username).ValidIdentityUsername();
        RuleFor(x => x.Password).ValidIdentityPassword();

        // Relational Validations - Payload Limits
        RuleFor(x => x.MajorIds)
            .NotEmpty().WithMessage("You must select at least one Major.")
            .Must(list => list.Count <= 3).WithMessage("You can select a maximum of 3 majors.");

        RuleFor(x => x.SpecialtyIds)
            .Must(list => list == null || list.Count <= 5).WithMessage("You can select a maximum of 5 specialties.")
            .MustAsync(SpecialtiesMustBelongToSelectedMajors).WithMessage("One or more selected specialties do not belong to your chosen majors.");

        RuleFor(x => x.SkillIds)
            .Must(list => list == null || list.Count <= 20).WithMessage("You cannot select more than 20 skills.")
            .MustAsync(SkillsMustExistInDatabase).WithMessage("One or more selected skills do not exist.");
    }

    // Custom Database Cross-Reference Rules
    private async Task<bool> SpecialtiesMustBelongToSelectedMajors(
        RegisterStudentCommand command,
        List<Guid> specialtyIds,
        CancellationToken cancellationToken)
    {
        if (specialtyIds == null || !specialtyIds.Any())
            return true;

        var validSpecialtyIdsForMajors = await _context.Specialties
            .Where(s => command.MajorIds.Contains(s.MajorId))
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);

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