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
/// Validates relational read-only projections, domain entity to DTO model mapping, and query exception handling bounds.
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