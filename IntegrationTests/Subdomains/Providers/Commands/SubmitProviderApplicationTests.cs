using AcademicGateway.Application.Features.ProviderApplications.Commands.SubmitProviderApplication;
using AcademicGateway.Application.Features.Providers.Commands.RegisterProvider;
using FluentAssertions;
using FluentValidation;
using IntegrationTests.Infrastructure;
using Xunit;

namespace AcademicGateway.IntegrationTests.Subdomains.Providers.Commands;

/// <summary>
/// Integration tests verifying pipeline validations, duplicate handling constraints,
/// and application boundary guards for the provider enrollment submission command pipeline.
/// </summary>
[Collection("SharedDatabase")]
public class SubmitProviderApplicationTests : BaseIntegrationTest
{
    public SubmitProviderApplicationTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    /// <summary>
    /// Ensures that attempting to submit an onboarding application for an arbitrary, non-existent 
    /// provider identifier is caught by fluid validation rules, throwing a <see cref="ValidationException"/>.
    /// </summary>
    [Fact]
    public async Task Should_ThrowValidationException_WhenSubmittingForNonExistentProvider()
    {
        // --- 1. ARRANGE ---
        // Prepare a submission command containing a completely random tracking Guid identifier
        var command = new SubmitProviderApplicationCommand
        {
            ProviderId = Guid.NewGuid(), // Simulates a missing aggregate profile reference context
            CompanyDetails = "Ghost Company LLC",
            VerificationDocumentsUrl = "https://verify.com/ghost.pdf"
        };

        // --- 2. ACT ---
        // Capture pipeline dispatch execution logic inside an asynchronous delegate
        Func<Task> act = async () => await SendAsync(command);

        // --- 3. ASSERT ---
        // Verify that the behavioral pipeline interceptors block execution and return an explicit relational error
        await act.Should().ThrowAsync<ValidationException>()
            .Where(ex => ex.Errors.Any(e => e.ErrorMessage.Contains("specified Provider profile does not exist")));
    }

    /// <summary>
    /// Ensures that a provider aggregate profile that already possesses an active onboarding tracking record 
    /// is strictly blocked from executing a duplicate submission loop, throwing an <see cref="InvalidOperationException"/>.
    /// </summary>
    [Fact]
    public async Task Should_ThrowInvalidOperationException_WhenProviderAttemptsDuplicateSubmission()
    {
        // --- 1. ARRANGE ---
        // Register a corporate partner profile cleanly using the system command pipeline
        var registerProviderCommand = new RegisterProviderCommand
        {
            Email = "duplicate.submission@academicgateway.com",
            Username = "dupsubmissions",
            Password = "SecurePassword123!",
            CompanyName = "Duplicate Testing Inc",
            CompanyDescription = "Compliance and Concurrency Structural Audits",
            WebsiteUrl = "https://verify-concurrency.com"
        };
        Guid providerId = await SendAsync(registerProviderCommand);

        // Construct a standard validation application command payload matching the verified profile identity
        var command = new SubmitProviderApplicationCommand
        {
            ProviderId = providerId,
            CompanyDetails = "Initial Valid Corporate Metadata Dossier Submittal",
            VerificationDocumentsUrl = "https://verify.com/docs.pdf"
        };

        // Dispatch the first submission transaction successfully to shift system state out of raw default standings
        await SendAsync(command);

        // --- 2. ACT & 3. ASSERT ---
        // Attempt to execute the exact same command payload a second time to trigger a concurrency violation
        Func<Task> act = async () => await SendAsync(command);

        // Verify the domain layer intercepts the duplicate submission attempt, safeguarding entity integrity
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*already has an active onboarding application*");
    }
}