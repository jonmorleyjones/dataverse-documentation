using DataverseDoc.Core.Models;

namespace DataverseDoc.Dataverse.Readers;

/// <summary>
/// Reader for extracting security roles from a Dataverse solution.
/// </summary>
public interface ISecurityRoleReader
{
    /// <summary>
    /// Gets all security roles in the specified solution.
    /// </summary>
    /// <param name="solutionName">The unique name of the solution.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of security roles.</returns>
    Task<IReadOnlyList<SecurityRole>> GetSecurityRolesAsync(
        string solutionName,
        CancellationToken cancellationToken = default);
}
