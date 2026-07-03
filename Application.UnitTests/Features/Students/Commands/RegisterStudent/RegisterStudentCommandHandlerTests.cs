using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.Students.Commands.RegisterStudent;
using AcademicGateway.Domain.Students;
using AcademicGateway.Domain.Students.Exceptions;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.Students.Commands.RegisterStudent;

/// <summary>
/// Contains isolated unit verification routines for the <see cref="RegisterStudentCommandHandler"/>.
/// Validates identity service provider loops, aggregate root constructor invariants, null-coalescing collection guards,
/// empty array loop conditions, and deep domain mapping constraint propagation.
/// </summary>
public class RegisterStudentCommandHandlerTests
{
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly RegisterStudentCommandHandler _handler;

    /// <summary>
    /// Initializes a pristine instance of the test class, establishing isolated mock dependencies.
    /// </summary>
    public RegisterStudentCommandHandlerTests()
    {
        _identityServiceMock = new Mock<IIdentityService>();
        _dbContextMock = new Mock<IApplicationDbContext>();

        // Setup the mock for the Students table to simulate an empty dataset baseline
        var mockStudentsDbSet = new List<Student>().BuildMockDbSet();
        _dbContextMock.Setup(db => db.Students).Returns(mockStudentsDbSet.Object);

        _handler = new RegisterStudentCommandHandler(_identityServiceMock.Object, _dbContextMock.Object);
    }

    /// <summary>
    /// Assures that passing a valid student registration command containing populated collection ids maps 
    /// credentials correctly, triggers aggregate routines natively, and flushes cross-reference structures.
    /// </summary>
    [Fact]
    public async Task Handle_GivenValidRegistrationDetailsWithFullMatrices_ShouldReturnUserIdAndPersistFullProfile()
    {
        // Arrange
        var expectedUserId = Guid.NewGuid();
        var majorId = Guid.NewGuid();
        var specialtyId = Guid.NewGuid();
        var skillId = Guid.NewGuid();

        var command = new RegisterStudentCommand
        {
            Username = "academicscholar",
            Email = "scholar@university.edu",
            Password = "SecurePassword123!",
            FullName = "Jane Doe",
            GraduationYear = 2027,
            MajorIds = new List<Guid> { majorId },
            SpecialtyIds = new List<Guid> { specialtyId },
            SkillIds = new List<Guid> { skillId }
        };

        _identityServiceMock
            .Setup(x => x.CreateUserAsync(command.Username, command.Email, command.Password))
            .ReturnsAsync((true, expectedUserId, new List<string>()));

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.Should().Be(expectedUserId);

        // Deep verify that the Student aggregate root tracking layout holds all mapped nested collections
        _dbContextMock.Verify(x => x.Students.Add(It.Is<Student>(s =>
            s.Id == expectedUserId &&
            s.FullName == command.FullName &&
            s.GraduationYear == command.GraduationYear &&
            s.StudentMajors.Count == 1 && s.StudentMajors.Any(m => m.MajorId == majorId) &&
            s.StudentSpecialties.Count == 1 && s.StudentSpecialties.Any(sp => sp.SpecialtyId == specialtyId) &&
            s.StudentSkills.Count == 1 && s.StudentSkills.Any(sk => sk.SkillId == skillId)
        )), Times.Once);

        _dbContextMock.Verify(x => x.SaveChangesAsync(TestContext.Current.CancellationToken), Times.Once);
    }

    /// <summary>
    /// Assures that when command sub-collections are passed as null arguments, the handler's internal null 
    /// guard guards process successfully, avoiding NullReferenceExceptions and tracking an isolated student structure.
    /// </summary>
    [Fact]
    public async Task Handle_GivenNullCollections_ShouldCreateStudentSafelyWithZeroMappers()
    {
        // Arrange
        var expectedUserId = Guid.NewGuid();
        var command = new RegisterStudentCommand
        {
            Username = "soloscholar",
            Email = "solo@university.edu",
            Password = "SecurePassword123!",
            FullName = "Jane Doe",
            GraduationYear = 2028,
            MajorIds = null!,      // Explicitly checking the null branch paths
            SpecialtyIds = null!,
            SkillIds = null!
        };

        _identityServiceMock
            .Setup(x => x.CreateUserAsync(command.Username, command.Email, command.Password))
            .ReturnsAsync((true, expectedUserId, new List<string>()));

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.Should().Be(expectedUserId);

        _dbContextMock.Verify(x => x.Students.Add(It.Is<Student>(s =>
            s.Id == expectedUserId &&
            s.StudentMajors.Count == 0 &&
            s.StudentSpecialties.Count == 0 &&
            s.StudentSkills.Count == 0
        )), Times.Once);

        _dbContextMock.Verify(x => x.SaveChangesAsync(TestContext.Current.CancellationToken), Times.Once);
    }

    /// <summary>
    /// Assures that passing initialized but empty list matrices processes flawless runs, saving 
    /// a valid student record that maps zero relational intersection components.
    /// </summary>
    [Fact]
    public async Task Handle_GivenEmptyCollections_ShouldCreateStudentSafelyWithZeroMappers()
    {
        // Arrange
        var expectedUserId = Guid.NewGuid();
        var command = new RegisterStudentCommand
        {
            Username = "emptyscholar",
            Email = "empty@university.edu",
            Password = "SecurePassword123!",
            FullName = "Jane Doe",
            GraduationYear = 2029,
            MajorIds = new List<Guid>(),
            SpecialtyIds = new List<Guid>(),
            SkillIds = new List<Guid>()
        };

        _identityServiceMock
            .Setup(x => x.CreateUserAsync(command.Username, command.Email, command.Password))
            .ReturnsAsync((true, expectedUserId, new List<string>()));

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.Should().Be(expectedUserId);

        _dbContextMock.Verify(x => x.Students.Add(It.Is<Student>(s =>
            s.StudentMajors.Count == 0 &&
            s.StudentSpecialties.Count == 0 &&
            s.StudentSkills.Count == 0
        )), Times.Once);

        _dbContextMock.Verify(x => x.SaveChangesAsync(TestContext.Current.CancellationToken), Times.Once);
    }

    /// <summary>
    /// Assures that if onboarding provisioning fails at the infrastructure identity tier,
    /// an <see cref="InvalidOperationException"/> is thrown and data mutations are entirely aborted.
    /// </summary>
    [Fact]
    public async Task Handle_GivenIdentityServiceFailure_ShouldThrowInvalidOperationExceptionAndAbortPersistence()
    {
        // Arrange
        var command = new RegisterStudentCommand
        {
            Username = "baduser",
            Email = "bad@example.com",
            Password = "Password123!",
            FullName = "John Doe"
        };

        var errors = new List<string> { "Username already claimed by an existing profile", "Password strength criteria invalid" };

        _identityServiceMock
            .Setup(x => x.CreateUserAsync(command.Username, command.Email, command.Password))
            .ReturnsAsync((false, Guid.Empty, errors));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Student identity configuration failed*Username already claimed*Password strength criteria invalid*");

        _dbContextMock.Verify(x => x.Students.Add(It.IsAny<Student>()), Times.Never);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that if identity service configuration generates success indicators but leaves a blank User Guid, 
    /// the domain layer boundary intercepts processing and bubbles up an <see cref="InvalidStudentDetailsException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenIdentityReturnsEmptyGuid_ShouldPropagateInvalidStudentDetailsException()
    {
        // Arrange
        var command = new RegisterStudentCommand
        {
            Username = "malformeduser",
            Email = "malformed@example.com",
            Password = "Password123!",
            FullName = "John Doe"
        };

        _identityServiceMock
            .Setup(x => x.CreateUserAsync(command.Username, command.Email, command.Password))
            .ReturnsAsync((true, Guid.Empty, new List<string>())); // Violation: Successful return with empty Guid tracker

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidStudentDetailsException>()
            .WithMessage("*Identity User ID reference context cannot be empty.*");

        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that if an invalid or whitespace display name is supplied to the handler, the core domain 
    /// aggregate protections fail, throwing an <see cref="InvalidStudentDetailsException"/>.
    /// Note: Input string is marked nullable (string?) to eliminate compiler reference typing warning checks.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("     ")]
    [InlineData(null)]
    public async Task Handle_GivenInvalidFullName_ShouldPropagateInvalidStudentDetailsExceptionAndAbort(string? invalidName)
    {
        // Arrange
        var command = new RegisterStudentCommand
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123!",
            FullName = invalidName!,
            GraduationYear = 2026
        };

        _identityServiceMock
            .Setup(x => x.CreateUserAsync(command.Username, command.Email, command.Password))
            .ReturnsAsync((true, Guid.NewGuid(), new List<string>()));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidStudentDetailsException>()
            .WithMessage("*Student identity name fields cannot be empty or whitespace.*");

        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that if the targeted completion timeline value drops beneath historical baseline parameters,
    /// the aggregate structure prevents instantiation, throwing an <see cref="InvalidGraduationYearException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenGraduationYearPriorToBaseline_ShouldPropagateInvalidGraduationYearExceptionAndAbort()
    {
        // Arrange
        var command = new RegisterStudentCommand
        {
            Username = "historicaluser",
            Email = "history@example.com",
            Password = "Password123!",
            FullName = "John Doe",
            GraduationYear = 1999 // Violation: Baseline minimum limit is set to 2000 index
        };

        _identityServiceMock
            .Setup(x => x.CreateUserAsync(command.Username, command.Email, command.Password))
            .ReturnsAsync((true, Guid.NewGuid(), new List<string>()));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidGraduationYearException>();
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that supplying an empty tracking Guid inside the Major collection invokes aggregate root 
    /// validation guards, throwing an <see cref="InvalidStudentDetailsException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenEmptyGuidInMajorIds_ShouldPropagateInvalidStudentDetailsExceptionAndAbort()
    {
        // Arrange
        var command = new RegisterStudentCommand
        {
            Username = "badmajoruser",
            Email = "major@example.com",
            Password = "Password123!",
            FullName = "John Doe",
            MajorIds = new List<Guid> { Guid.Empty } // Violation: Malformed identification key tracker
        };

        _identityServiceMock
            .Setup(x => x.CreateUserAsync(command.Username, command.Email, command.Password))
            .ReturnsAsync((true, Guid.NewGuid(), new List<string>()));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidStudentDetailsException>()
            .WithMessage("*Target reference major identification context cannot be empty.*");

        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that supplying an empty tracking Guid inside the Specialty concentration collection invokes 
    /// aggregate validation guards, throwing an <see cref="InvalidStudentDetailsException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenEmptyGuidInSpecialtyIds_ShouldPropagateInvalidStudentDetailsExceptionAndAbort()
    {
        // Arrange
        var command = new RegisterStudentCommand
        {
            Username = "badspecialtyuser",
            Email = "specialty@example.com",
            Password = "Password123!",
            FullName = "John Doe",
            SpecialtyIds = new List<Guid> { Guid.Empty }
        };

        _identityServiceMock
            .Setup(x => x.CreateUserAsync(command.Username, command.Email, command.Password))
            .ReturnsAsync((true, Guid.NewGuid(), new List<string>()));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidStudentDetailsException>()
            .WithMessage("*Target reference sub-track specialty identification context cannot be empty.*");

        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that supplying an empty tracking Guid inside the competency Skills inventory collection invokes 
    /// aggregate validation guards, throwing an <see cref="InvalidStudentDetailsException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenEmptyGuidInSkillIds_ShouldPropagateInvalidStudentDetailsExceptionAndAbort()
    {
        // Arrange
        var command = new List<Guid> { Guid.Empty };
        var registerCommand = new RegisterStudentCommand
        {
            Username = "badskilluser",
            Email = "skills@example.com",
            Password = "Password123!",
            FullName = "John Doe",
            SkillIds = command
        };

        _identityServiceMock
            .Setup(x => x.CreateUserAsync(registerCommand.Username, registerCommand.Email, registerCommand.Password))
            .ReturnsAsync((true, Guid.NewGuid(), new List<string>()));

        // Act
        Func<Task> act = async () => await _handler.Handle(registerCommand, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidStudentDetailsException>()
            .WithMessage("*Target reference skill identification context cannot be empty.*");

        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}