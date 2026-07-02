using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.Students.Commands.RegisterStudent;
using AcademicGateway.Domain.Curriculum;
using AcademicGateway.Domain.Skills;
using FluentValidation.TestHelper;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.Students.Commands.RegisterStudent;

/// <summary>
/// Contains unit tests for the <see cref="RegisterStudentCommandValidator"/>.
/// Evaluates text format limits, selection threshold limits, and cross-relational lookup verification filters.
/// </summary>
public class RegisterStudentCommandValidatorTests
{
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly RegisterStudentCommandValidator _validator;

    /// <summary>
    /// Initializes a pristine instance of the validator under evaluation, mapping core mock dependencies.
    /// </summary>
    public RegisterStudentCommandValidatorTests()
    {
        _dbContextMock = new Mock<IApplicationDbContext>();
        _validator = new RegisterStudentCommandValidator(_dbContextMock.Object);

        // Best Practice: Default configure database entity lookups to return empty mock datasets 
        // to prevent unexpected null references during baseline constraint checks.
        _dbContextMock.Setup(x => x.Skills).Returns(new List<Skill>().BuildMockDbSet().Object);
        _dbContextMock.Setup(x => x.Specialties).Returns(new List<Specialty>().BuildMockDbSet().Object);
    }

    /// <summary>
    /// Assures that if an incoming command string supplies an empty or whitespace email configuration,
    /// the custom dry extensions framework safely logs an intercept validation error.
    /// </summary>
    [Fact]
    public async Task Validate_GivenEmptyEmail_ShouldHaveValidationError()
    {
        // Arrange
        var command = new RegisterStudentCommand { Email = string.Empty };

        // Act
        // Best Practice (xUnit1051): Use TestContext.Current.CancellationToken for responsive test shutdowns.
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    /// <summary>
    /// Assures that if a password entry falls below baseline security complexity length limits,
    /// a corresponding validation error is tracked against the password member.
    /// </summary>
    [Fact]
    public async Task Validate_GivenTooShortPassword_ShouldHaveValidationError()
    {
        // Arrange
        var command = new RegisterStudentCommand { Password = "123" };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    /// <summary>
    /// Assures that when formatting matches requirements, structural sub-specialties match chosen major parents, 
    /// and required profiles are complete, the validator registers passing marks with zero errors.
    /// </summary>
    [Fact]
    public async Task Validate_GivenValidCommandConfiguration_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        // Best Practice: Construct curriculum profiles natively via standard DDD patterns 
        // to correctly align IDs and map internal aggregate properties.
        var major = new Major("Computer Science");
        major.AddSpecialty("Software Engineering");

        var specialty = major.Specialties.First();

        // Feed our structured in-memory domain profiles into the mock DB sets
        var mockSpecialtyDbSet = new List<Specialty> { specialty }.BuildMockDbSet();
        _dbContextMock.Setup(x => x.Specialties).Returns(mockSpecialtyDbSet.Object);

        var command = new RegisterStudentCommand
        {
            Email = "valid@example.com",
            Username = "validuser",
            Password = "ValidPassword123!",
            FullName = "John Doe",                 // Required to satisfy core profile integrity rule
            MajorIds = new List<Guid> { major.Id },
            SpecialtyIds = new List<Guid> { specialty.Id },
            SkillIds = Array.Empty<Guid>()
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}