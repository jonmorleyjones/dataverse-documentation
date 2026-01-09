using System.Text;
using DataverseDoc.Core.Models;

namespace DataverseDoc.Renderers;

/// <summary>
/// Renders Mermaid diagrams.
/// </summary>
public class MermaidRenderer : ISingleOutputRenderer
{
    /// <inheritdoc />
    public void Render<T>(T data, TextWriter output)
    {
        var mermaid = data switch
        {
            EntityMetadata entity => RenderEntityDiagram(entity),
            _ => throw new NotSupportedException($"Cannot render {typeof(T).Name} as Mermaid diagram.")
        };

        output.WriteLine(mermaid);
    }

    /// <summary>
    /// Renders an entity relationship diagram in Mermaid format.
    /// </summary>
    public static string RenderEntityDiagram(EntityMetadata entity)
    {
        var sb = new StringBuilder();
        sb.AppendLine("erDiagram");

        var processedRelationships = new HashSet<string>();

        foreach (var rel in entity.Relationships)
        {
            var key = $"{rel.ParentEntity}-{rel.ChildEntity}";
            if (processedRelationships.Contains(key))
                continue;

            processedRelationships.Add(key);

            var relationSymbol = rel.RelationshipType switch
            {
                "OneToMany" => "||--o{",
                "ManyToOne" => "}o--||",
                "ManyToMany" => "}o--o{",
                _ => "--"
            };

            var label = rel.RelationshipName ?? "relates to";
            sb.AppendLine($"    {FormatEntityName(rel.ParentEntity)} {relationSymbol} {FormatEntityName(rel.ChildEntity)} : \"{label}\"");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Renders a flow dependency diagram in Mermaid format.
    /// </summary>
    public static string RenderFlowDiagram(IEnumerable<(string Source, string Target)> dependencies)
    {
        var sb = new StringBuilder();
        sb.AppendLine("flowchart TD");

        foreach (var (source, target) in dependencies)
        {
            sb.AppendLine($"    {SanitizeNodeId(source)}[\"{source}\"] --> {SanitizeNodeId(target)}[\"{target}\"]");
        }

        return sb.ToString();
    }

    private static string FormatEntityName(string name)
    {
        return name.ToUpperInvariant().Replace("_", "-");
    }

    private static string SanitizeNodeId(string name)
    {
        return name
            .Replace(" ", "_")
            .Replace("-", "_")
            .Replace(".", "_");
    }
}
