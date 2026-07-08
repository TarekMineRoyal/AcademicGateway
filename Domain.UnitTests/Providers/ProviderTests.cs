using System;
using System.Linq;
using AcademicGateway.Domain.Providers;
using AcademicGateway.Domain.Providers.Events;
using AcademicGateway.Domain.Providers.Exceptions;
using FluentAssertions;
using Xunit;

namespace AcademicGateway.Domain.UnitTests.Providers;

public class ProviderTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldInitializeCorrectly_WhenParametersAreValid()
    {
        // Arrange
        var validId = Guid.NewGuid();
        var untrimmedCompanyName = "   Cyberdyne Systems Corp   ";
        var expectedCompanyName = "Cyberdyne Systems Corp";

        // Act
        var provider = new Provider(validId, untrimmedCompanyName);

        // Assert
        provider.Id.Should().Be(validId);
        provider.CompanyName.Should().Be(expectedCompanyName);
        provider.CompanyDescription.Should().BeEmpty();
        provider.WebsiteUrl.Should().BeEmpty();
        provider.IsVerified.Should().BeFalse();
        provider.ProjectTemplates.Should().BeEmpty();
        provider.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidProviderDetailsException_WhenIdIsEmpty()
    {
        // Act
        Action act = () => _ = new Provider(Guid.Empty, "Stark Industries");

        // Assert
        act.Should().Throw<InvalidProviderDetailsException>()
           .WithMessage("Identity User ID cannot be empty.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldThrowInvalidProviderDetailsException_WhenCompanyNameIsInvalid(string? invalidName)
    {
        // Act
        Action act = () => _ = new Provider(Guid.NewGuid(), invalidName!);

        // Assert
        act.Should().Throw<InvalidProviderDetailsException>()
           .WithMessage("Company name cannot be empty or whitespace.");
    }

    #endregion

    #region UpdateCompanyName Tests

    [Fact]
    public void UpdateCompanyName_ShouldModifyAndTrim_WhenNameIsValid()
    {
        // Arrange
        var provider = new Provider(Guid.NewGuid(), "Stark Industries");
        var newUntrimmedName = "  Stark Industries International  ";
        var expectedName = "Stark Industries International";

        // Act
        provider.UpdateCompanyName(newUntrimmedName);

        // Assert
        provider.CompanyName.Should().Be(expectedName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateCompanyName_ShouldThrowInvalidProviderDetailsException_WhenNameIsInvalid(string? invalidName)
    {
        // Arrange
        var provider = new Provider(Guid.NewGuid(), "Wayne Enterprises");

        // Act
        Action act = () => provider.UpdateCompanyName(invalidName!);

        // Assert
        act.Should().Throw<InvalidProviderDetailsException>()
           .WithMessage("Company name cannot be empty or whitespace.");
    }

    #endregion

    #region UpdateProfileDetails Tests

    [Fact]
    public void UpdateProfileDetails_ShouldModifyAndTrim_WhenParametersAreValid()
    {
        // Arrange
        var provider = new Provider(Guid.NewGuid(), "Oscorp");
        var description = "  Leading biotechnical research firm.  ";
        var website = "  https://oscorp.com  ";

        // Act
        provider.UpdateProfileDetails(description, website);

        // Assert
        provider.CompanyDescription.Should().Be("Leading biotechnical research firm.");
        provider.WebsiteUrl.Should().Be("https://oscorp.com");
    }

    [Fact]
    public void UpdateProfileDetails_ShouldHandleNullWebsiteUrl_BySettingEmptyString()
    {
        // Arrange
        var provider = new Provider(Guid.NewGuid(), "Oscorp");
        var description = "Biotechnical research firm.";

        // Act
        provider.UpdateProfileDetails(description, null!);

        // Assert
        provider.CompanyDescription.Should().Be(description);
        provider.WebsiteUrl.Should().Be(string.Empty);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateProfileDetails_ShouldThrowInvalidProviderDetailsException_WhenDescriptionIsInvalid(string? invalidDesc)
    {
        // Arrange
        var provider = new Provider(Guid.NewGuid(), "Tyrell Corp");

        // Act
        Action act = () => provider.UpdateProfileDetails(invalidDesc!, "https://tyrell.io");

        // Assert
        act.Should().Throw<InvalidProviderDetailsException>()
           .WithMessage("Company description cannot be empty or whitespace.");
    }

    #endregion

    #region Verification Lifecycle Tests

    [Fact]
    public void VerifyProfile_ShouldSetIsVerifiedToTrue()
    {
        // Arrange
        var provider = new Provider(Guid.NewGuid(), "Umbrella Corp");

        // Act
        provider.VerifyProfile();

        // Assert
        provider.IsVerified.Should().BeTrue();
    }

    [Fact]
    public void RevokeVerification_ShouldSetIsVerifiedToFalseAndAppendDomainEvent()
    {
        // Arrange
        var provider = new Provider(Guid.NewGuid(), "Acme Corp");
        provider.VerifyProfile(); // Flip to true first

        // Act
        provider.RevokeVerification();

        // Assert
        provider.IsVerified.Should().BeFalse();

        // Asserting aggregate domain event tracking mechanics
        provider.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProviderVerificationRevokedEvent>()
            .Which.ProviderId.Should().Be(provider.Id);
    }

    #endregion
}