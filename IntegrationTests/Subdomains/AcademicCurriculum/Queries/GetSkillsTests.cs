using AcademicGateway.Application.Features.Skills.Queries.GetSkills;
using AcademicGateway.Domain.Skills;
using FluentAssertions;
using IntegrationTests.Infrastructure;
using Xunit;

namespace AcademicGateway.IntegrationTests.Subdomains.AcademicCurriculum.Queries;

/// <summary>
/// Integration tests verifying technical skill and professional competency 
/// lookup read operations within the academic curriculum subdomain.
/// </summary>
[Collection("SharedDatabase")]
public class GetSkillsTests : BaseIntegrationTest
{
    public GetSkillsTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    /// <summary>
    /// Ensures that dispatching a <see cref="GetSkillsQuery"/> through the application pipeline 
    /// returns all seeded professional competencies and technical capabilities accurately.
    /// </summary>
    [Fact]
    public async Task Should_ReturnAllSeededSkills()
    {
        // --- 1. ARRANGE ---
        // Instantiate and seed professional competencies using rich domain constructor patterns
        var skill1 = new Skill("Docker");
        var skill2 = new Skill("PostgreSQL");
        var skill3 = new Skill("ASP.NET Core");

        await AddAsync(skill1);
        await AddAsync(skill2);
        await AddAsync(skill3);

        // Prepare the lookup query object
        var query = new GetSkillsQuery();

        // --- 2. ACT ---
        // Route the query through MediatR pipeline behaviors
        var result = await SendAsync(query);

        // --- 3. ASSERT ---
        result.Should().NotBeNull();

        // Assert that at least our three newly seeded records are safely present in the returned matrix
        result.Should().HaveCountGreaterThanOrEqualTo(3);

        result.Should().Contain(s => s.Id == skill1.Id && s.Name == "Docker");
        result.Should().Contain(s => s.Id == skill2.Id && s.Name == "PostgreSQL");
        result.Should().Contain(s => s.Id == skill3.Id && s.Name == "ASP.NET Core");
    }
}