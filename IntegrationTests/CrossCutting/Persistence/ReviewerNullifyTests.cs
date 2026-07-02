using AcademicGateway.Application.Features.Providers.Commands.RegisterProvider;
using AcademicGateway.Domain.Providers;
using AcademicGateway.Domain.Reviewers;
using AcademicGateway.Infrastructure.Persistence;
using FluentAssertions;
using IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AcademicGateway.IntegrationTests.CrossCutting.Persistence;

/// <summary>
/// Integration tests verifying foreign key nullification constraints when 
/// platform evaluators or reviewers are deleted from the system.
/// </summary>
[Collection("SharedDatabase")]
public class ReviewerNullifyTests : BaseIntegrationTest
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ReviewerNullifyTests(CustomWebApplicationFactory factory) : base(factory)
    {
        _scopeFactory = factory.Services.GetRequiredService<IServiceScopeFactory>();
    }

    /// <summary>
    /// Ensures that deleting a <see cref="Reviewer"/> safely nullifies tracking foreign keys 
    /// on associated <see cref="ProviderApplication"/> records instead of triggering destructive cascades.
    /// </summary>
    [Fact]
    public async Task DeletingReviewer_ShouldSetNull_OnTrackingForeignKeys()
    {
        // --- 1. ARRANGE ---
        // Register a corporate provider account to act as the application owner
        var providerCommand = new RegisterProviderCommand
        {
            Email = "reviewer.nullify@academicgateway.com",
            Username = "nullifyprovider",
            Password = "SecurePassword123!",
            CompanyName = "Nullify Systems",
            CompanyDescription = "External Audit Sandbox Compliance Systems",
            WebsiteUrl = "https://nullify-sec.com"
        };
        Guid providerId = await SendAsync(providerCommand);

        // Instantiate an operational Reviewer aggregate with an explicit unique identity context
        Guid reviewerId = Guid.NewGuid();
        var reviewer = new Reviewer(reviewerId, "System Auditor");
        await AddAsync(reviewer);

        // Provision an onboarding application workflow tracking instance for the provider
        var providerApp = new ProviderApplication(
            providerId: providerId,
            companyDetails: "Security Verification Paperwork & Compliance Specifications",
            verificationDocumentsUrl: "https://docs.com/sec.pdf",
            createdAt: DateTime.UtcNow
        );

        // Execute state transitions via rich domain methods to simulate an evaluation loop
        providerApp.SubmitForReview();
        providerApp.Approve(reviewerId, DateTime.UtcNow);
        await AddAsync(providerApp);

        // Pre-assertion: Verify the onboarding record is actively mapped to our evaluator
        var savedApp = await FindAsync<ProviderApplication>(providerApp.Id);
        savedApp.Should().NotBeNull();
        savedApp!.ReviewedById.Should().Be(reviewerId);

        // --- 2. ACT ---
        // Delete the auditor reviewer aggregate root from the persistence store
        using (var scope = _scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var reviewerEntity = await context.Reviewers.FindAsync(new object[] { reviewerId }, TestContext.Current.CancellationToken);
            reviewerEntity.Should().NotBeNull();

            context.Reviewers.Remove(reviewerEntity!);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        // --- 3. ASSERT ---
        // Verify the reviewer aggregate root was permanently dropped
        (await FindAsync<Reviewer>(reviewerId)).Should().BeNull();

        // Verify the application remains intact but its tracking foreign reference was safely set to null
        var updatedApp = await FindAsync<ProviderApplication>(providerApp.Id);
        updatedApp.Should().NotBeNull();
        updatedApp!.ReviewedById.Should().BeNull();
    }
}