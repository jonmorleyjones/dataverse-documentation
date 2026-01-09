namespace DataverseDoc.Core.Models;

/// <summary>
/// Represents a Dataverse security role.
/// </summary>
public record SecurityRole(
    string Name,
    string? BusinessUnitName,
    Dictionary<string, List<string>> PrivilegesByEntity);
