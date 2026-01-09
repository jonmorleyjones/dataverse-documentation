using System.CommandLine;
using DataverseDoc.Core;

namespace DataverseDoc.Cli.Commands;

/// <summary>
/// Command to list classic processes in a solution.
/// </summary>
public static class ProcessesCommand
{
    public static Command Create()
    {
        var command = new Command("processes", "List all classic processes (workflows and BPFs) in a solution");

        var solutionOption = new Option<string>(
            aliases: ["--solution", "-s"],
            description: "The unique name of the solution")
        {
            IsRequired = true
        };

        command.AddOption(solutionOption);

        command.SetHandler(async (context) =>
        {
            var solution = context.ParseResult.GetValueForOption(solutionOption)!;

            // TODO: Implement actual data retrieval
            Console.WriteLine($"Listing processes for solution: {solution}");
            Console.WriteLine();
            Console.WriteLine("Note: This is a placeholder. Connect to Dataverse to retrieve actual data.");

            context.ExitCode = ExitCodes.Success;
        });

        return command;
    }
}
