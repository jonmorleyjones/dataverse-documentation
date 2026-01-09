using System.CommandLine;
using DataverseDoc.Cli.Commands;

namespace DataverseDoc.Cli;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Dataverse Documentation CLI - Document Microsoft Dataverse solutions");

        // Add global options
        var urlOption = new Option<string?>(
            aliases: ["--url", "-u"],
            description: "The Dataverse environment URL (e.g., https://org.crm.dynamics.com)")
        {
            IsRequired = false
        };

        var authModeOption = new Option<string>(
            aliases: ["--auth-mode", "-a"],
            description: "Authentication mode: interactive or serviceprincipal")
        {
            IsRequired = false
        };
        authModeOption.SetDefaultValue("interactive");

        var tenantIdOption = new Option<string?>(
            aliases: ["--tenant-id", "-t"],
            description: "Azure AD tenant ID");

        var clientIdOption = new Option<string?>(
            aliases: ["--client-id", "-c"],
            description: "Application (client) ID");

        var clientSecretOption = new Option<string?>(
            aliases: ["--client-secret"],
            description: "Client secret for service principal authentication");

        var outputOption = new Option<string>(
            aliases: ["--output", "-o"],
            description: "Output format: table, json, markdown, or mermaid");
        outputOption.SetDefaultValue("table");

        var outputFileOption = new Option<string?>(
            aliases: ["--output-file", "-f"],
            description: "Write output to file instead of console");

        var verboseOption = new Option<bool>(
            aliases: ["--verbose", "-v"],
            description: "Enable verbose logging");

        // Add global options to root command
        rootCommand.AddGlobalOption(urlOption);
        rootCommand.AddGlobalOption(authModeOption);
        rootCommand.AddGlobalOption(tenantIdOption);
        rootCommand.AddGlobalOption(clientIdOption);
        rootCommand.AddGlobalOption(clientSecretOption);
        rootCommand.AddGlobalOption(outputOption);
        rootCommand.AddGlobalOption(outputFileOption);
        rootCommand.AddGlobalOption(verboseOption);

        // Add subcommands
        rootCommand.AddCommand(EnvironmentVariablesCommand.Create());
        rootCommand.AddCommand(SecurityRolesCommand.Create());
        rootCommand.AddCommand(QueuesCommand.Create());
        rootCommand.AddCommand(EntityDiagramCommand.Create());
        rootCommand.AddCommand(OptionSetsCommand.Create());
        rootCommand.AddCommand(ProcessesCommand.Create());
        rootCommand.AddCommand(CloudFlowsCommand.Create());
        rootCommand.AddCommand(FlowDiagramCommand.Create());

        return await rootCommand.InvokeAsync(args);
    }
}
