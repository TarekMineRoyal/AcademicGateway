using AcademicGateway.Application.Features.Professors.Commands.RegisterProfessor;
using AcademicGateway.Application.Features.Professors.Queries.GetProfessorProfile;
using FluentAssertions;
using IntegrationTests.Infrastructure;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.IntegrationTests.Subdomains.Professors.Queries;

/// <summary>
/// Integration tests verifying lookups, profile data mapping accuracy, and error 
/// boundaries inside the GetProfessorProfile query pipeline handler loop.
/// </summary>
[Collection("SharedDatabase")]
public class GetProfessorProfileTests : BaseIntegrationTest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetProfessorProfileTests"/> class.
    /// </summary>
    /// <param name="factory">The centralized integration web application testing factory infrastructure context.</param>
    public GetProfessorProfileTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    /// <summary>
    /// Ensures that querying for a professor profile using an active, verified identifier 
    /// returns the appropriate structural DTO matching the persisted aggregate fields.
    /// </summary>
    [Fact]
    public async Task Should_ReturnProfessorProfile_WhenUserIdExists()
    {
        // --- 1. ARRANGE ---
        // Configure a complete registration payload meeting all mandatory structural validation rules
        var registerCommand = new RegisterProfessorCommand
        {
            Email = "profile.professor@academicgateway.com",
            Username = "profileprofessor",
            Password = "SecurePassword123!",
            FullName = "Professor Jane Doe",
            AcademicDepartment = "Data Science",
            Rank = "Associate Professor",
            MaxSupervisionCapacity = 4
        };

        // Execution provisions identity elements and returns a strongly-typed Guid identifier
        Guid professorId = await SendAsync(registerCommand);
        var query = new GetProfessorProfileQuery(professorId);

        // --- 2. ACT ---
        // Dispatch the query message through the pipeline layer
        var result = await SendAsync(query);

        // --- 3. ASSERT ---
        result.Should().NotBeNull();

        // Asserts against the returned DTO's 'Id' primary reference tracking key
        result.Id.Should().Be(professorId);

        // Asserts against the DTO's mapped 'Department' text property mapping
        result.Department.Should().Be("Data Science");
    }

    /// <summary>
    /// Ensures that dispatching a lookup query with a non-existent tracking reference 
    /// short-circuits gracefully at the data layer, throwing a <see cref="KeyNotFoundException"/>.
    /// </summary>
    [Fact]
    public async Task Should_ThrowKeyNotFoundException_WhenUserIdDoesNotExist()
    {
        // --- 1. ARRANGE ---
        // Instantiated using a clean, unmapped random Guid value to challenge lookups
        var query = new GetProfessorProfileQuery(Guid.NewGuid());

        // --- 2. ACT ---
        Func<Task> act = async () => await SendAsync(query);

        // --- 3. ASSERT ---
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}