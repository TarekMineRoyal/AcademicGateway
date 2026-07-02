using AcademicGateway.Application.Common.Interfaces;
using Application.Features.ProviderApplications.Commands.SubmitProviderApplication;
using Domain.Providers;
using Domain.Providers.Enums;
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

namespace AcademicGateway.Application.UnitTests.Features.Providers.Commands;

public class SubmitProviderApplicationCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly SubmitProviderApplicationCommandHandler _handler;

    public SubmitProviderApplicationCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _handler = new SubmitProviderApplicationCommandHandler(_mockContext.Object);
    }

    [Fact]
    public async Task Handle_ValidUnverifiedProvider_ShouldCreateApplicationAndReturnId()
    {
        // Arrange
        var targetProviderId = "auth0|provider_corporate_clean";
        var command = new SubmitProviderApplicationCommand
        {
            ProviderId = targetProviderId,
            CompanyDetails = "A fully compliant educational partner looking to host technical skill tracks.",
            VerificationDocumentsUrl = "https://storage.academicgateway.net/docs/compliance_v1.pdf"
        };

        var provider = CreateEntityWithPrivateConstructor<Provider>();
        SetPrivateProperty(provider, nameof(Provider.UserId), targetProviderId);
        SetPrivateProperty(provider, nameof(Provider.IsVerified), false);

        var mockProvidersSet = new List<Provider> { provider }.BuildMockDbSet();
        _mockContext.Setup(c => c.Providers).Returns(mockProvidersSet.Object);

        var mockApplicationsSet = new List<ProviderApplication>().BuildMockDbSet();
        _mockContext.Setup(c => c.ProviderApplications).Returns(mockApplicationsSet.Object);

        // Act
        var resultId = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultId.Should().NotBeEmpty();

        // FIX: Focused the verification on data payloads mapped from the command to allow the domain constructor to own its default state
        _mockContext.Verify(c => c.ProviderApplications.Add(It.Is<ProviderApplication>(a =>
            a.ProviderId == targetProviderId &&
            a.CompanyDetails == command.CompanyDetails &&
            a.VerificationDocumentsUrl == command.VerificationDocumentsUrl
        )), Times.Once);

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ProviderProfileDoesNotExist_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var command = new SubmitProviderApplicationCommand
        {
            ProviderId = "auth0|missing_provider_context",
            CompanyDetails = "Some Details",
            VerificationDocumentsUrl = "https://docs.com/doc.pdf"
        };

        var mockProvidersSet = new List<Provider>().BuildMockDbSet();
        _mockContext.Setup(c => c.Providers).Returns(mockProvidersSet.Object);

        var mockApplicationsSet = new List<ProviderApplication>().BuildMockDbSet();
        _mockContext.Setup(c => c.ProviderApplications).Returns(mockApplicationsSet.Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*was not found*");
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DuplicateSubmissionWhileApplicationIsPending_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var targetProviderId = "auth0|provider_spammer";
        var command = new SubmitProviderApplicationCommand
        {
            ProviderId = targetProviderId,
            CompanyDetails = "Submitting duplicate application tracking text.",
            VerificationDocumentsUrl = "https://storage.net/doc.pdf"
        };

        var provider = CreateEntityWithPrivateConstructor<Provider>();
        SetPrivateProperty(provider, nameof(Provider.UserId), targetProviderId);
        SetPrivateProperty(provider, nameof(Provider.IsVerified), false);

        var mockProvidersSet = new List<Provider> { provider }.BuildMockDbSet();
        _mockContext.Setup(c => c.Providers).Returns(mockProvidersSet.Object);

        var existingApplication = new ProviderApplication(targetProviderId, "Original details", "https://docs.com/original.pdf");
        SetPrivateProperty(existingApplication, nameof(ProviderApplication.Status), ProviderApplicationStatus.PendingReview);

        var mockApplicationsSet = new List<ProviderApplication> { existingApplication }.BuildMockDbSet();
        _mockContext.Setup(c => c.ProviderApplications).Returns(mockApplicationsSet.Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already has an active onboarding application*");
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static T CreateEntityWithPrivateConstructor<T>() =>
        (T)Activator.CreateInstance(typeof(T), true)!;

    private static void SetPrivateProperty(object obj, string propertyName, object value) =>
        obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?
           .SetValue(obj, value);
}