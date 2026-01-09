using DataverseDoc.Core.Models;

namespace DataverseDoc.Dataverse.Readers;

/// <summary>
/// Reader for extracting option sets from a Dataverse solution.
/// </summary>
public interface IOptionSetReader
{
    /// <summary>
    /// Gets all option sets in the specified solution.
    /// </summary>
    /// <param name="solutionName">The unique name of the solution.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of option sets.</returns>
    Task<IReadOnlyList<OptionSetInfo>> GetOptionSetsAsync(
        string solutionName,
        CancellationToken cancellationToken = default);
}
