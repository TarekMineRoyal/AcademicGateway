namespace AcademicGateway.Api.Common.Models;

/// <summary>
/// Represents a standardized presentation-layer payload contract returned by write operations 
/// following the successful creation or resubmission of a system resource.
/// </summary>
/// <param name="Id">The unique global surrogate key generated for the newly created resource aggregate root.</param>
public record ResourceCreatedResponse(Guid Id);