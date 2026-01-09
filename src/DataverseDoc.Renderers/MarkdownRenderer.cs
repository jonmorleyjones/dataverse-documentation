namespace DataverseDoc.Renderers;

/// <summary>
/// Renders output as Markdown.
/// </summary>
public class MarkdownRenderer : IOutputRenderer
{
    /// <inheritdoc />
    public void Render<T>(IEnumerable<T> data, TextWriter output)
    {
        var items = data.ToList();
        if (items.Count == 0)
        {
            output.WriteLine("*No data found.*");
            return;
        }

        var properties = typeof(T).GetProperties();

        // Header row
        output.WriteLine("| " + string.Join(" | ", properties.Select(p => FormatColumnName(p.Name))) + " |");

        // Separator row
        output.WriteLine("| " + string.Join(" | ", properties.Select(_ => "---")) + " |");

        // Data rows
        foreach (var item in items)
        {
            var values = properties.Select(p =>
            {
                var value = p.GetValue(item);
                return FormatValue(value);
            });
            output.WriteLine("| " + string.Join(" | ", values) + " |");
        }
    }

    private static string FormatColumnName(string name)
    {
        var result = new System.Text.StringBuilder();
        foreach (var c in name)
        {
            if (char.IsUpper(c) && result.Length > 0)
            {
                result.Append(' ');
            }
            result.Append(c);
        }
        return result.ToString();
    }

    private static string FormatValue(object? value)
    {
        return value switch
        {
            null => "-",
            bool b => b ? "Yes" : "No",
            Dictionary<string, List<string>> dict => $"{dict.Count} entities",
            IEnumerable<object> list => $"{list.Count()} items",
            _ => EscapeMarkdown(value.ToString() ?? string.Empty)
        };
    }

    private static string EscapeMarkdown(string text)
    {
        return text
            .Replace("|", "\\|")
            .Replace("\n", " ")
            .Replace("\r", "");
    }
}
