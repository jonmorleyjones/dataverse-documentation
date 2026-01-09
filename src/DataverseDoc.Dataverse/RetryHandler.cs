using System.Net;
using Microsoft.Extensions.Logging;

namespace DataverseDoc.Dataverse;

/// <summary>
/// HTTP delegating handler that implements retry logic with exponential backoff.
/// Handles transient failures and rate limiting (HTTP 429).
/// </summary>
public class RetryHandler : DelegatingHandler
{
    private readonly ILogger<RetryHandler> _logger;
    private readonly int _maxRetries;
    private readonly int _initialDelayMs;
    private readonly double _backoffMultiplier;

    private static readonly HashSet<HttpStatusCode> RetryableStatusCodes = new()
    {
        HttpStatusCode.TooManyRequests,        // 429
        HttpStatusCode.InternalServerError,    // 500
        HttpStatusCode.BadGateway,             // 502
        HttpStatusCode.ServiceUnavailable,     // 503
        HttpStatusCode.GatewayTimeout          // 504
    };

    /// <summary>
    /// Creates a new RetryHandler with the specified configuration.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="maxRetries">Maximum number of retry attempts. Default is 3.</param>
    /// <param name="initialDelayMs">Initial delay in milliseconds before first retry. Default is 1000.</param>
    /// <param name="backoffMultiplier">Multiplier for exponential backoff. Default is 2.0.</param>
    public RetryHandler(
        ILogger<RetryHandler> logger,
        int maxRetries = 3,
        int initialDelayMs = 1000,
        double backoffMultiplier = 2.0)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (maxRetries < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxRetries), "Max retries must be at least 1.");
        }

        if (initialDelayMs < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(initialDelayMs), "Initial delay must be at least 1ms.");
        }

        _maxRetries = maxRetries;
        _initialDelayMs = initialDelayMs;
        _backoffMultiplier = backoffMultiplier;
    }

    /// <inheritdoc/>
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var attempt = 0;
        HttpResponseMessage? response = null;

        while (attempt <= _maxRetries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                // Clone the request for retry attempts (original request can only be sent once)
                var requestToSend = attempt == 0 ? request : await CloneRequestAsync(request);

                response = await base.SendAsync(requestToSend, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    return response;
                }

                if (!IsRetryableStatusCode(response.StatusCode))
                {
                    _logger.LogDebug(
                        "Request to {Uri} returned non-retryable status {StatusCode}",
                        request.RequestUri,
                        response.StatusCode);
                    return response;
                }

                if (attempt >= _maxRetries)
                {
                    _logger.LogWarning(
                        "Request to {Uri} failed with status {StatusCode} after {Attempts} attempts",
                        request.RequestUri,
                        response.StatusCode,
                        attempt + 1);
                    break;
                }

                var delay = CalculateDelay(response, attempt);
                _logger.LogInformation(
                    "Request to {Uri} returned {StatusCode}, retrying in {Delay}ms (attempt {Attempt}/{MaxRetries})",
                    request.RequestUri,
                    response.StatusCode,
                    delay.TotalMilliseconds,
                    attempt + 1,
                    _maxRetries);

                await Task.Delay(delay, cancellationToken);
                attempt++;
            }
            catch (HttpRequestException ex) when (attempt < _maxRetries)
            {
                var delay = CalculateDelay(null, attempt);
                _logger.LogWarning(
                    ex,
                    "Request to {Uri} failed with exception, retrying in {Delay}ms (attempt {Attempt}/{MaxRetries})",
                    request.RequestUri,
                    delay.TotalMilliseconds,
                    attempt + 1,
                    _maxRetries);

                await Task.Delay(delay, cancellationToken);
                attempt++;
            }
        }

        throw new HttpRequestException(
            $"Request to {request.RequestUri} failed after {_maxRetries} retry attempts. " +
            $"Last status code: {response?.StatusCode}");
    }

    private bool IsRetryableStatusCode(HttpStatusCode statusCode)
    {
        return RetryableStatusCodes.Contains(statusCode);
    }

    private TimeSpan CalculateDelay(HttpResponseMessage? response, int attempt)
    {
        // Check for Retry-After header (rate limiting)
        if (response?.Headers.RetryAfter != null)
        {
            if (response.Headers.RetryAfter.Delta.HasValue)
            {
                return response.Headers.RetryAfter.Delta.Value;
            }

            if (response.Headers.RetryAfter.Date.HasValue)
            {
                var retryAfterDate = response.Headers.RetryAfter.Date.Value;
                var delay = retryAfterDate - DateTimeOffset.UtcNow;
                if (delay > TimeSpan.Zero)
                {
                    return delay;
                }
            }
        }

        // Calculate exponential backoff
        var exponentialDelay = _initialDelayMs * Math.Pow(_backoffMultiplier, attempt);

        // Add jitter (±20%) to prevent thundering herd
        var jitter = Random.Shared.NextDouble() * 0.4 - 0.2;
        var delayWithJitter = exponentialDelay * (1 + jitter);

        // Cap at 30 seconds
        return TimeSpan.FromMilliseconds(Math.Min(delayWithJitter, 30000));
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version
        };

        // Copy headers
        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        // Copy content if present
        if (request.Content != null)
        {
            var contentBytes = await request.Content.ReadAsByteArrayAsync();
            clone.Content = new ByteArrayContent(contentBytes);

            foreach (var header in request.Content.Headers)
            {
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        // Copy options
        foreach (var option in request.Options)
        {
            clone.Options.TryAdd(option.Key, option.Value);
        }

        return clone;
    }
}
