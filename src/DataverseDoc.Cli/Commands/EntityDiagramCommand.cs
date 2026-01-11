using System.CommandLine;
using DataverseDoc.Core;

namespace DataverseDoc.Cli.Commands;

/// <summary>
/// Command to generate an entity relationship diagram.
/// </summary>
public static class EntityDiagramCommand
{
    public static Command Create()
    {
        var command = new Command("entity-diagram", "Generate a Mermaid ER diagram for an entity");

        var entityOption = new Option<string>(
            aliases: ["--entity", "-e"],
            description: "The logical name of the entity")
        {
            IsRequired = true
        };

        var depthOption = new Option<int>(
            aliases: ["--depth", "-d"],
            description: "The depth of relationships to include");
        depthOption.SetDefaultValue(1);

        command.AddOption(entityOption);
        command.AddOption(depthOption);

        command.SetHandler((context) =>
        {
            var entity = context.ParseResult.GetValueForOption(entityOption)!;
            var depth = context.ParseResult.GetValueForOption(depthOption);

            // TODO: Implement actual data retrieval
            Console.WriteLine($"Generating entity diagram for: {entity} (depth: {depth})");
            Console.WriteLine();
            Console.WriteLine("erDiagram");
            Console.WriteLine($"    {entity.ToUpperInvariant()} ||--o{{ RELATED-ENTITY : \"has\"");
            Console.WriteLine();
            Console.WriteLine("Note: This is a placeholder. Connect to Dataverse to retrieve actual data.");

            context.ExitCode = ExitCodes.Success;
        });

        return command;
    }
}
