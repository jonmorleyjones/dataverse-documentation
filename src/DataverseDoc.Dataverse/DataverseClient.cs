using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using DataverseDoc.Core.Configuration;
using Microsoft.Extensions.Logging;

namespace DataverseDoc.Dataverse;

/// <summary>
/// Client for interacting with the Dataverse Web API.
/// </summary>
public class DataverseClient : IDataverseClient, IDisposable
{
    private const string ApiVersion = "v9.2";
    private const string ApiPath = "/api/data/";

    private readonly ILogger<DataverseClient> _logger;
    private readonly IAuthenticationService _authenticationService;
    private readonly DataverseConnectionOptions _options;
    private readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;
    private string? _cachedToken;
    private bool _disposed;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Creates a new DataverseClient.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="authenticationService">Authentication service for obtaining tokens.</param>
    /// <param name="options">Connection options.</param>
    public DataverseClient(
        ILogger<DataverseClient> logger,
        IAuthenticationService authenticationService,
        DataverseConnectionOptions options)
        : this(logger, authenticationService, options, null)
    {
    }

    /// <summary>
    /// Creates a new DataverseClient with a custom HTTP message handler (for testing).
    /// </summary>
    internal DataverseClient(
        ILogger<DataverseClient> logger,
        IAuthenticationService authenticationService,
        DataverseConnectionOptions options,
        HttpMessageHandler? handler)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
        _options = options ?? throw new ArgumentNullException(nameof(options));

        if (string.IsNullOrWhiteSpace(options.Url))
        {
            throw new ArgumentException("Dataverse URL is required.", nameof(options));
        }

        if (handler != null)
        {
            _httpClient = new HttpClient(handler);
            _ownsHttpClient = true;
        }
        else
        {
            var retryHandler = new RetryHandler(
                new Logger<RetryHandler>(new LoggerFactory()))
            {
                InnerHandler = new HttpClientHandler()
            };
            _httpClient = new HttpClient(retryHandler);
            _ownsHttpClient = true;
        }

        ConfigureHttpClient();
    }

    private void ConfigureHttpClient()
    {
        var baseUrl = _options.Url!.TrimEnd('/');
        _httpClient.BaseAddress = new Uri($"{baseUrl}{ApiPath}{ApiVersion}/");

        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        _httpClient.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
        _httpClient.DefaultRequestHeaders.Add("OData-Version", "4.0");
        _httpClient.DefaultRequestHeaders.Add("Prefer", "odata.include-annotations=\"*\"");
    }

    /// <inheritdoc/>
    public async Task<JsonDocument> ExecuteWebApiAsync(
        string endpoint,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await EnsureAuthenticatedAsync(cancellationToken);

        var normalizedEndpoint = endpoint.TrimStart('/');
        _logger.LogDebug("Executing Web API request: {Endpoint}", normalizedEndpoint);

        var request = new HttpRequestMessage(HttpMethod.Get, normalizedEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _cachedToken);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await HandleErrorResponseAsync(response, normalizedEndpoint, cancellationToken);
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonDocument.Parse(content);
    }

    /// <inheritdoc/>
    public async Task<T> ExecuteAsync<T>(
        string endpoint,
        CancellationToken cancellationToken = default)
    {
        using var document = await ExecuteWebApiAsync(endpoint, cancellationToken);
        var json = document.RootElement.GetRawText();
        return JsonSerializer.Deserialize<T>(json, JsonOptions)
            ?? throw new DataverseException("Failed to deserialize response.", HttpStatusCode.OK);
    }

    private async Task EnsureAuthenticatedAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_cachedToken))
        {
            _logger.LogDebug("Acquiring access token");
            _cachedToken = await _authenticationService.GetAccessTokenAsync(_options, cancellationToken);
            _logger.LogDebug("Access token acquired");
        }
    }

    private async Task HandleErrorResponseAsync(
        HttpResponseMessage response,
        string endpoint,
        CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        string? errorCode = null;
        string message;

        try
        {
            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("error", out var errorElement))
            {
                if (errorElement.TryGetProperty("code", out var codeElement))
                {
                    errorCode = codeElement.GetString();
                }

                if (errorElement.TryGetProperty("message", out var messageElement))
                {
                    message = messageElement.GetString() ?? response.ReasonPhrase ?? "Unknown error";
                }
                else
                {
                    message = response.ReasonPhrase ?? "Unknown error";
                }
            }
            else
            {
                message = response.ReasonPhrase ?? "Unknown error";
            }
        }
        catch (JsonException)
        {
            message = string.IsNullOrWhiteSpace(content)
                ? response.ReasonPhrase ?? "Unknown error"
                : content;
        }

        _logger.LogError(
            "Dataverse API error: {StatusCode} {ErrorCode} - {Message}",
            response.StatusCode,
            errorCode,
            message);

        throw new DataverseException(
            message,
            response.StatusCode,
            errorCode,
            new Uri(_httpClient.BaseAddress!, endpoint));
    }

    /// <summary>
    /// Disposes the HTTP client if owned by this instance.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes managed resources.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing && _ownsHttpClient)
            {
                _httpClient.Dispose();
            }
            _disposed = true;
        }
    }
}
