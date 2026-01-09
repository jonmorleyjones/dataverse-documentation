namespace DataverseDoc.Core.Configuration;

/// <summary>
/// Supported output formats for command results.
/// </summary>
public enum OutputFormat
{
    /// <summary>
    /// Console table format (default).
    /// </summary>
    Table,

    /// <summary>
    /// JSON format.
    /// </summary>
    Json,

    /// <summary>
    /// Markdown format.
    /// </summary>
    Markdown,

    /// <summary>
    /// Mermaid diagram format.
    /// </summary>
    Mermaid
}
