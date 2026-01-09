using System.Net;
using System.Text.Json;
using DataverseDoc.Core.Models;
using Microsoft.Extensions.Logging;

namespace DataverseDoc.Dataverse.Readers;

/// <summary>
/// Reads queues from a Dataverse solution.
/// </summary>
public class QueueReader : IQueueReader
{
    private readonly ILogger<QueueReader> _logger;
    private readonly IDataverseClient _dataverseClient;

    // Queue type codes
    private const int PublicQueueType = 1;
    private const int PrivateQueueType = 2;

    // Solution component type for Queue
    private const int QueueComponentType = 2020;

    public QueueReader(
        ILogger<QueueReader> logger,
        IDataverseClient dataverseClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dataverseClient = dataverseClient ?? throw new ArgumentNullException(nameof(dataverseClient));
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<QueueInfo>> GetQueuesAsync(
        string solutionName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(solutionName))
        {
            throw new ArgumentException("Solution name is required.", nameof(solutionName));
        }

        _logger.LogInformation("Retrieving queues for solution: {SolutionName}", solutionName);

        // First, get the solution ID
        var solutionId = await GetSolutionIdAsync(solutionName, cancellationToken);
        _logger.LogDebug("Found solution ID: {SolutionId}", solutionId);

        // Get queue IDs from solution components
        var queueIds = await GetQueueIdsFromSolutionAsync(solutionId, cancellationToken);

        if (queueIds.Count == 0)
        {
            _logger.LogInformation("No queues found in solution {SolutionName}", solutionName);
            return Array.Empty<QueueInfo>();
        }

        // Get queue details
        var queues = await GetQueueDetailsAsync(queueIds, cancellationToken);

        _logger.LogInformation("Found {Count} queues in solution {SolutionName}",
            queues.Count, solutionName);

        return queues;
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

    private async Task<HashSet<Guid>> GetQueueIdsFromSolutionAsync(
        Guid solutionId,
        CancellationToken cancellationToken)
    {
        var query = $"solutioncomponents?$select=objectid&$filter=_solutionid_value eq {solutionId} and componenttype eq {QueueComponentType}";

        using var response = await _dataverseClient.ExecuteWebApiAsync(query, cancellationToken);

        var queueIds = new HashSet<Guid>();

        if (response.RootElement.TryGetProperty("value", out var components))
        {
            foreach (var component in components.EnumerateArray())
            {
                if (component.TryGetProperty("objectid", out var objectId))
                {
                    queueIds.Add(Guid.Parse(objectId.GetString()!));
                }
            }
        }

        return queueIds;
    }

    private async Task<IReadOnlyList<QueueInfo>> GetQueueDetailsAsync(
        HashSet<Guid> queueIds,
        CancellationToken cancellationToken)
    {
        var query = new ODataQueryBuilder("queues")
            .Select("queueid", "name", "queuetypecode", "emailaddress")
            .Build();

        using var response = await _dataverseClient.ExecuteWebApiAsync(query, cancellationToken);

        var queues = new List<QueueInfo>();

        if (response.RootElement.TryGetProperty("value", out var valueArray))
        {
            foreach (var item in valueArray.EnumerateArray())
            {
                var queueId = Guid.Parse(item.GetProperty("queueid").GetString()!);

                // Filter to only include queues that are in the solution
                if (!queueIds.Contains(queueId))
                {
                    continue;
                }

                var name = GetStringProperty(item, "name") ?? "Unknown";
                var typeCode = item.TryGetProperty("queuetypecode", out var typeElement)
                    ? typeElement.GetInt32()
                    : PublicQueueType;
                var emailAddress = GetStringProperty(item, "emailaddress");

                var typeName = typeCode == PrivateQueueType ? "Private" : "Public";
                var emailEnabled = !string.IsNullOrEmpty(emailAddress);

                queues.Add(new QueueInfo(name, typeName, emailEnabled));
            }
        }

        return queues;
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
}
