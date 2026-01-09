using Spectre.Console;

namespace DataverseDoc.Renderers;

/// <summary>
/// Renders output as a console table using Spectre.Console.
/// </summary>
public class TableRenderer : IOutputRenderer
{
    private readonly IAnsiConsole _console;

    public TableRenderer() : this(AnsiConsole.Console)
    {
    }

    public TableRenderer(IAnsiConsole console)
    {
        _console = console;
    }

    /// <inheritdoc />
    public void Render<T>(IEnumerable<T> data, TextWriter output)
    {
        var items = data.ToList();
        if (items.Count == 0)
        {
            _console.MarkupLine("[yellow]No data found.[/]");
            return;
        }

        var table = new Table();
        table.Border(TableBorder.Rounded);

        var properties = typeof(T).GetProperties();

        // Add columns
        foreach (var prop in properties)
        {
            table.AddColumn(new TableColumn(FormatColumnName(prop.Name)).Centered());
        }

        // Add rows
        foreach (var item in items)
        {
            var values = properties.Select(p =>
            {
                var value = p.GetValue(item);
                return FormatValue(value);
            }).ToArray();

            table.AddRow(values);
        }

        _console.Write(table);
    }

    private static string FormatColumnName(string name)
    {
        // Convert PascalCase to Title Case with spaces
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
            null => "[dim]-[/]",
            bool b => b ? "[green]Yes[/]" : "[red]No[/]",
            Dictionary<string, List<string>> dict => $"{dict.Count} entities",
            IEnumerable<object> list => $"{list.Count()} items",
            _ => Markup.Escape(value.ToString() ?? string.Empty)
        };
    }
}
