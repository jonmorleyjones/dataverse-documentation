namespace DataverseDoc.Core.Models;

/// <summary>
/// Represents a classic Dataverse process (workflow or business process flow).
/// </summary>
public record ProcessInfo(
    string Name,
    string Type,
    string Status);
