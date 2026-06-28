using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.Users.Queries.GetProviderProfile;
using AcademicGateway.Domain.Entities;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.Users.Queries.GetProviderProfile;

public class GetProviderProfileQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly GetProviderProfileQueryHandler _handler;

    public GetProviderProfileQueryHandlerTests()
    {
        _dbContextMock = new Mock<IApplicationDbContext>();
        _handler = new GetProviderProfileQueryHandler(_dbContextMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Profile_When_Provider_Exists()
    {
        // Arrange
        var targetUserId = "provider-456";
        var providers = new List<Provider>
        {
            new Provider
            {
                UserId = targetUserId,
                OrganizationName = "Innovate LLC",
                Industry = "BioTech",
                WebsiteUrl = "https://innovate.io",
                IsVerified = true
            }
        };

        var mockDbSet = providers.BuildMockDbSet();
        _dbContextMock.Setup(db => db.Providers).Returns(mockDbSet.Object);

        var query = new GetProviderProfileQuery(targetUserId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(targetUserId);
        result.OrganizationName.Should().Be("Innovate LLC");
        result.Industry.Should().Be("BioTech");
        result.WebsiteUrl.Should().Be("https://innovate.io");
        result.IsVerified.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_Throw_KeyNotFoundException_When_Provider_Does_Not_Exist()
    {
        // Arrange
        var mockDbSet = new List<Provider>().BuildMockDbSet();
        _dbContextMock.Setup(db => db.Providers).Returns(mockDbSet.Object);

        var query = new GetProviderProfileQuery("missing-id");

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*User ID 'missing-id' was not found.*");
    }
}