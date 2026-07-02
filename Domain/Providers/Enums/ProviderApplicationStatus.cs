namespace AcademicGateway.Domain.Providers.Enums;

/// <summary>
/// Defines the specific state of a provider application within the onboarding evaluation pipeline.
/// </summary>
public enum ProviderApplicationStatus
{
    /// <summary>
    /// The application is in a preliminary state and is still being compiled by the provider.
    /// </summary>
    Draft = 1,

    /// <summary>
    /// The application has been submitted for verification and is awaiting evaluation by a reviewer.
    /// </summary>
    PendingReview = 2,

    /// <summary>
    /// The application data and documentation have been validated and approved.
    /// </summary>
    Approved = 3,

    /// <summary>
    /// The application has been rejected due to compliance gaps, missing data, or invalid documentation.
    /// </summary>
    Rejected = 4
}