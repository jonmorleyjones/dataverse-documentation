using System.Text.Json;

namespace DataverseDoc.Dataverse;

/// <summary>
/// Client for querying Dataverse Web API.
/// </summary>
public interface IDataverseClient
{
    /// <summary>
    /// Executes a Web API GET request and returns the result.
    /// </summary>
    /// <param name="endpoint">The API endpoint (relative to the base URL).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The JSON response as a JsonDocument.</returns>
    Task<JsonDocument> ExecuteWebApiAsync(string endpoint, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a Web API GET request and returns deserialized results.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="endpoint">The API endpoint (relative to the base URL).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deserialized response.</returns>
    Task<T> ExecuteAsync<T>(string endpoint, CancellationToken cancellationToken = default);
}
