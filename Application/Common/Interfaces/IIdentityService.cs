namespace AcademicGateway.Application.Common.Interfaces;

public interface IIdentityService
{
    // Returns the new UserId if successful, or a list of errors if it fails
    Task<(bool Succeeded, Guid UserId, IEnumerable<string> Errors)> CreateUserAsync(string userName, string email, string password);
    // Returns the JWT string if successful
    Task<string?> AuthenticateAsync(string email, string password);
}