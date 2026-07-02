using AcademicGateway.Application.Features.Curriculum.Queries.GetMajorsWithSpecialties;
using AcademicGateway.Domain.Curriculum;
using FluentAssertions;
using IntegrationTests.Infrastructure;
using Xunit;

namespace AcademicGateway.IntegrationTests.Subdomains.AcademicCurriculum.Queries;

/// <summary>
/// Integration tests verifying academic major and nested sub-specialty curriculum
/// lookup read operations within the academic curriculum subdomain.
/// </summary>
[Collection("SharedDatabase")]
public class GetMajorsWithSpecialtiesTests : BaseIntegrationTest
{
    public GetMajorsWithSpecialtiesTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    /// <summary>
    /// Ensures that dispatching a <see cref="GetMajorsWithSpecialtiesQuery"/> through the application pipeline
    /// accurately pulls all active majors mapped with their hierarchical collections of nested specialties.
    /// </summary>
    [Fact]
    public async Task Should_ReturnMajorsWithTheirNestedSpecialties()
    {
        // --- 1. ARRANGE ---
        // Instantiate the primary aggregate root using proper encapsulated domain rules
        var major = new Major("Engineering");

        // Provision nested child specialties strictly through the parent aggregate method vector
        major.AddSpecialty("Mechanical Engineering");
        major.AddSpecialty("Electrical Engineering");

        // Persist the entire aggregate root graph cleanly to the relational database store
        await AddAsync(major);

        // Dynamically extract the generated tracking identifiers out of the aggregate read-only collection
        var specialty1 = major.Specialties.Single(s => s.Name == "Mechanical Engineering");
        var specialty2 = major.Specialties.Single(s => s.Name == "Electrical Engineering");

        // Build the target CQRS lookup query
        var query = new GetMajorsWithSpecialtiesQuery();

        // --- 2. ACT ---
        // Route the query straight through MediatR pipeline behaviors
        var result = await SendAsync(query);

        // --- 3. ASSERT ---
        result.Should().NotBeNull();

        // Isolate our target major record out of the broader returned master curriculum listing
        var returnedMajor = result.FirstOrDefault(m => m.Id == major.Id);
        returnedMajor.Should().NotBeNull();
        returnedMajor!.Name.Should().Be("Engineering");

        // Verify the nested hierarchical collection mapped and converted correctly out to the DTO layer
        returnedMajor.Specialties.Should().HaveCount(2);
        returnedMajor.Specialties.Should().Contain(s => s.Id == specialty1.Id && s.Name == "Mechanical Engineering");
        returnedMajor.Specialties.Should().Contain(s => s.Id == specialty2.Id && s.Name == "Electrical Engineering");
    }
}