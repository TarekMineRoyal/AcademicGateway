using AcademicGateway.Application.Features.TechSupportAccounts.Commands.CreateTechSupportAccount;
using AcademicGateway.Application.Features.Users.Commands.RegisterProvider;
using AcademicGateway.Application.Features.Providers.Commands.SubmitApplication;
using AcademicGateway.Application.Features.Reviewers.Commands.ReviewApplication;
using AcademicGateway.Domain.Entities;
using FluentAssertions;
using FluentValidation;
using Xunit;

namespace AcademicGateway.IntegrationTests.Features.TechSupportAccounts.Commands.CreateTechSupportAccount;

public class CreateTechSupportAccountTests : BaseIntegrationTest
{
    public CreateTechSupportAccountTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Should_ProvisionTechSupportAccount_WhenProviderIsVerifiedAndCommandIsValid()
    {
        // --- 1. ARRANGE ---
        // Step A: Register the provider (starts out unverified)
        var registerProviderCommand = new RegisterProviderCommand
        {
            Email = "verified.owner@academicgateway.com",
            Username = "verifiedowner",
            Password = "SecurePassword123!",
            OrganizationName = "Enterprise Support Corp",
            Industry = "Technology"
        };
        var providerId = await SendAsync(registerProviderCommand);

        // Step B: Seed a Reviewer using the providerId as IdentityUserId to satisfy constraints
        var reviewerId = Guid.NewGuid();
        var reviewer = new Reviewer(reviewerId, providerId, "System Administrator");
        await AddAsync(reviewer);

        // Step C: Submit an onboarding application for review
        var submitAppCommand = new SubmitProviderApplicationCommand
        {
            ProviderId = providerId,
            CompanyDetails = "Detailed credentials",
            VerificationDocumentsUrl = "https://docs.com/verify"
        };
        var applicationId = await SendAsync(submitAppCommand);

        // Step D: Approve the application to switch the provider's IsVerified status to true
        var reviewCommand = new ReviewProviderApplicationCommand
        {
            ApplicationId = applicationId,
            ReviewerIdentityUserId = providerId,
            IsApproved = true,
            RejectionReason = null
        };
        await SendAsync(reviewCommand);

        // Step E: Formulate the final tech support provisioning command
        var command = new CreateTechSupportAccountCommand
        {
            ProviderId = providerId,
            Email = "auxiliary.support@academicgateway.com",
            Password = "SecurePass123!", // Satisfies complexity rules
            FullName = "John Doe Support"
        };

        // --- 2. ACT ---
        var techAccountId = await SendAsync(command);

        // --- 3. ASSERT ---
        techAccountId.Should().NotBeEmpty();

        // Verify tracking record persistence in database
        var accountRecord = await FindAsync<TechSupportAccount>(techAccountId);
        accountRecord.Should().NotBeNull();
        accountRecord!.ProviderId.Should().Be(providerId);
        accountRecord.FullName.Should().Be("John Doe Support");
        accountRecord.IdentityUserId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Should_ThrowInvalidOperationException_WhenProviderIsNotVerified()
    {
        // --- 1. ARRANGE ---
        // Register provider but DO NOT submit or approve their verification application
        var registerProviderCommand = new RegisterProviderCommand
        {
            Email = "unverified.owner@academicgateway.com",
            Username = "unverifiedowner",
            Password = "SecurePassword123!",
            OrganizationName = "Unverified Logistics",
            Industry = "Logistics"
        };
        var providerId = await SendAsync(registerProviderCommand);

        var command = new CreateTechSupportAccountCommand
        {
            ProviderId = providerId,
            Email = "illegal.support@academicgateway.com",
            Password = "SecurePass123!",
            FullName = "Jane Doe Support"
        };

        // --- 2. ACT ---
        Func<Task> act = async () => await SendAsync(command);

        // --- 3. ASSERT ---
        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("Unverified providers are not permitted to provision technical support accounts.");
    }

    [Fact]
    public async Task Should_ThrowKeyNotFoundException_WhenProviderDoesNotExist()
    {
        // --- 1. ARRANGE ---
        var command = new CreateTechSupportAccountCommand
        {
            ProviderId = "non-existent-provider-guid-string",
            Email = "ghost.support@academicgateway.com",
            Password = "SecurePass123!",
            FullName = "Ghost User"
        };

        // --- 2. ACT ---
        Func<Task> act = async () => await SendAsync(command);

        // --- 3. ASSERT ---
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Should_ThrowValidationException_WhenPasswordDoesNotMeetComplexity()
    {
        // --- 1. ARRANGE ---
        var command = new CreateTechSupportAccountCommand
        {
            ProviderId = "valid-format-id",
            Email = "badpass.support@academicgateway.com",
            Password = "simple", // Fails minimum length, uppercase, numeric, and special char rules
            FullName = "Validation Tester"
        };

        // --- 2. ACT ---
        Func<Task> act = async () => await SendAsync(command);

        // --- 3. ASSERT ---
        await act.Should().ThrowAsync<ValidationException>();
    }
}