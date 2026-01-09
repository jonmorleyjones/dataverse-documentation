using System.CommandLine;
using System.CommandLine.Invocation;
using DataverseDoc.Core;
using DataverseDoc.Core.Configuration;
using DataverseDoc.Dataverse;
using DataverseDoc.Dataverse.Readers;
using DataverseDoc.Renderers;
using Microsoft.Extensions.Logging;

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

        command.SetHandler(async (context) =>
        {
            var solution = context.ParseResult.GetValueForOption(solutionOption)!;
            var exitCode = await ExecuteAsync(context, solution);
            context.ExitCode = exitCode;
        });

        return command;
    }

    private static async Task<int> ExecuteAsync(InvocationContext context, string solutionName)
    {
        // Get global options
        var url = GetGlobalOption<string?>(context, "--url");
        var authMode = GetGlobalOption<string>(context, "--auth-mode") ?? "interactive";
        var tenantId = GetGlobalOption<string?>(context, "--tenant-id");
        var clientId = GetGlobalOption<string?>(context, "--client-id");
        var clientSecret = GetGlobalOption<string?>(context, "--client-secret");
        var output = GetGlobalOption<string>(context, "--output") ?? "table";
        var outputFile = GetGlobalOption<string?>(context, "--output-file");
        var verbose = GetGlobalOption<bool>(context, "--verbose");

        // Create logger
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(verbose ? LogLevel.Debug : LogLevel.Warning);
        });

        var logger = loggerFactory.CreateLogger("envvars");

        try
        {
            // Build connection options
            var configLoader = new ConfigurationLoader();
            var cliOptions = new DataverseConnectionOptions
            {
                Url = url,
                AuthMode = ParseAuthMode(authMode),
                TenantId = tenantId,
                ClientId = clientId,
                ClientSecret = clientSecret
            };
            var connectionOptions = configLoader.Load(cliOptions);

            // Validate required options
            if (string.IsNullOrWhiteSpace(connectionOptions.Url))
            {
                Console.Error.WriteLine("Error: Dataverse URL is required. Use --url or set DATAVERSE_URL environment variable.");
                return ExitCodes.InvalidArguments;
            }

            if (string.IsNullOrWhiteSpace(connectionOptions.ClientId))
            {
                Console.Error.WriteLine("Error: Client ID is required. Use --client-id or set DATAVERSE_CLIENT_ID environment variable.");
                return ExitCodes.InvalidArguments;
            }

            // Create services
            var authService = new MsalAuthenticationService(
                loggerFactory.CreateLogger<MsalAuthenticationService>());

            using var dataverseClient = new DataverseClient(
                loggerFactory.CreateLogger<DataverseClient>(),
                authService,
                connectionOptions);

            var reader = new EnvironmentVariableReader(
                loggerFactory.CreateLogger<EnvironmentVariableReader>(),
                dataverseClient);

            // Execute query
            logger.LogInformation("Retrieving environment variables for solution: {Solution}", solutionName);
            var envVars = await reader.GetEnvironmentVariablesAsync(solutionName, context.GetCancellationToken());

            // Render output
            var outputFormat = ParseOutputFormat(output);
            var renderer = OutputRendererFactory.Create(outputFormat);

            using TextWriter writer = outputFile != null
                ? new StreamWriter(outputFile)
                : Console.Out;

            renderer.Render(envVars, writer);

            if (outputFile != null)
            {
                Console.WriteLine($"Output written to: {outputFile}");
            }

            return ExitCodes.Success;
        }
        catch (DataverseException ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            logger.LogError(ex, "Dataverse error occurred");
            return ex.StatusCode switch
            {
                System.Net.HttpStatusCode.NotFound => ExitCodes.ResourceNotFound,
                System.Net.HttpStatusCode.Unauthorized => ExitCodes.AuthenticationFailed,
                System.Net.HttpStatusCode.Forbidden => ExitCodes.AccessDenied,
                _ => ExitCodes.GeneralError
            };
        }
        catch (InvalidOperationException ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            logger.LogError(ex, "Configuration error");
            return ExitCodes.AuthenticationFailed;
        }
        catch (OperationCanceledException)
        {
            Console.Error.WriteLine("Operation cancelled.");
            return ExitCodes.GeneralError;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unexpected error: {ex.Message}");
            logger.LogError(ex, "Unexpected error");
            return ExitCodes.GeneralError;
        }
    }

    private static T? GetGlobalOption<T>(InvocationContext context, string name)
    {
        var option = context.ParseResult.CommandResult.Command.Parents
            .SelectMany(p => p.Options)
            .FirstOrDefault(o => o.Aliases.Contains(name));

        if (option != null)
        {
            return context.ParseResult.GetValueForOption((Option<T>)option);
        }

        return default;
    }

    private static AuthenticationMode ParseAuthMode(string mode)
    {
        return mode.ToLowerInvariant() switch
        {
            "serviceprincipal" or "sp" => AuthenticationMode.ServicePrincipal,
            _ => AuthenticationMode.Interactive
        };
    }

    private static OutputFormat ParseOutputFormat(string format)
    {
        return format.ToLowerInvariant() switch
        {
            "json" => OutputFormat.Json,
            "markdown" or "md" => OutputFormat.Markdown,
            _ => OutputFormat.Table
        };
    }
}
