namespace DataverseDoc.Core.Models;

/// <summary>
/// Represents an option value within an option set.
/// </summary>
public record OptionValue(int Value, string Label);

/// <summary>
/// Represents a Dataverse option set (global or local).
/// </summary>
public record OptionSetInfo(
    string Name,
    string Type,
    List<OptionValue> Options);
