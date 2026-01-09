using System.Net;
using System.Text.Json;
using DataverseDoc.Core.Models;
using Microsoft.Extensions.Logging;

namespace DataverseDoc.Dataverse.Readers;

/// <summary>
/// Reads environment variables from a Dataverse solution.
/// </summary>
public class EnvironmentVariableReader : IEnvironmentVariableReader
{
    private readonly ILogger<EnvironmentVariableReader> _logger;
    private readonly IDataverseClient _dataverseClient;

    // Environment variable type codes
    private static readonly Dictionary<int, string> TypeMap = new()
    {
        { 100000000, "String" },
        { 100000001, "Number" },
        { 100000002, "Boolean" },
        { 100000003, "JSON" },
        { 100000004, "Data Source" },
        { 100000005, "Secret" }
    };

    public EnvironmentVariableReader(
        ILogger<EnvironmentVariableReader> logger,
        IDataverseClient dataverseClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dataverseClient = dataverseClient ?? throw new ArgumentNullException(nameof(dataverseClient));
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<EnvironmentVariable>> GetEnvironmentVariablesAsync(
        string solutionName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(solutionName))
        {
            throw new ArgumentException("Solution name is required.", nameof(solutionName));
        }

        _logger.LogInformation("Retrieving environment variables for solution: {SolutionName}", solutionName);

        // First, get the solution ID
        var solutionId = await GetSolutionIdAsync(solutionName, cancellationToken);
        _logger.LogDebug("Found solution ID: {SolutionId}", solutionId);

        // Get environment variable definitions with their values
        var envVars = await GetEnvironmentVariableDefinitionsAsync(solutionId, cancellationToken);

        _logger.LogInformation("Found {Count} environment variables in solution {SolutionName}",
            envVars.Count, solutionName);

        return envVars;
    }

    private async Task<Guid> GetSolutionIdAsync(string solutionName, CancellationToken cancellationToken)
    {
        var query = $"solutions?$select=solutionid,uniquename&$filter=uniquename eq '{solutionName}'";

        using var response = await _dataverseClient.ExecuteWebApiAsync(query, cancellationToken);

        if (response.RootElement.TryGetProperty("value", out var valueArray) &&
            valueArray.GetArrayLength() > 0)
        {
            var solutionIdStr = valueArray[0].GetProperty("solutionid").GetString();
            return Guid.Parse(solutionIdStr!);
        }

        throw new DataverseException(
            $"Solution '{solutionName}' not found.",
            HttpStatusCode.NotFound);
    }

    private async Task<IReadOnlyList<EnvironmentVariable>> GetEnvironmentVariableDefinitionsAsync(
        Guid solutionId,
        CancellationToken cancellationToken)
    {
        // Query environment variable definitions that are part of the solution
        var query = new ODataQueryBuilder("environmentvariabledefinitions")
            .Select("displayname", "schemaname", "type", "defaultvalue", "environmentvariabledefinitionid")
            .Expand("environmentvariabledefinition_environmentvariablevalue",
                select: new[] { "value" })
            .Build();

        // We need to filter by solution - get solution components first
        var componentQuery = $"solutioncomponents?$select=objectid&$filter=_solutionid_value eq {solutionId} and componenttype eq 380";

        using var componentResponse = await _dataverseClient.ExecuteWebApiAsync(componentQuery, cancellationToken);
        var componentIds = new HashSet<Guid>();

        if (componentResponse.RootElement.TryGetProperty("value", out var components))
        {
            foreach (var component in components.EnumerateArray())
            {
                if (component.TryGetProperty("objectid", out var objectId))
                {
                    componentIds.Add(Guid.Parse(objectId.GetString()!));
                }
            }
        }

        if (componentIds.Count == 0)
        {
            return Array.Empty<EnvironmentVariable>();
        }

        // Now get the environment variable definitions
        using var response = await _dataverseClient.ExecuteWebApiAsync(query, cancellationToken);

        var envVars = new List<EnvironmentVariable>();

        if (response.RootElement.TryGetProperty("value", out var valueArray))
        {
            foreach (var item in valueArray.EnumerateArray())
            {
                var envVarId = Guid.Parse(item.GetProperty("environmentvariabledefinitionid").GetString()!);

                // Filter to only include variables that are in the solution
                if (!componentIds.Contains(envVarId))
                {
                    continue;
                }

                var displayName = GetStringProperty(item, "displayname") ?? "Unknown";
                var schemaName = GetStringProperty(item, "schemaname") ?? "Unknown";
                var typeCode = item.TryGetProperty("type", out var typeElement) ? typeElement.GetInt32() : 100000000;
                var defaultValue = GetStringProperty(item, "defaultvalue");
                var currentValue = GetCurrentValue(item);

                var typeName = TypeMap.TryGetValue(typeCode, out var name) ? name : "Unknown";

                envVars.Add(new EnvironmentVariable(
                    displayName,
                    schemaName,
                    currentValue,
                    defaultValue,
                    typeName));
            }
        }

        return envVars;
    }

    private static string? GetStringProperty(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var property) &&
            property.ValueKind == JsonValueKind.String)
        {
            return property.GetString();
        }
        return null;
    }

    private static string? GetCurrentValue(JsonElement envVarElement)
    {
        if (envVarElement.TryGetProperty("environmentvariabledefinition_environmentvariablevalue", out var values) &&
            values.ValueKind == JsonValueKind.Array &&
            values.GetArrayLength() > 0)
        {
            var firstValue = values[0];
            if (firstValue.TryGetProperty("value", out var valueProperty))
            {
                return valueProperty.GetString();
            }
        }
        return null;
    }
}
