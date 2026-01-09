using DataverseDoc.Core.Models;

namespace DataverseDoc.Dataverse.Readers;

/// <summary>
/// Reader for extracting environment variables from a Dataverse solution.
/// </summary>
public interface IEnvironmentVariableReader
{
    /// <summary>
    /// Gets all environment variables in the specified solution.
    /// </summary>
    /// <param name="solutionName">The unique name of the solution.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of environment variables.</returns>
    Task<IReadOnlyList<EnvironmentVariable>> GetEnvironmentVariablesAsync(
        string solutionName,
        CancellationToken cancellationToken = default);
}
