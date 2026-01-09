using System.CommandLine;
using DataverseDoc.Core;

namespace DataverseDoc.Cli.Commands;

/// <summary>
/// Command to list security roles in a solution.
/// </summary>
public static class SecurityRolesCommand
{
    public static Command Create()
    {
        var command = new Command("roles", "List all security roles in a solution");

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
            Console.WriteLine($"Listing security roles for solution: {solution}");
            Console.WriteLine();
            Console.WriteLine("Note: This is a placeholder. Connect to Dataverse to retrieve actual data.");

            context.ExitCode = ExitCodes.Success;
        });

        return command;
    }
}
