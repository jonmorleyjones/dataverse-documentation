using System.CommandLine;
using DataverseDoc.Core;

namespace DataverseDoc.Cli.Commands;

/// <summary>
/// Command to list cloud flows in a solution.
/// </summary>
public static class CloudFlowsCommand
{
    public static Command Create()
    {
        var command = new Command("flows", "List all cloud flows in a solution");

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

            // TODO: Implement actual data retrieval
            Console.WriteLine($"Listing cloud flows for solution: {solution}");
            Console.WriteLine();
            Console.WriteLine("Note: This is a placeholder. Connect to Dataverse to retrieve actual data.");

            context.ExitCode = ExitCodes.Success;
        });

        return command;
    }
}
