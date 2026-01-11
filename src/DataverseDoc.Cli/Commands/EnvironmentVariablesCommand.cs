using System.CommandLine;
using DataverseDoc.Core;

namespace DataverseDoc.Cli.Commands;

/// <summary>
/// Command to list environment variables in a solution.
/// </summary>
public static class EnvironmentVariablesCommand
{
    public static Command Create()
    {
        var command = new Command("envvars", "List all environment variables in a solution");

        var solutionOption = new Option<string>(
            aliases: ["--solution", "-s"],
            description: "The unique name of the solution")
        {
            IsRequired = true
        };

        command.AddOption(solutionOption);

        command.SetHandler((context) =>
        {
            var solution = context.ParseResult.GetValueForOption(solutionOption)!;

            // Get global output option from root command
            var rootCommand = context.ParseResult.RootCommandResult.Command;
            var outputOption = rootCommand.Options.FirstOrDefault(o => o.Name == "output") as Option<string>;
            var output = outputOption != null
                ? context.ParseResult.GetValueForOption(outputOption) ?? "table"
                : "table";

            // TODO: Implement actual data retrieval
            Console.WriteLine($"Listing environment variables for solution: {solution}");
            Console.WriteLine($"Output format: {output}");
            Console.WriteLine();
            Console.WriteLine("Note: This is a placeholder. Connect to Dataverse to retrieve actual data.");

            context.ExitCode = ExitCodes.Success;
        });

        return command;
    }
}
