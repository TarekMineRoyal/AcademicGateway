using AcademicGateway.Application.Features.Professors.Commands.RegisterProfessor;
using AcademicGateway.Application.Features.Professors.Queries.GetProfessorProfile;
using FluentAssertions;
using IntegrationTests.Infrastructure;
using Xunit;

namespace AcademicGateway.IntegrationTests.Subdomains.Professors.Queries;

/// <summary>
/// Integration tests verifying lookups, profile data mapping accuracy, and error 
/// boundaries inside the GetProfessorProfile query pipeline handler loop.
/// </summary>
[Collection("SharedDatabase")]
public class GetProfessorProfileTests : BaseIntegrationTest
{
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
        var registerCommand = new RegisterProfessorCommand
        {
            Email = "profile.professor@academicgateway.com",
            Username = "profileprofessor",
            Password = "SecurePassword123!",
            AcademicDepartment = "Data Science"
        };

        // Execution returns a strongly-typed Guid key identifier
        Guid professorId = await SendAsync(registerCommand);
        var query = new GetProfessorProfileQuery(professorId);

        // --- 2. ACT ---
        var result = await SendAsync(query);

        // --- 3. ASSERT ---
        result.Should().NotBeNull();

        // Asserts against the DTO's 'Id' property
        result.Id.Should().Be(professorId);

        // Asserts against the DTO's 'AcademicDepartment' property
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