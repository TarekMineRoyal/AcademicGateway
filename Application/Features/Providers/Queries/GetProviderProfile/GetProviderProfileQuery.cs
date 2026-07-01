using AcademicGateway.Application.Features.Providers.Queries.GetProviderProfile;
using MediatR;
using System;

namespace AcademicGateway.Application.Features.Providers.Queries.GetProviderProfile;

/// <summary>
/// CQRS Query to retrieve the comprehensive profile view, verified operational metrics, 
/// and tracking configurations for a specific corporate industry provider.
/// </summary>
/// <param name="ProviderId">The unique global identifier of the provider profile, mapping 1:1 to their security identity context.</param>
public record GetProviderProfileQuery(Guid ProviderId) : IRequest<ProviderProfileDto>;