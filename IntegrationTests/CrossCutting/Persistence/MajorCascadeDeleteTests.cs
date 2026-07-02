using AcademicGateway.Domain.Curriculum;
using FluentAssertions;
using Infrastructure.Persistence.Context;
using IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AcademicGateway.IntegrationTests.CrossCutting.Persistence;

/// <summary>
/// Integration tests verifying database persistence constraints and foreign key cascade rules
/// within the academic curriculum subdomain boundaries.
/// </summary>
[Collection("SharedDatabase")]
public class MajorCascadeDeleteTests : BaseIntegrationTest
{
    private readonly IServiceScopeFactory _scopeFactory;

    public MajorCascadeDeleteTests(CustomWebApplicationFactory factory) : base(factory)
    {
        _scopeFactory = factory.Services.GetRequiredService<IServiceScopeFactory>();
    }

    /// <summary>
    /// Ensures that removing a <see cref="Major"/> aggregate root from the database context
    /// automatically cascades to delete all nested and associated child <see cref="Specialty"/> records.
    /// </summary>
    [Fact]
    public async Task DeletingMajor_ShouldCascadeDelete_ItsAssociatedSpecialties()
    {
        // --- 1. ARRANGE ---
        // Instantiate the aggregate root using proper encapsulated domain rules
        var major = new Major("Electrical Engineering");

        // Append sub-track specialties strictly via the parent aggregate method vector
        major.AddSpecialty("Telecommunications");
        major.AddSpecialty("Embedded Systems");

        // Persist the entire aggregate root graph cleanly to the database store
        await AddAsync(major);

        // Safely extract the generated child tracking details from the read-only collection
        var specialty1 = major.Specialties.Single(s => s.Name == "Telecommunications");
        var specialty2 = major.Specialties.Single(s => s.Name == "Embedded Systems");

        // Pre-assertion: Verify the major and its dependencies are actively tracked in the store
        (await FindAsync<Major>(major.Id)).Should().NotBeNull();
        (await FindAsync<Specialty>(specialty1.Id)).Should().NotBeNull();
        (await FindAsync<Specialty>(specialty2.Id)).Should().NotBeNull();

        // --- 2. ACT ---
        // Remove the major aggregate root through an isolated database context scope
        using (var scope = _scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var majorEntity = await context.Majors.FindAsync(new object[] { major.Id }, TestContext.Current.CancellationToken);
            majorEntity.Should().NotBeNull();

            context.Majors.Remove(majorEntity!);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        // --- 3. ASSERT ---
        // Verify the primary major record was completely removed
        (await FindAsync<Major>(major.Id)).Should().BeNull();

        // Verify relational cascade configurations successfully wiped out both dependent specialty sub-tracks
        (await FindAsync<Specialty>(specialty1.Id)).Should().BeNull();
        (await FindAsync<Specialty>(specialty2.Id)).Should().BeNull();
    }
}