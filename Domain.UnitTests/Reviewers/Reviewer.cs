using System;
using AcademicGateway.Domain.Reviewers;
using AcademicGateway.Domain.Reviewers.Exceptions;
using FluentAssertions;
using Xunit;

namespace AcademicGateway.Domain.UnitTests.Reviewers;

public class ReviewerTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldInitializeCorrectly_WhenParametersAreValid()
    {
        // Arrange
        var validId = Guid.NewGuid();
        var untrimmedName = "   Auditor Sarah Connor   ";
        var expectedName = "Auditor Sarah Connor";

        // Act
        var reviewer = new Reviewer(validId, untrimmedName);

        // Assert
        reviewer.Id.Should().Be(validId);
        reviewer.FullName.Should().Be(expectedName);
        reviewer.ReviewedApplications.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidReviewerDetailsException_WhenIdIsEmpty()
    {
        // Arrange
        var emptyId = Guid.Empty;

        // Act
        Action act = () => _ = new Reviewer(emptyId, "Auditor Smith");

        // Assert
        act.Should().Throw<InvalidReviewerDetailsException>()
           .WithMessage("Identity User ID reference context cannot be empty.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldThrowInvalidReviewerDetailsException_WhenFullNameIsInvalid(string? invalidName)
    {
        // Act
        Action act = () => _ = new Reviewer(Guid.NewGuid(), invalidName!);

        // Assert
        act.Should().Throw<InvalidReviewerDetailsException>()
           .WithMessage("Reviewer full name cannot be empty or whitespace.");
    }

    #endregion

    #region UpdateFullName Tests

    [Fact]
    public void UpdateFullName_ShouldModifyNameAndTrim_WhenNewNameIsValid()
    {
        // Arrange
        var reviewer = new Reviewer(Guid.NewGuid(), "Officer K");
        var untrimmedNewName = "  Chief Officer K  ";
        var expectedName = "Chief Officer K";

        // Act
        reviewer.UpdateFullName(untrimmedNewName);

        // Assert
        reviewer.FullName.Should().Be(expectedName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateFullName_ShouldThrowInvalidReviewerDetailsException_WhenNewNameIsInvalid(string? invalidName)
    {
        // Arrange
        var reviewer = new Reviewer(Guid.NewGuid(), "Officer K");

        // Act
        Action act = () => reviewer.UpdateFullName(invalidName!);

        // Assert
        act.Should().Throw<InvalidReviewerDetailsException>()
           .WithMessage("Reviewer full name cannot be empty or whitespace.");
    }

    #endregion
}