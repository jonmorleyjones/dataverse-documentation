using DataverseDoc.Core.Models;

namespace DataverseDoc.Dataverse.Readers;

/// <summary>
/// Reader for extracting classic processes from a Dataverse solution.
/// </summary>
public interface IProcessReader
{
    /// <summary>
    /// Gets all classic processes (workflows and BPFs) in the specified solution.
    /// </summary>
    /// <param name="solutionName">The unique name of the solution.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of processes.</returns>
    Task<IReadOnlyList<ProcessInfo>> GetProcessesAsync(
        string solutionName,
        CancellationToken cancellationToken = default);
}
