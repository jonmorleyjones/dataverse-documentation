using System.Text.Json;
using System.Text.Json.Serialization;

namespace DataverseDoc.Renderers;

/// <summary>
/// Renders output as JSON.
/// </summary>
public class JsonRenderer : IOutputRenderer, ISingleOutputRenderer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <inheritdoc />
    public void Render<T>(IEnumerable<T> data, TextWriter output)
    {
        var json = JsonSerializer.Serialize(data, Options);
        output.WriteLine(json);
    }

    /// <inheritdoc />
    public void Render<T>(T data, TextWriter output)
    {
        var json = JsonSerializer.Serialize(data, Options);
        output.WriteLine(json);
    }
}
