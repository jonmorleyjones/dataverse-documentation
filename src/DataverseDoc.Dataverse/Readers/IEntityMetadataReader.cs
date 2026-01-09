using DataverseDoc.Core.Models;

namespace DataverseDoc.Dataverse.Readers;

/// <summary>
/// Reader for extracting entity metadata and relationships.
/// </summary>
public interface IEntityMetadataReader
{
    /// <summary>
    /// Gets entity metadata including relationships up to the specified depth.
    /// </summary>
    /// <param name="entityLogicalName">The logical name of the entity.</param>
    /// <param name="depth">The depth of relationships to include (default 1).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Entity metadata with relationships.</returns>
    Task<EntityMetadata> GetEntityMetadataAsync(
        string entityLogicalName,
        int depth = 1,
        CancellationToken cancellationToken = default);
}
