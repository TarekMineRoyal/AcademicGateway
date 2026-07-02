using AcademicGateway.Application.Features.TechSupportAccounts.Commands.CreateTechSupportAccount;
using AcademicGateway.Application.Features.Providers.Commands.RegisterProvider;
using AcademicGateway.Application.Features.ProviderApplications.Commands.SubmitProviderApplication;
using AcademicGateway.Application.Features.ProviderApplications.Commands.ReviewProviderApplication;
using AcademicGateway.Domain.Providers;
using AcademicGateway.Domain.Providers.Exceptions;
using AcademicGateway.Infrastructure.Identity;
using FluentAssertions;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using IntegrationTests.Infrastructure;

namespace IntegrationTests.Subdomains.TechSupportAccounts.Commands;

/// <summary>
/// Integration tests verifying validation rules, provider state preconditions, and 
/// account provisioning invariants handled by the create tech support account command pipeline.
/// </summary>
[Collection("SharedDatabase")]
public class CreateTechSupportAccountTests : BaseIntegrationTest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateTechSupportAccountTests"/> class.
    /// </summary>
    /// <param name="factory">The centralized integration web application testing factory infrastructure context.</param>
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

        // Step B: Seed underlying ApplicationUser security credentials for the platform reviewer
        var reviewerUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "admin.reviewer@academicgateway.com",
            Email = "admin.reviewer@academicgateway.com"
        };
        await AddAsync(reviewerUser);

        // Step C: Seed active institutional reviewer profile context using the generated Identity ID context
        var reviewer = new AcademicGateway.Domain.Reviewers.Reviewer(reviewerUser.Id, "System Administrator");
        await AddAsync(reviewer);

        // Step D: Submit an onboarding application for review to trigger proper verification steps
        var submitAppCommand = new SubmitProviderApplicationCommand
        {
            ProviderId = providerId,
            CompanyDetails = "Detailed corporate infrastructure credentials",
            VerificationDocumentsUrl = "https://docs.com/verify-enterprise.pdf"
        };
        Guid applicationId = await SendAsync(submitAppCommand);

        // Step E: Approve the application through the official command pipeline to naturally verify the provider
        var reviewCommand = new ReviewProviderApplicationCommand
        {
            ApplicationId = applicationId,
            ReviewerId = reviewer.Id,
            IsApproved = true,
            RejectionReason = null
        };
        await SendAsync(reviewCommand);

        // Step F: Formulate the final technical support provisioning command containing valid structural properties
        var command = new CreateTechSupportAccountCommand
        {
            ProviderId = providerId,
            Email = "auxiliary.support@academicgateway.com",
            Password = "SecurePass123!",
            StaffNumber = "EMP-SUP-001",
            SupportTier = "Tier 2 Helpdesk"
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

        // Include valid staff details to clear fluent validation and hit the inner handler guard clauses
        var command = new CreateTechSupportAccountCommand
        {
            ProviderId = providerId,
            Email = "illegal.support@academicgateway.com",
            Password = "SecurePass123!",
            StaffNumber = "EMP-SUP-002",
            SupportTier = "Tier 1 Support"
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
        // Include valid staff details to clear fluent validation checkpoints
        var command = new CreateTechSupportAccountCommand
        {
            ProviderId = Guid.NewGuid(), // Simulates a missing company reference context
            Email = "ghost.support@academicgateway.com",
            Password = "SecurePass123!",
            StaffNumber = "EMP-SUP-003",
            SupportTier = "Tier 3 Engineer"
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
        // Supply valid prerequisites to explicitly isolate the target password complexity rule failure
        var command = new CreateTechSupportAccountCommand
        {
            ProviderId = Guid.NewGuid(),
            Email = "badpass.support@academicgateway.com",
            Password = "simple", // Fails uppercase, numeric, and special character criteria rules
            StaffNumber = "EMP-SUP-004",
            SupportTier = "Tier 1 Support"
        };

        // --- 2. ACT ---
        Func<Task> act = async () => await SendAsync(command);

        // --- 3. ASSERT ---
        await act.Should().ThrowAsync<ValidationException>();
    }
}