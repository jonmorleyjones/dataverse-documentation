namespace DataverseDoc.Core.Models;

/// <summary>
/// Represents a Dataverse queue.
/// </summary>
public record QueueInfo(
    string Name,
    string Type,
    bool EmailEnabled);
