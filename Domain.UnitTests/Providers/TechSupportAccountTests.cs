using System;
using AcademicGateway.Domain.Providers;
using AcademicGateway.Domain.Providers.Exceptions;
using FluentAssertions;
using Xunit;

namespace AcademicGateway.Domain.UnitTests.Providers;

public class TechSupportAccountTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldInitializeCorrectly_WhenParametersAreValid()
    {
        // Arrange
        var validId = Guid.NewGuid();
        var validProviderId = Guid.NewGuid();
        var untrimmedStaffNumber = "   EMP-99823   ";
        var untrimmedSupportTier = "   Tier 3 Systems Admin   ";

        // Act
        var account = new TechSupportAccount(validId, validProviderId, untrimmedStaffNumber, untrimmedSupportTier);

        // Assert
        account.Id.Should().Be(validId);
        account.ProviderId.Should().Be(validProviderId);
        account.StaffNumber.Should().Be("EMP-99823");
        account.SupportTier.Should().Be("Tier 3 Systems Admin");
        account.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidTechSupportDetailsException_WhenIdIsEmpty()
    {
        // Act
        Action act = () => _ = new TechSupportAccount(Guid.Empty, Guid.NewGuid(), "EMP-123", "Tier 1");

        // Assert
        act.Should().Throw<InvalidTechSupportDetailsException>()
           .WithMessage("Identity User ID cannot be empty.");
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidTechSupportDetailsException_WhenProviderIdIsEmpty()
    {
        // Act
        Action act = () => _ = new TechSupportAccount(Guid.NewGuid(), Guid.Empty, "EMP-123", "Tier 1");

        // Assert
        act.Should().Throw<InvalidTechSupportDetailsException>()
           .WithMessage("Parent Provider ID tenant assignment cannot be empty.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldThrowInvalidTechSupportDetailsException_WhenStaffNumberIsInvalid(string? invalidStaffNumber)
    {
        // Act
        Action act = () => _ = new TechSupportAccount(Guid.NewGuid(), Guid.NewGuid(), invalidStaffNumber!, "Tier 2");

        // Assert
        act.Should().Throw<InvalidTechSupportDetailsException>()
           .WithMessage("Staff number cannot be empty or whitespace.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldThrowInvalidTechSupportDetailsException_WhenSupportTierIsInvalid(string? invalidSupportTier)
    {
        // Act
        Action act = () => _ = new TechSupportAccount(Guid.NewGuid(), Guid.NewGuid(), "EMP-123", invalidSupportTier!);

        // Assert
        act.Should().Throw<InvalidTechSupportDetailsException>()
           .WithMessage("Support tier assignment level cannot be empty or whitespace.");
    }

    #endregion

    #region UpdateStaffDetails Tests

    [Fact]
    public void UpdateStaffDetails_ShouldModifyAndTrim_WhenParametersAreValid()
    {
        // Arrange
        var account = new TechSupportAccount(Guid.NewGuid(), Guid.NewGuid(), "EMP-111", "Tier 1");

        // Act
        account.UpdateStaffDetails("  EMP-111-REV A  ", "  Tier 2 Supervisor  ");

        // Assert
        account.StaffNumber.Should().Be("EMP-111-REV A");
        account.SupportTier.Should().Be("Tier 2 Supervisor");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateStaffDetails_ShouldThrowInvalidTechSupportDetailsException_WhenStaffNumberIsInvalid(string? invalidStaffNumber)
    {
        // Arrange
        var account = new TechSupportAccount(Guid.NewGuid(), Guid.NewGuid(), "EMP-111", "Tier 1");

        // Act
        Action act = () => account.UpdateStaffDetails(invalidStaffNumber!, "Tier 2");

        // Assert
        act.Should().Throw<InvalidTechSupportDetailsException>()
           .WithMessage("Staff number cannot be empty or whitespace.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateStaffDetails_ShouldThrowInvalidTechSupportDetailsException_WhenSupportTierIsInvalid(string? invalidSupportTier)
    {
        // Arrange
        var account = new TechSupportAccount(Guid.NewGuid(), Guid.NewGuid(), "EMP-111", "Tier 1");

        // Act
        Action act = () => account.UpdateStaffDetails("EMP-222", invalidSupportTier!);

        // Assert
        act.Should().Throw<InvalidTechSupportDetailsException>()
           .WithMessage("Support tier assignment level cannot be empty or whitespace.");
    }

    #endregion

    #region Account Lifecycle Tests

    [Fact]
    public void DeactivateAccount_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var account = new TechSupportAccount(Guid.NewGuid(), Guid.NewGuid(), "EMP-123", "Tier 1");

        // Act
        account.DeactivateAccount();

        // Assert
        account.IsActive.Should().BeFalse();
    }

    [Fact]
    public void ActivateAccount_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var account = new TechSupportAccount(Guid.NewGuid(), Guid.NewGuid(), "EMP-123", "Tier 1");
        account.DeactivateAccount(); // Flip to false first

        // Act
        account.ActivateAccount();

        // Assert
        account.IsActive.Should().BeTrue();
    }

    #endregion
}