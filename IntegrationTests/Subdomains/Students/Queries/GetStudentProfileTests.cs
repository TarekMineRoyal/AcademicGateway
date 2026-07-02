using AcademicGateway.Application.Features.Students.Commands.RegisterStudent;
using AcademicGateway.Application.Features.Students.Queries.GetStudentProfile;
using AcademicGateway.Domain.Curriculum;
using FluentAssertions;
using IntegrationTests.Infrastructure;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.IntegrationTests.Subdomains.Students.Queries;

/// <summary>
/// Integration tests verifying lookups, profile data mapping accuracy, and error 
/// boundaries inside the GetStudentProfile query pipeline handler loop.
/// </summary>
[Collection("SharedDatabase")]
public class GetStudentProfileTests : BaseIntegrationTest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetStudentProfileTests"/> class.
    /// </summary>
    /// <param name="factory">The centralized integration web application testing factory infrastructure context.</param>
    public GetStudentProfileTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    /// <summary>
    /// Ensures that querying for a student profile using an active, verified identifier 
    /// returns the appropriate structural DTO matching the persisted aggregate fields.
    /// </summary>
    [Fact]
    public async Task Should_ReturnStudentProfile_WhenUserIdExists()
    {
        // --- 1. ARRANGE ---
        // Seed a rich domain dependency major lookup entity 
        var major = new Major("Computer Science");
        await AddAsync(major);

        // Configure a complete registration payload meeting all mandatory structural validation rules
        var registerCommand = new RegisterStudentCommand
        {
            Email = "profile.student@academicgateway.com",
            Username = "profilestudent",
            Password = "SecurePassword123!",
            FullName = "Jane Doe", // <-- Added property here to pass validation
            GraduationYear = 2026,
            MajorIds = new List<Guid> { major.Id }
        };

        // Execution provisions identity elements and returns a strongly-typed Guid identifier
        Guid studentId = await SendAsync(registerCommand);
        var query = new GetStudentProfileQuery(studentId);

        // --- 2. ACT ---
        // Dispatch the query message through the pipeline layer
        var result = await SendAsync(query);

        // --- 3. ASSERT ---
        result.Should().NotBeNull();

        // Asserts against the returned DTO's 'Id' primary reference tracking key
        result.Id.Should().Be(studentId);
        result.FullName.Should().Be("Jane Doe");
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
        var query = new GetStudentProfileQuery(Guid.NewGuid());

        // --- 2. ACT ---
        Func<Task> act = async () => await SendAsync(query);

        // --- 3. ASSERT ---
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}