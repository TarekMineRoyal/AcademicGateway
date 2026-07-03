using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.ProviderApplications.Commands.SubmitProviderApplication;
using AcademicGateway.Domain.Providers;
using FluentValidation.TestHelper;
using Moq;
using MockQueryable.Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.ProviderApplications.Commands.SubmitProviderApplication;

/// <summary>
/// Production-grade validation suite ensuring structural compliance and relationship integrity
/// validations for incoming <see cref="SubmitProviderApplicationCommand"/> requests.
/// </summary>
public class SubmitProviderApplicationCommandValidatorTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly SubmitProviderApplicationCommandValidator _validator;

    /// <summary>
    /// Initializes test dependencies, setting up the isolated database context mock layer.
    /// </summary>
    public SubmitProviderApplicationCommandValidatorTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _validator = new SubmitProviderApplicationCommandValidator(_mockContext.Object);
    }

    /// <summary>
    /// Verifies that when a command provides completely compliant fields and a legitimate registered
    /// provider, validation rules pass without throwing structural error diagnostics.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithValidCommandParameters_ShouldPassValidation()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var cancellationToken = TestContext.Current.CancellationToken;

        // Maintain domain aggregates structural integrity by using standard constructor pattern
        var provider = new Provider(providerId, "Gateway Corporate Solutions");
        var providerList = new List<Provider> { provider };
        var mockDbSet = providerList.BuildMockDbSet();

        _mockContext.Setup(x => x.Providers).Returns(mockDbSet.Object);

        var command = new SubmitProviderApplicationCommand
        {
            ProviderId = providerId,
            CompanyDetails = "An enterprise scale provider focused on distributed systems research and educational sponsorship.",
            VerificationDocumentsUrl = "https://storage.academicgateway.net/verification/docs/cert.pdf"
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Validates that an empty tracking token for a provider sets off a mandatory non-empty constraint rule violation.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WhenProviderIdIsEmpty_ShouldFailWithRequiredMessage()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;

        // Fixed: Explicitly typed out assignment variable to clear compilation context alignment errors
        var mockDbSet = new List<Provider>().BuildMockDbSet();
        _mockContext.Setup(x => x.Providers).Returns(mockDbSet.Object);

        var command = new SubmitProviderApplicationCommand
        {
            ProviderId = Guid.Empty,
            CompanyDetails = "Valid length operational detail string text example.",
            VerificationDocumentsUrl = "https://gateway.com/doc.pdf"
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProviderId)
              .WithErrorMessage("Provider ID is required.");
    }

    /// <summary>
    /// Ensures that if a populated provider identifier maps onto a missing profile record in the relational store,
    /// a backend query validation warning triggers indicating profile non-existence.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WhenProviderDoesNotExistInDatabase_ShouldFailWithNotFoundMessage()
    {
        // Arrange
        var nonExistentProviderId = Guid.NewGuid();
        var cancellationToken = TestContext.Current.CancellationToken;

        // Seed with empty system repository matrix to mock missing database entity scenario
        var providerList = new List<Provider>();
        var mockDbSet = providerList.BuildMockDbSet();
        _mockContext.Setup(x => x.Providers).Returns(mockDbSet.Object);

        var command = new SubmitProviderApplicationCommand
        {
            ProviderId = nonExistentProviderId,
            CompanyDetails = "Valid length operational detail string text example.",
            VerificationDocumentsUrl = "https://gateway.com/doc.pdf"
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProviderId)
              .WithErrorMessage("The specified Provider profile does not exist.");
    }

    /// <summary>
    /// Tests the lower-bound limits and blank parameter scenarios for corporate textual information strings.
    /// String arguments explicitly marked as nullable to support analyzer validation metrics.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ValidateAsync_WhenCompanyDetailsAreMissingOrBlank_ShouldFailWithRequiredMessage(string? invalidCompanyDetails)
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var cancellationToken = TestContext.Current.CancellationToken;

        var provider = new Provider(providerId, "Quantum Industries LLC");
        var mockDbSet = new List<Provider> { provider }.BuildMockDbSet();
        _mockContext.Setup(x => x.Providers).Returns(mockDbSet.Object);

        var command = new SubmitProviderApplicationCommand
        {
            ProviderId = providerId,
            CompanyDetails = invalidCompanyDetails!,
            VerificationDocumentsUrl = "https://gateway.com/doc.pdf"
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CompanyDetails)
              .WithErrorMessage("Company details are required.");
    }

    /// <summary>
    /// Ensures that operational profiles presenting text below the minimal acceptable threshold fail with structural warnings.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WhenCompanyDetailsAreTooShort_ShouldFailWithLengthConstraintMessage()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var cancellationToken = TestContext.Current.CancellationToken;

        var provider = new Provider(providerId, "Quantum Industries LLC");
        var mockDbSet = new List<Provider> { provider }.BuildMockDbSet();
        _mockContext.Setup(x => x.Providers).Returns(mockDbSet.Object);

        var command = new SubmitProviderApplicationCommand
        {
            ProviderId = providerId,
            CompanyDetails = "Short 123", // 9 characters - violates the MinimumLength(10) rule
            VerificationDocumentsUrl = "https://gateway.com/doc.pdf"
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CompanyDetails)
              .WithErrorMessage("Company details must be at least 10 characters to be considered viable.");
    }

    /// <summary>
    /// Evaluates upper-bound character allocations to safely guard database schemas against massive text entries.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WhenCompanyDetailsExceedMaximumLimit_ShouldFailWithLengthExceededMessage()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var cancellationToken = TestContext.Current.CancellationToken;

        var provider = new Provider(providerId, "Quantum Industries LLC");
        var mockDbSet = new List<Provider> { provider }.BuildMockDbSet();
        _mockContext.Setup(x => x.Providers).Returns(mockDbSet.Object);

        var longDetailsText = new string('A', 2001); // 2001 characters - violates the MaximumLength(2000) rule

        var command = new SubmitProviderApplicationCommand
        {
            ProviderId = providerId,
            CompanyDetails = longDetailsText,
            VerificationDocumentsUrl = "https://gateway.com/doc.pdf"
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CompanyDetails)
              .WithErrorMessage("Company details cannot exceed 2000 characters.");
    }

    /// <summary>
    /// Verifies precise boundary compliance behavior exactly at the edge thresholds of the validation rules.
    /// </summary>
    [Theory]
    [InlineData(10)]
    [InlineData(2000)]
    public async Task ValidateAsync_WhenCompanyDetailsAreExactlyAtLengthBoundaries_ShouldPassValidation(int boundaryLength)
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var cancellationToken = TestContext.Current.CancellationToken;

        var provider = new Provider(providerId, "Quantum Industries LLC");
        var mockDbSet = new List<Provider> { provider }.BuildMockDbSet();
        _mockContext.Setup(x => x.Providers).Returns(mockDbSet.Object);

        var boundaryText = new string('B', boundaryLength);

        var command = new SubmitProviderApplicationCommand
        {
            ProviderId = providerId,
            CompanyDetails = boundaryText,
            VerificationDocumentsUrl = "https://gateway.com/doc.pdf"
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CompanyDetails);
    }

    /// <summary>
    /// Verifies missing and unpopulated compliance record reference addresses fail validation.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ValidateAsync_WhenVerificationDocumentsUrlIsMissing_ShouldFailWithRequiredMessage(string? invalidUrl)
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var cancellationToken = TestContext.Current.CancellationToken;

        var provider = new Provider(providerId, "Alpha Labs Corp");
        var mockDbSet = new List<Provider> { provider }.BuildMockDbSet();
        _mockContext.Setup(x => x.Providers).Returns(mockDbSet.Object);

        var command = new SubmitProviderApplicationCommand
        {
            ProviderId = providerId,
            CompanyDetails = "Valid description statement exceeding length requirements.",
            VerificationDocumentsUrl = invalidUrl!
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.VerificationDocumentsUrl)
              .WithErrorMessage("Verification documents URL is required.");
    }

    /// <summary>
    /// Ensures secure infrastructure compliance constraints are met by enforcing valid http or https schema components.
    /// </summary>
    [Theory]
    [InlineData("ftp://insecure-server.org/credentials.zip")]
    [InlineData("file:///C:/LocalDocuments/business_license.pdf")]
    [InlineData("just-a-plain-text-filename.docx")]
    public async Task ValidateAsync_WhenVerificationDocumentsUrlHasInvalidProtocolPrefix_ShouldFailWithProtocolMessage(string? malformedUrl)
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var cancellationToken = TestContext.Current.CancellationToken;

        var provider = new Provider(providerId, "Alpha Labs Corp");
        var mockDbSet = new List<Provider> { provider }.BuildMockDbSet();
        _mockContext.Setup(x => x.Providers).Returns(mockDbSet.Object);

        var command = new SubmitProviderApplicationCommand
        {
            ProviderId = providerId,
            CompanyDetails = "Valid description statement exceeding length requirements.",
            VerificationDocumentsUrl = malformedUrl!
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.VerificationDocumentsUrl)
              .WithErrorMessage("Verification documents URL must start with a valid http:// or https:// protocol prefix.");
    }

    /// <summary>
    /// Ensures document link strings cannot violate storage parameters by exceeding the 500 character rule limit.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WhenVerificationDocumentsUrlExceedsMaximumLimit_ShouldFailWithLengthExceededMessage()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var cancellationToken = TestContext.Current.CancellationToken;

        var provider = new Provider(providerId, "Alpha Labs Corp");
        var mockDbSet = new List<Provider> { provider }.BuildMockDbSet();
        _mockContext.Setup(x => x.Providers).Returns(mockDbSet.Object);

        var excessiveUrl = "https://secure-vault.gateway.edu/compliance/audit/onboarding/documents/" + new string('x', 450) + ".pdf";

        var command = new SubmitProviderApplicationCommand
        {
            ProviderId = providerId,
            CompanyDetails = "Valid description statement exceeding length requirements.",
            VerificationDocumentsUrl = excessiveUrl
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.VerificationDocumentsUrl)
              .WithErrorMessage("Verification documents URL cannot exceed 500 characters.");
    }
}