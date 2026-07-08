using System;
using AcademicGateway.Domain.Professors;
using AcademicGateway.Domain.Professors.Exceptions;
using FluentAssertions;
using Xunit;

namespace AcademicGateway.Domain.UnitTests.Professors;

public class ResearchInterestTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldInitializeCorrectly_WhenAreaIsValid()
    {
        // Arrange
        var untrimmedArea = "   Distributed Systems   ";
        var expectedArea = "Distributed Systems";

        // Act
        var researchInterest = new ResearchInterest(untrimmedArea);

        // Assert
        researchInterest.Id.Should().NotBeEmpty();
        researchInterest.Area.Should().Be(expectedArea);
        researchInterest.ProfessorLinks.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldThrowEmptyResearchInterestAreaException_WhenAreaIsNullOrEmptyOrWhitespace(string? invalidArea)
    {
        // Act
        Action act = () => _ = new ResearchInterest(invalidArea!);

        // Assert
        act.Should().Throw<EmptyResearchInterestAreaException>();
    }

    #endregion

    #region UpdateArea Tests

    [Fact]
    public void UpdateArea_ShouldModifyAreaAndTrim_WhenNewAreaIsValid()
    {
        // Arrange
        var researchInterest = new ResearchInterest("Computer Vision");
        var newValidArea = "  Advanced Computer Vision  ";
        var expectedArea = "Advanced Computer Vision";

        // Act
        researchInterest.UpdateArea(newValidArea);

        // Assert
        researchInterest.Area.Should().Be(expectedArea);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateArea_ShouldThrowEmptyResearchInterestAreaException_WhenNewAreaIsInvalid(string? invalidArea)
    {
        // Arrange
        var researchInterest = new ResearchInterest("Cryptography");

        // Act
        Action act = () => researchInterest.UpdateArea(invalidArea!);

        // Assert
        act.Should().Throw<EmptyResearchInterestAreaException>();
    }

    #endregion
}