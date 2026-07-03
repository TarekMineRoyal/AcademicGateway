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
/// Evaluates identity extensions, character length boundaries, selection thresholds, 
/// null collection safeguards, and cross-relational lookup verification filters.
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

        // Best Practice: Default configure database lookups to return empty mock datasets
        _dbContextMock.Setup(x => x.Skills).Returns(new List<Skill>().BuildMockDbSet().Object);
        _dbContextMock.Setup(x => x.Specialties).Returns(new List<Specialty>().BuildMockDbSet().Object);
    }

    /// <summary>
    /// Assures that formatting matches requirements, structural sub-specialties match chosen major parents, 
    /// and required profiles are complete, the validator registers passing marks with zero errors.
    /// </summary>
    [Fact]
    public async Task Validate_GivenValidCommandConfiguration_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        var major = new Major("Computer Science");
        major.AddSpecialty("Software Engineering");
        var specialty = major.Specialties.First();

        var skill = new Skill("C# Programming");

        _dbContextMock.Setup(x => x.Specialties).Returns(new List<Specialty> { specialty }.BuildMockDbSet().Object);
        _dbContextMock.Setup(x => x.Skills).Returns(new List<Skill> { skill }.BuildMockDbSet().Object);

        var command = new RegisterStudentCommand
        {
            Email = "valid@example.com",
            Username = "validuser",
            Password = "ValidPassword123!",
            FullName = "John Doe",
            MajorIds = new List<Guid> { major.Id },
            SpecialtyIds = new List<Guid> { specialty.Id },
            SkillIds = new List<Guid> { skill.Id }
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Assures that if an incoming command string supplies an empty or whitespace email configuration,
    /// the custom dry extensions framework safely logs an intercept validation error.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Validate_GivenInvalidEmail_ShouldHaveValidationError(string? invalidEmail)
    {
        // Arrange
        var command = new RegisterStudentCommand { Email = invalidEmail! };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    /// <summary>
    /// Assures that if a username parameter is empty, whitespace, or null, identity rules log an error.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("      ")]
    [InlineData(null)]
    public async Task Validate_GivenInvalidUsername_ShouldHaveValidationError(string? invalidUsername)
    {
        // Arrange
        var command = new RegisterStudentCommand { Username = invalidUsername! };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username);
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
    /// Assures that providing an empty or whitespace name logs a validation error with the proper message.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Validate_GivenInvalidFullName_ShouldHaveValidationErrorWithCorrectMessage(string? invalidName)
    {
        // Arrange
        var command = new RegisterStudentCommand { FullName = invalidName! };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FullName)
            .WithErrorMessage("Student profile display full name cannot be empty or whitespace.");
    }

    /// <summary>
    /// Assures that providing a full name that exceeds 150 characters violates data constraints.
    /// </summary>
    [Fact]
    public async Task Validate_GivenFullNameExceedingLimit_ShouldHaveValidationErrorWithCorrectMessage()
    {
        // Arrange
        var bloatedName = new string('A', 151);
        var command = new RegisterStudentCommand { FullName = bloatedName };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FullName)
            .WithErrorMessage("Full name description details cannot exceed 150 characters.");
    }

    /// <summary>
    /// Assures that a null or empty collection list of majors fails rule verification.
    /// </summary>
    [Fact]
    public async Task Validate_GivenNullOrEmptyMajors_ShouldHaveValidationErrorWithCorrectMessage()
    {
        // Arrange
        var commandWithNull = new RegisterStudentCommand { MajorIds = null! };
        var commandWithEmpty = new RegisterStudentCommand { MajorIds = new List<Guid>() };

        // Act & Assert
        var resultNull = await _validator.TestValidateAsync(commandWithNull, cancellationToken: TestContext.Current.CancellationToken);
        resultNull.ShouldHaveValidationErrorFor(x => x.MajorIds)
            .WithErrorMessage("You must select at least one core academic Major program.");

        var resultEmpty = await _validator.TestValidateAsync(commandWithEmpty, cancellationToken: TestContext.Current.CancellationToken);
        resultEmpty.ShouldHaveValidationErrorFor(x => x.MajorIds)
            .WithErrorMessage("You must select at least one core academic Major program.");
    }

    /// <summary>
    /// Assures that passing more than 3 major identifiers triggers a collection capacity violation.
    /// </summary>
    [Fact]
    public async Task Validate_GivenMoreThanThreeMajors_ShouldHaveValidationErrorWithCorrectMessage()
    {
        // Arrange
        var command = new RegisterStudentCommand
        {
            MajorIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() }
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MajorIds)
            .WithErrorMessage("You can select a maximum of 3 academic majors simultaneously.");
    }

    /// <summary>
    /// Assures that passing more than 5 educational sub-specialties triggers a validation error.
    /// </summary>
    [Fact]
    public async Task Validate_GivenMoreThanFiveSpecialties_ShouldHaveValidationErrorWithCorrectMessage()
    {
        // Arrange
        var command = new RegisterStudentCommand
        {
            SpecialtyIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() }
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SpecialtyIds)
            .WithErrorMessage("You can select a maximum of 5 educational sub-specialties.");
    }

    /// <summary>
    /// Assures that passing null or empty specialty collections safely triggers short-circuit bypass paths.
    /// </summary>
    [Fact]
    public async Task Validate_GivenNullOrEmptySpecialties_ShouldNotThrowOrCauseRelationalValidationFailures()
    {
        // Arrange
        var commandWithNull = new RegisterStudentCommand { SpecialtyIds = null! };
        var commandWithEmpty = new RegisterStudentCommand { SpecialtyIds = new List<Guid>() };

        // Act
        var resultNull = await _validator.TestValidateAsync(commandWithNull, cancellationToken: TestContext.Current.CancellationToken);
        var resultEmpty = await _validator.TestValidateAsync(commandWithEmpty, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        resultNull.ShouldNotHaveValidationErrorFor(x => x.SpecialtyIds);
        resultEmpty.ShouldNotHaveValidationErrorFor(x => x.SpecialtyIds);
    }

    /// <summary>
    /// Assures that if a sub-specialty does not map back to a major selected in the command payload,
    /// a hierarchical relationship validation violation is successfully captured.
    /// </summary>
    [Fact]
    public async Task Validate_GivenSpecialtyOutsideOfSelectedMajors_ShouldHaveValidationErrorWithCorrectMessage()
    {
        // Arrange
        var selectedMajor = new Major("Computer Science");
        var unselectedMajor = new Major("Mechanical Engineering");
        unselectedMajor.AddSpecialty("Robotics Kinematics");
        var foreignSpecialty = unselectedMajor.Specialties.First();

        // Hydrate mock context database with the specialty
        _dbContextMock.Setup(x => x.Specialties).Returns(new List<Specialty> { foreignSpecialty }.BuildMockDbSet().Object);

        var command = new RegisterStudentCommand
        {
            MajorIds = new List<Guid> { selectedMajor.Id }, // Selected Major Id doesn't match the specialty's parent ID
            SpecialtyIds = new List<Guid> { foreignSpecialty.Id }
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SpecialtyIds)
            .WithErrorMessage("One or more selected specialties do not belong to your chosen academic majors.");
    }

    /// <summary>
    /// Assures that trying to associate more than 20 skills triggers a capacity threshold failure.
    /// </summary>
    [Fact]
    public async Task Validate_GivenMoreThanTwentySkills_ShouldHaveValidationErrorWithCorrectMessage()
    {
        // Arrange
        var bloatedSkillIds = Enumerable.Range(1, 21).Select(_ => Guid.NewGuid()).ToList();
        var command = new RegisterStudentCommand { SkillIds = bloatedSkillIds };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SkillIds)
            .WithErrorMessage("You cannot assign more than 20 capability skills to your profile inventory.");
    }

    /// <summary>
    /// Assures that passing null or empty skill collections short-circuits evaluation without database context lookups.
    /// </summary>
    [Fact]
    public async Task Validate_GivenNullOrEmptySkills_ShouldBypassDatabaseVerificationSafely()
    {
        // Arrange
        var commandWithNull = new RegisterStudentCommand { SkillIds = null! };
        var commandWithEmpty = new RegisterStudentCommand { SkillIds = new List<Guid>() };

        // Act
        var resultNull = await _validator.TestValidateAsync(commandWithNull, cancellationToken: TestContext.Current.CancellationToken);
        var resultEmpty = await _validator.TestValidateAsync(commandWithEmpty, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        resultNull.ShouldNotHaveValidationErrorFor(x => x.SkillIds);
        resultEmpty.ShouldNotHaveValidationErrorFor(x => x.SkillIds);
        _dbContextMock.Verify(x => x.Skills, Times.Never);
    }

    /// <summary>
    /// Assures that if any provided skill unique identifiers cannot be cross-referenced with active 
    /// records in the database directory, a validation block is generated.
    /// </summary>
    [Fact]
    public async Task Validate_GivenNonExistentSkillId_ShouldHaveValidationErrorWithCorrectMessage()
    {
        // Arrange
        var validSkill = new Skill("Python Automation");
        var missingSkillId = Guid.NewGuid();

        _dbContextMock.Setup(x => x.Skills).Returns(new List<Skill> { validSkill }.BuildMockDbSet().Object);

        var command = new RegisterStudentCommand
        {
            SkillIds = new List<Guid> { validSkill.Id, missingSkillId } // 1 valid, 1 missing. Database count will be 1 instead of 2.
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SkillIds)
            .WithErrorMessage("One or more selected technical capability skills do not exist within the system directory.");
    }
}