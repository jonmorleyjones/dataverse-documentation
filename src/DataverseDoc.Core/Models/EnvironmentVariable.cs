namespace DataverseDoc.Core.Models;

/// <summary>
/// Represents a Dataverse environment variable.
/// </summary>
public record EnvironmentVariable(
    string DisplayName,
    string SchemaName,
    string? CurrentValue,
    string? DefaultValue,
    string Type);
