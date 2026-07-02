using AcademicGateway.Application.Features.TechSupportAccounts.Commands.CreateTechSupportAccount;
using AcademicGateway.Application.Features.Providers.Commands.RegisterProvider;
using AcademicGateway.Application.Features.ProviderApplications.Commands.SubmitProviderApplication;
using AcademicGateway.Application.Features.ProviderApplications.Commands.ReviewProviderApplication;
using AcademicGateway.Domain.Providers;
using AcademicGateway.Domain.Providers.Exceptions;
using FluentAssertions;
using FluentValidation;
using Xunit;
using IntegrationTests.Infrastructure;

namespace AcademicGateway.IntegrationTests.Subdomains.Providers.Commands;

/// <summary>
/// Integration tests verifying validation rules, provider state preconditions, and 
/// account provisioning invariants handled by the create tech support account command pipeline.
/// </summary>
[Collection("SharedDatabase")]
public class CreateTechSupportAccountTests : BaseIntegrationTest
{
    public CreateTechSupportAccountTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    /// <summary>
    /// Ensures that a fully verified provider organization can successfully provision 
    /// a valid secondary technical support operator account.
    /// </summary>
    [Fact]
    public async Task Should_ProvisionTechSupportAccount_WhenProviderIsVerifiedAndCommandIsValid()
    {
        // --- 1. ARRANGE ---
        // Step A: Register a baseline corporate provider profile via the pipeline
        var registerProviderCommand = new RegisterProviderCommand
        {
            Email = "verified.owner@academicgateway.com",
            Username = "verifiedowner",
            Password = "SecurePassword123!",
            CompanyName = "Enterprise Support Corp",
            CompanyDescription = "Global Technical Integration Providers",
            WebsiteUrl = "https://enterprise-support.com"
        };
        Guid providerId = await SendAsync(registerProviderCommand);

        // Step B: Seed an active institutional reviewer profile context using clean aggregate constructors
        // Note: Reusing a separate ID context to ensure clean relational tracking bounds.
        var reviewer = new AcademicGateway.Domain.Reviewers.Reviewer(Guid.NewGuid(), "System Administrator");
        await AddAsync(reviewer);

        // Step C: Submit an onboarding application for review to trigger proper verification steps
        var submitAppCommand = new SubmitProviderApplicationCommand
        {
            ProviderId = providerId,
            CompanyDetails = "Detailed corporate infrastructure credentials",
            VerificationDocumentsUrl = "https://docs.com/verify-enterprise.pdf"
        };
        Guid applicationId = await SendAsync(submitAppCommand);

        // Step D: Approve the application through the official command pipeline to naturally verify the provider
        var reviewCommand = new ReviewProviderApplicationCommand
        {
            ApplicationId = applicationId,
            ReviewerId = reviewer.Id,
            IsApproved = true,
            RejectionReason = null
        };
        await SendAsync(reviewCommand);

        // Step E: Formulate the final technical support provisioning command
        var command = new CreateTechSupportAccountCommand
        {
            ProviderId = providerId,
            Email = "auxiliary.support@academicgateway.com",
            Password = "SecurePass123!"
        };

        // --- 2. ACT ---
        Guid techAccountId = await SendAsync(command);

        // --- 3. ASSERT ---
        techAccountId.Should().NotBeEmpty();

        // Verify the technical support record was properly written to the database
        var accountRecord = await FindAsync<TechSupportAccount>(techAccountId);
        accountRecord.Should().NotBeNull();
        accountRecord!.Id.Should().Be(techAccountId);
    }

    /// <summary>
    /// Ensures that an unverified corporate provider profile attempting to provision 
    /// subordinate accounts is blocked by domain guards, throwing a <see cref="ProviderNotVerifiedException"/>.
    /// </summary>
    [Fact]
    public async Task Should_ThrowProviderNotVerifiedException_WhenProviderIsNotVerified()
    {
        // --- 1. ARRANGE ---
        // Register a provider profile but leave it in its default, unverified state
        var registerProviderCommand = new RegisterProviderCommand
        {
            Email = "unverified.owner@academicgateway.com",
            Username = "unverifiedowner",
            Password = "SecurePassword123!",
            CompanyName = "Unverified Logistics",
            CompanyDescription = "Supply Chain Management Internships",
            WebsiteUrl = "https://unverified-logistics.com"
        };
        Guid providerId = await SendAsync(registerProviderCommand);

        var command = new CreateTechSupportAccountCommand
        {
            ProviderId = providerId,
            Email = "illegal.support@academicgateway.com",
            Password = "SecurePass123!"
        };

        // --- 2. ACT ---
        Func<Task> act = async () => await SendAsync(command);

        // --- 3. ASSERT ---
        // Verify that the domain layer blocks the action for unverified profiles
        await act.Should().ThrowAsync<ProviderNotVerifiedException>();
    }

    /// <summary>
    /// Ensures that executing the command with a non-existent provider tracking identifier 
    /// fails at the application boundary, throwing a <see cref="KeyNotFoundException"/>.
    /// </summary>
    [Fact]
    public async Task Should_ThrowKeyNotFoundException_WhenProviderDoesNotExist()
    {
        // --- 1. ARRANGE ---
        var command = new CreateTechSupportAccountCommand
        {
            ProviderId = Guid.NewGuid(), // Simulates a missing company reference context
            Email = "ghost.support@academicgateway.com",
            Password = "SecurePass123!"
        };

        // --- 2. ACT ---
        Func<Task> act = async () => await SendAsync(command);

        // --- 3. ASSERT ---
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    /// <summary>
    /// Ensures that input passwords failing baseline application complexity rules 
    /// are rejected early by fluent validation interceptors, throwing a <see cref="ValidationException"/>.
    /// </summary>
    [Fact]
    public async Task Should_ThrowValidationException_WhenPasswordDoesNotMeetComplexity()
    {
        // --- 1. ARRANGE ---
        var command = new CreateTechSupportAccountCommand
        {
            ProviderId = Guid.NewGuid(),
            Email = "badpass.support@academicgateway.com",
            Password = "simple", // Fails uppercase, numeric, and special character criteria rules
        };

        // --- 2. ACT ---
        Func<Task> act = async () => await SendAsync(command);

        // --- 3. ASSERT ---
        await act.Should().ThrowAsync<ValidationException>();
    }
}