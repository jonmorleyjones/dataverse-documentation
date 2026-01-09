using DataverseDoc.Core.Models;

namespace DataverseDoc.Dataverse.Readers;

/// <summary>
/// Reader for extracting cloud flows from a Dataverse solution.
/// </summary>
public interface ICloudFlowReader
{
    /// <summary>
    /// Gets all cloud flows in the specified solution.
    /// </summary>
    /// <param name="solutionName">The unique name of the solution.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of cloud flows.</returns>
    Task<IReadOnlyList<CloudFlowInfo>> GetCloudFlowsAsync(
        string solutionName,
        CancellationToken cancellationToken = default);
}
