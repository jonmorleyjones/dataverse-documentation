namespace DataverseDoc.Core.Models;

/// <summary>
/// Represents a Power Automate cloud flow.
/// </summary>
public record CloudFlowInfo(
    string Name,
    string State,
    string? Owner);
