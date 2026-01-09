using DataverseDoc.Core.Configuration;

namespace DataverseDoc.Dataverse;

/// <summary>
/// Service for authenticating to Dataverse.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Gets an access token for the Dataverse environment.
    /// </summary>
    /// <param name="options">Connection options containing authentication details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The access token.</returns>
    Task<string> GetAccessTokenAsync(DataverseConnectionOptions options, CancellationToken cancellationToken = default);
}
