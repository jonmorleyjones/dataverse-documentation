using DataverseDoc.Core.Configuration;

namespace DataverseDoc.Renderers;

/// <summary>
/// Factory for creating output renderers based on format.
/// </summary>
public static class OutputRendererFactory
{
    /// <summary>
    /// Creates an output renderer for the specified format.
    /// </summary>
    /// <param name="format">The output format.</param>
    /// <returns>The appropriate renderer.</returns>
    public static IOutputRenderer Create(OutputFormat format)
    {
        return format switch
        {
            OutputFormat.Table => new TableRenderer(),
            OutputFormat.Json => new JsonRenderer(),
            OutputFormat.Markdown => new MarkdownRenderer(),
            OutputFormat.Mermaid => throw new NotSupportedException(
                "Mermaid format requires ISingleOutputRenderer. Use CreateSingle instead."),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unsupported output format.")
        };
    }

    /// <summary>
    /// Creates a single-item output renderer for the specified format.
    /// </summary>
    /// <param name="format">The output format.</param>
    /// <returns>The appropriate renderer.</returns>
    public static ISingleOutputRenderer CreateSingle(OutputFormat format)
    {
        return format switch
        {
            OutputFormat.Json => new JsonRenderer(),
            OutputFormat.Mermaid => new MermaidRenderer(),
            _ => throw new NotSupportedException(
                $"Format {format} does not support single-item rendering.")
        };
    }
}
