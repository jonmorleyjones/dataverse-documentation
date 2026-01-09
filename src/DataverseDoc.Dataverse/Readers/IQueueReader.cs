using DataverseDoc.Core.Models;

namespace DataverseDoc.Dataverse.Readers;

/// <summary>
/// Reader for extracting queues from a Dataverse solution.
/// </summary>
public interface IQueueReader
{
    /// <summary>
    /// Gets all queues in the specified solution.
    /// </summary>
    /// <param name="solutionName">The unique name of the solution.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of queues.</returns>
    Task<IReadOnlyList<QueueInfo>> GetQueuesAsync(
        string solutionName,
        CancellationToken cancellationToken = default);
}
