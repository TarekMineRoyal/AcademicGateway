using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.Reviewers.Commands.ReviewApplication;
using AcademicGateway.Domain.Entities;
using AcademicGateway.Domain.Enums;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.Reviewers.Commands;

public class ReviewProviderApplicationCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly ReviewProviderApplicationCommandHandler _handler;

    public ReviewProviderApplicationCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _handler = new ReviewProviderApplicationCommandHandler(_mockContext.Object);
    }

    [Fact]
    public async Task Handle_ValidApproval_ShouldApproveApplicationAndVerifyProvider()
    {
        // Arrange
        var targetApplicationId = Guid.NewGuid();
        var targetProviderId = "auth0|provider_to_verify";
        var mockReviewerIdentityId = "auth0|admin_reviewer_1";
        var resolvedReviewerGuid = Guid.NewGuid();

        var command = new ReviewProviderApplicationCommand
        {
            ApplicationId = targetApplicationId,
            ReviewerIdentityUserId = mockReviewerIdentityId,
            IsApproved = true,
            RejectionReason = null
        };

        var application = new ProviderApplication(targetProviderId, "Details", "https://docs.com/1.pdf");
        SetPrivateProperty(application, nameof(ProviderApplication.Id), targetApplicationId);
        // FIX: Initialize the application in PendingReview status so the domain guard allows compliance execution
        SetPrivateProperty(application, nameof(ProviderApplication.Status), ProviderApplicationStatus.PendingReview);

        var provider = CreateEntityWithPrivateConstructor<Provider>();
        SetPrivateProperty(provider, nameof(Provider.UserId), targetProviderId);
        SetPrivateProperty(provider, nameof(Provider.IsVerified), false);

        var reviewer = CreateEntityWithPrivateConstructor<Reviewer>();
        SetPrivateProperty(reviewer, nameof(Reviewer.Id), resolvedReviewerGuid);
        SetPrivateProperty(reviewer, nameof(Reviewer.IdentityUserId), mockReviewerIdentityId);

        var mockAppSet = new List<ProviderApplication> { application }.BuildMockDbSet();
        var mockProviderSet = new List<Provider> { provider }.BuildMockDbSet();
        var mockReviewerSet = new List<Reviewer> { reviewer }.BuildMockDbSet();

        _mockContext.Setup(c => c.ProviderApplications).Returns(mockAppSet.Object);
        _mockContext.Setup(c => c.Providers).Returns(mockProviderSet.Object);
        _mockContext.Setup(c => c.Reviewers).Returns(mockReviewerSet.Object);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        application.Status.Should().Be(ProviderApplicationStatus.Approved);
        provider.IsVerified.Should().BeTrue();
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidRejection_ShouldRejectApplicationAndLeaveProviderUnverified()
    {
        // Arrange
        var targetApplicationId = Guid.NewGuid();
        var targetProviderId = "auth0|provider_to_reject";
        var mockReviewerIdentityId = "auth0|admin_reviewer_2";
        var resolvedReviewerGuid = Guid.NewGuid();
        var standardRejectionReason = "The submitted document URL is malformed or inaccessible.";

        var command = new ReviewProviderApplicationCommand
        {
            ApplicationId = targetApplicationId,
            ReviewerIdentityUserId = mockReviewerIdentityId,
            IsApproved = false,
            RejectionReason = standardRejectionReason
        };

        var application = new ProviderApplication(targetProviderId, "Details", "https://docs.com/bad.pdf");
        SetPrivateProperty(application, nameof(ProviderApplication.Id), targetApplicationId);
        // FIX: Force target application to PendingReview status before calling rejection pathways
        SetPrivateProperty(application, nameof(ProviderApplication.Status), ProviderApplicationStatus.PendingReview);

        var provider = CreateEntityWithPrivateConstructor<Provider>();
        SetPrivateProperty(provider, nameof(Provider.UserId), targetProviderId);
        SetPrivateProperty(provider, nameof(Provider.IsVerified), false);

        var reviewer = CreateEntityWithPrivateConstructor<Reviewer>();
        SetPrivateProperty(reviewer, nameof(Reviewer.Id), resolvedReviewerGuid);
        SetPrivateProperty(reviewer, nameof(Reviewer.IdentityUserId), mockReviewerIdentityId);

        var mockAppSet = new List<ProviderApplication> { application }.BuildMockDbSet();
        var mockProviderSet = new List<Provider> { provider }.BuildMockDbSet();
        var mockReviewerSet = new List<Reviewer> { reviewer }.BuildMockDbSet();

        _mockContext.Setup(c => c.ProviderApplications).Returns(mockAppSet.Object);
        _mockContext.Setup(c => c.Providers).Returns(mockProviderSet.Object);
        _mockContext.Setup(c => c.Reviewers).Returns(mockReviewerSet.Object);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        application.Status.Should().Be(ProviderApplicationStatus.Rejected);
        application.RejectionReason.Should().Be(standardRejectionReason);
        provider.IsVerified.Should().BeFalse();

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ApplicationNotFound_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var command = new ReviewProviderApplicationCommand
        {
            ApplicationId = Guid.NewGuid(),
            ReviewerIdentityUserId = "auth0|any_reviewer",
            IsApproved = true
        };

        var mockAppSet = new List<ProviderApplication>().BuildMockDbSet();
        _mockContext.Setup(c => c.ProviderApplications).Returns(mockAppSet.Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*was not found*");
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReviewerProfileNotFound_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var targetApplicationId = Guid.NewGuid();
        var command = new ReviewProviderApplicationCommand
        {
            ApplicationId = targetApplicationId,
            ReviewerIdentityUserId = "auth0|unregistered_admin",
            IsApproved = true
        };

        var application = new ProviderApplication("provider_1", "Details", "https://docs.com/1.pdf");
        SetPrivateProperty(application, nameof(ProviderApplication.Id), targetApplicationId);

        var mockAppSet = new List<ProviderApplication> { application }.BuildMockDbSet();
        var mockReviewerSet = new List<Reviewer>().BuildMockDbSet();

        _mockContext.Setup(c => c.ProviderApplications).Returns(mockAppSet.Object);
        _mockContext.Setup(c => c.Reviewers).Returns(mockReviewerSet.Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*Reviewer domain profile*was not found*");
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static T CreateEntityWithPrivateConstructor<T>() =>
        (T)Activator.CreateInstance(typeof(T), true)!;

    private static void SetPrivateProperty(object obj, string propertyName, object value) =>
        obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?
           .SetValue(obj, value);
}