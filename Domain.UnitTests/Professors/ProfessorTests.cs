using System;
using System.Linq;
using AcademicGateway.Domain.Professors;
using AcademicGateway.Domain.Professors.Exceptions;
using FluentAssertions;
using Xunit;

namespace AcademicGateway.Domain.UnitTests.Professors;

public class ProfessorTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldInitializeCorrectly_WhenParametersAreValid()
    {
        // Arrange
        var validId = Guid.NewGuid();
        var untrimmedName = "  Dr. Alan Turing  ";
        var untrimmedDept = "  Computer Science  ";
        var untrimmedRank = "  Full Professor  ";
        var maxCapacity = 5;

        // Act
        var professor = new Professor(validId, untrimmedName, untrimmedDept, untrimmedRank, maxCapacity);

        // Assert
        professor.Id.Should().Be(validId);
        professor.FullName.Should().Be("Dr. Alan Turing");
        professor.Department.Should().Be("Computer Science");
        professor.Rank.Should().Be("Full Professor");
        professor.MaxSupervisionCapacity.Should().Be(maxCapacity);
        professor.CurrentProjectCount.Should().Be(0);
        professor.IsAcceptingProjects.Should().BeTrue();
        professor.ResearchInterests.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidProfessorDetailsException_WhenIdIsEmpty()
    {
        // Act
        Action act = () => _ = new Professor(Guid.Empty, "Name", "Dept", "Rank", 3);

        // Assert
        act.Should().Throw<InvalidProfessorDetailsException>()
           .WithMessage("Professor identity tracking reference context cannot be empty.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldThrowInvalidProfessorDetailsException_WhenFullNameIsInvalid(string? invalidName)
    {
        // Act
        Action act = () => _ = new Professor(Guid.NewGuid(), invalidName!, "Dept", "Rank", 3);

        // Assert
        act.Should().Throw<InvalidProfessorDetailsException>()
           .WithMessage("Professor faculty identity full name cannot be empty or whitespace.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldThrowInvalidProfessorDetailsException_WhenDepartmentIsInvalid(string? invalidDept)
    {
        // Act
        Action act = () => _ = new Professor(Guid.NewGuid(), "Name", invalidDept!, "Rank", 3);

        // Assert
        act.Should().Throw<InvalidProfessorDetailsException>()
           .WithMessage("Academic department assignment details cannot be empty or whitespace.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldThrowInvalidProfessorDetailsException_WhenRankIsInvalid(string? invalidRank)
    {
        // Act
        Action act = () => _ = new Professor(Guid.NewGuid(), "Name", "Dept", invalidRank!, 3);

        // Assert
        act.Should().Throw<InvalidProfessorDetailsException>()
           .WithMessage("Faculty positional rank status details cannot be empty or whitespace.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Constructor_ShouldThrowInvalidSupervisionCapacityException_WhenCapacityIsLessThanOrEqualToZero(int invalidCapacity)
    {
        // Act
        Action act = () => _ = new Professor(Guid.NewGuid(), "Name", "Dept", "Rank", invalidCapacity);

        // Assert
        act.Should().Throw<InvalidSupervisionCapacityException>()
           .WithMessage("Initial maximum supervisor project capacity limit bounds must exceed zero.");
    }

    #endregion

    #region UpdateFacultyDetails Tests

    [Fact]
    public void UpdateFacultyDetails_ShouldModifyAndTrim_WhenParametersAreValid()
    {
        // Arrange
        var professor = new Professor(Guid.NewGuid(), "Name", "Dept", "Rank", 3);

        // Act
        professor.UpdateFacultyDetails("  New Name  ", "  New Dept  ", "  New Rank  ");

        // Assert
        professor.FullName.Should().Be("New Name");
        professor.Department.Should().Be("New Dept");
        professor.Rank.Should().Be("New Rank");
    }

    [Theory]
    [InlineData(null, "Dept", "Rank", "Professor faculty identity full name cannot be empty or whitespace.")]
    [InlineData("", "Dept", "Rank", "Professor faculty identity full name cannot be empty or whitespace.")]
    [InlineData("   ", "Dept", "Rank", "Professor faculty identity full name cannot be empty or whitespace.")]
    [InlineData("Name", null, "Rank", "Academic department assignment details cannot be empty or whitespace.")]
    [InlineData("Name", "", "Rank", "Academic department assignment details cannot be empty or whitespace.")]
    [InlineData("Name", "   ", "Rank", "Academic department assignment details cannot be empty or whitespace.")]
    [InlineData("Name", "Dept", null, "Faculty positional rank status details cannot be empty or whitespace.")]
    [InlineData("Name", "Dept", "", "Faculty positional rank status details cannot be empty or whitespace.")]
    [InlineData("Name", "Dept", "   ", "Faculty positional rank status details cannot be empty or whitespace.")]
    public void UpdateFacultyDetails_ShouldThrowInvalidProfessorDetailsException_WhenAnyStringIsInvalid(
        string? name, string? dept, string? rank, string expectedMessage)
    {
        // Arrange
        var professor = new Professor(Guid.NewGuid(), "Valid Name", "Valid Dept", "Valid Rank", 3);

        // Act
        Action act = () => professor.UpdateFacultyDetails(name!, dept!, rank!);

        // Assert
        act.Should().Throw<InvalidProfessorDetailsException>()
           .WithMessage(expectedMessage);
    }

    #endregion

    #region UpdateSupervisionCapacity Tests

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    public void UpdateSupervisionCapacity_ShouldModifyCapacity_WhenValueIsValid(int validCapacity)
    {
        // Arrange
        var professor = new Professor(Guid.NewGuid(), "Name", "Dept", "Rank", 5);

        // Act
        professor.UpdateSupervisionCapacity(validCapacity);

        // Assert
        professor.MaxSupervisionCapacity.Should().Be(validCapacity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void UpdateSupervisionCapacity_ShouldThrowInvalidSupervisionCapacityException_WhenValueIsZeroOrLess(int invalidCapacity)
    {
        // Arrange
        var professor = new Professor(Guid.NewGuid(), "Name", "Dept", "Rank", 5);

        // Act
        Action act = () => professor.UpdateSupervisionCapacity(invalidCapacity);

        // Assert
        act.Should().Throw<InvalidSupervisionCapacityException>()
           .WithMessage("Altered maximum supervisor project capacity limit bounds must exceed zero.");
    }

    [Fact]
    public void UpdateSupervisionCapacity_ShouldThrowInvalidSupervisionCapacityException_WhenValueDropsBeneathActiveAllocations()
    {
        // Arrange
        var professor = new Professor(Guid.NewGuid(), "Name", "Dept", "Rank", 2);
        professor.IncrementActiveProjects();
        professor.IncrementActiveProjects(); // Current count is 2

        // Act
        Action act = () => professor.UpdateSupervisionCapacity(1);

        // Assert
        act.Should().Throw<InvalidSupervisionCapacityException>()
           .WithMessage("*cannot drop beneath the total of current active allocations (2).*");
    }

    #endregion

    #region Increment / Decrement Active Projects Tests

    [Fact]
    public void IncrementActiveProjects_ShouldIncreaseCount_WhenAcceptingProjects()
    {
        // Arrange
        var professor = new Professor(Guid.NewGuid(), "Name", "Dept", "Rank", 2);

        // Act
        professor.IncrementActiveProjects();

        // Assert
        professor.CurrentProjectCount.Should().Be(1);
        professor.IsAcceptingProjects.Should().BeTrue();
    }

    [Fact]
    public void IncrementActiveProjects_ShouldThrowProfessorCapacityReachedException_WhenCapacityIsFull()
    {
        // Arrange
        var professor = new Professor(Guid.NewGuid(), "Name", "Dept", "Rank", 1);
        professor.IncrementActiveProjects(); // Reaches limit

        // Act
        Action act = () => professor.IncrementActiveProjects();

        // Assert
        professor.IsAcceptingProjects.Should().BeFalse();
        act.Should().Throw<ProfessorCapacityReachedException>();
    }

    [Fact]
    public void DecrementActiveProjects_ShouldDecreaseCount_WhenCountIsGreaterThanZero()
    {
        // Arrange
        var professor = new Professor(Guid.NewGuid(), "Name", "Dept", "Rank", 2);
        professor.IncrementActiveProjects();

        // Act
        professor.DecrementActiveProjects();

        // Assert
        professor.CurrentProjectCount.Should().Be(0);
    }

    [Fact]
    public void DecrementActiveProjects_ShouldStayAtZero_WhenCountIsAlreadyZero()
    {
        // Arrange
        var professor = new Professor(Guid.NewGuid(), "Name", "Dept", "Rank", 2);

        // Act
        professor.DecrementActiveProjects();

        // Assert
        professor.CurrentProjectCount.Should().Be(0);
    }

    #endregion

    #region Research Interest Management Tests

    [Fact]
    public void AddResearchInterest_ShouldAttachLink_WhenIdIsValidAndUnique()
    {
        // Arrange
        var professor = new Professor(Guid.NewGuid(), "Name", "Dept", "Rank", 3);
        var interestId = Guid.NewGuid();

        // Act
        professor.AddResearchInterest(interestId);

        // Assert
        professor.ResearchInterests.Should().ContainSingle(ri => ri.ResearchInterestId == interestId);
    }

    [Fact]
    public void AddResearchInterest_ShouldThrowInvalidProfessorDetailsException_WhenIdIsEmpty()
    {
        // Arrange
        var professor = new Professor(Guid.NewGuid(), "Name", "Dept", "Rank", 3);

        // Act
        Action act = () => professor.AddResearchInterest(Guid.Empty);

        // Assert
        act.Should().Throw<InvalidProfessorDetailsException>()
           .WithMessage("Target reference identity context for research alignment links cannot be empty.");
    }

    [Fact]
    public void AddResearchInterest_ShouldNotAddDuplicate_WhenLinkAlreadyExists()
    {
        // Arrange
        var professor = new Professor(Guid.NewGuid(), "Name", "Dept", "Rank", 3);
        var interestId = Guid.NewGuid();
        professor.AddResearchInterest(interestId);

        // Act
        professor.AddResearchInterest(interestId);

        // Assert
        professor.ResearchInterests.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveResearchInterest_ShouldEvictLink_WhenLinkExists()
    {
        // Arrange
        var professor = new Professor(Guid.NewGuid(), "Name", "Dept", "Rank", 3);
        var interestId = Guid.NewGuid();
        professor.AddResearchInterest(interestId);

        // Act
        professor.RemoveResearchInterest(interestId);

        // Assert
        professor.ResearchInterests.Should().BeEmpty();
    }

    [Fact]
    public void RemoveResearchInterest_ShouldDoNothing_WhenLinkDoesNotExist()
    {
        // Arrange
        var professor = new Professor(Guid.NewGuid(), "Name", "Dept", "Rank", 3);
        var activeInterestId = Guid.NewGuid();
        var nonExistentInterestId = Guid.NewGuid();
        professor.AddResearchInterest(activeInterestId);

        // Act
        professor.RemoveResearchInterest(nonExistentInterestId);

        // Assert
        professor.ResearchInterests.Should().HaveCount(1);
    }

    #endregion
}