using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.Providers.Queries.GetProviderProfile;
using AcademicGateway.Domain.Providers;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.Providers.Queries.GetProviderProfile;

/// <summary>
/// Contains isolated unit verification routines for the <see cref="GetProviderProfileQueryHandler"/>.
/// Validates relational read-only select projections, domain entity to DTO model mapping, 
/// multiple dataset row isolation, and query exception handling bounds.
/// </summary>
public class GetProviderProfileQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly GetProviderProfileQueryHandler _handler;

    /// <summary>
    /// Initializes a pristine instance of the test class with isolated database mock mappings.
    /// </summary>
    public GetProviderProfileQueryHandlerTests()
    {
        _dbContextMock = new Mock<IApplicationDbContext>();
        _handler = new GetProviderProfileQueryHandler(_dbContextMock.Object);
    }

    /// <summary>
    /// Assures that executing a lookup query for an existing provider profile accurately transforms
    /// data fields into a projected <see cref="ProviderProfileDto"/> with all metadata and compliance flags.
    /// </summary>
    [Fact]
    public async Task Handle_GivenExistingProviderId_ShouldReturnProjectedProfileDto()
    {
        // Arrange
        var targetProviderId = Guid.NewGuid();

        // Best Practice: Use native constructors and aggregate routines to populate corporate focus boundaries.
        var provider = new Provider(targetProviderId, "Innovate LLC");
        provider.UpdateProfileDetails(
            description: "A specialized biotechnology firm designing automated cellular sorting matrix architectures.",
            websiteUrl: "https://innovate.io");
        provider.VerifyProfile(); // Transitions compliance standing to IsVerified = true

        var providersList = new List<Provider> { provider };
        var mockDbSet = providersList.BuildMockDbSet();
        _dbContextMock.Setup(db => db.Providers).Returns(mockDbSet.Object);

        var query = new GetProviderProfileQuery(targetProviderId);

        // Act
        // Best Practice (xUnit1051): Pass TestContext.Current.CancellationToken to support responsive test run cancellations.
        var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(targetProviderId);
        result.CompanyName.Should().Be("Innovate LLC");
        result.CompanyDescription.Should().Be("A specialized biotechnology firm designing automated cellular sorting matrix architectures.");
        result.WebsiteUrl.Should().Be("https://innovate.io");
        result.IsVerified.Should().BeTrue();
    }

    /// <summary>
    /// Assures that when a corporate partner exists but has not yet passed verification gates,
    /// the query handler returns their projected profile with the compliance status flag accurately set to false.
    /// </summary>
    [Fact]
    public async Task Handle_GivenExistingUnverifiedProvider_ShouldReturnProjectedProfileDtoWithIsVerifiedFalse()
    {
        // Arrange
        var targetProviderId = Guid.NewGuid();

        // Context: Instantiated but DO NOT call provider.VerifyProfile(). IsVerified remains false.
        var provider = new Provider(targetProviderId, "Unverified Tech Startup");
        provider.UpdateProfileDetails("Early stage venture overview.", "https://startup.net");

        var mockDbSet = new List<Provider> { provider }.BuildMockDbSet();
        _dbContextMock.Setup(db => db.Providers).Returns(mockDbSet.Object);

        var query = new GetProviderProfileQuery(targetProviderId);

        // Act
        var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(targetProviderId);
        result.IsVerified.Should().BeFalse();
    }

    /// <summary>
    /// Assures that if a provider has a fresh profile shell without custom descriptions or site configurations,
    /// the data view projects cleanly, setting default text blocks without throwing null reference exceptions.
    /// </summary>
    [Fact]
    public async Task Handle_GivenFreshProviderWithoutProfileDetails_ShouldProjectCleanlyWithEmptyStrings()
    {
        // Arrange
        var targetProviderId = Guid.NewGuid();

        // Context: Description and Website are left as default empty strings inside the domain entity layer
        var freshProvider = new Provider(targetProviderId, "Fresh Consulting Group");

        var mockDbSet = new List<Provider> { freshProvider }.BuildMockDbSet();
        _dbContextMock.Setup(db => db.Providers).Returns(mockDbSet.Object);

        var query = new GetProviderProfileQuery(targetProviderId);

        // Act
        var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.CompanyName.Should().Be("Fresh Consulting Group");
        result.CompanyDescription.Should().Be(string.Empty);
        result.WebsiteUrl.Should().Be(string.Empty);
    }

    /// <summary>
    /// Assures that when multiple provider records reside within the database context, the query engine
    /// cleanly applies key filters to isolate and project only the requested profile instance.
    /// </summary>
    [Fact]
    public async Task Handle_GivenMultipleProvidersInDataset_ShouldIsolateCorrectTargetProfileInstance()
    {
        // Arrange
        var searchTargetId = Guid.NewGuid();
        var alternativeId = Guid.NewGuid();

        var targetProvider = new Provider(searchTargetId, "Target Corporate Entity");
        var secondaryProvider = new Provider(alternativeId, "Alternative Competitor Corp");

        var multiProviderPool = new List<Provider> { targetProvider, secondaryProvider };
        var mockDbSet = multiProviderPool.BuildMockDbSet();
        _dbContextMock.Setup(db => db.Providers).Returns(mockDbSet.Object);

        var query = new GetProviderProfileQuery(searchTargetId);

        // Act
        var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(searchTargetId);
        result.CompanyName.Should().Be("Target Corporate Entity");
    }

    /// <summary>
    /// Assures that looking up a non-existent corporate identity code stops execution pipeline routing
    /// and throws a descriptive <see cref="KeyNotFoundException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenNonExistentProviderId_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var mockDbSet = new List<Provider>().BuildMockDbSet();
        _dbContextMock.Setup(db => db.Providers).Returns(mockDbSet.Object);

        var missingId = Guid.NewGuid();
        var query = new GetProviderProfileQuery(missingId);

        // Act
        // Best Practice (xUnit1051): Wrap implementation execution with TestContext.Current.CancellationToken inside the delegate context.
        Func<Task> act = async () => await _handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*Provider profile with tracking identity '{missingId}' was not found within the institutional directory.*");
    }
}