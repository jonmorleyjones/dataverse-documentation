using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DataverseDoc.Dataverse.Tests;

public class RetryHandlerTests
{
    private readonly Mock<ILogger<RetryHandler>> _loggerMock;

    public RetryHandlerTests()
    {
        _loggerMock = new Mock<ILogger<RetryHandler>>();
    }

    [Fact]
    public async Task SendAsync_SuccessfulRequest_ReturnsResponse()
    {
        // Arrange
        var handler = new RetryHandler(_loggerMock.Object)
        {
            InnerHandler = new TestHttpMessageHandler(HttpStatusCode.OK)
        };
        var client = new HttpClient(handler);

        // Act
        var response = await client.GetAsync("https://example.com/api/test");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SendAsync_TransientFailureThenSuccess_RetriesAndSucceeds()
    {
        // Arrange
        var responses = new Queue<HttpStatusCode>(new[]
        {
            HttpStatusCode.ServiceUnavailable,
            HttpStatusCode.OK
        });
        var handler = new RetryHandler(_loggerMock.Object, maxRetries: 3, initialDelayMs: 10)
        {
            InnerHandler = new TestHttpMessageHandler(responses)
        };
        var client = new HttpClient(handler);

        // Act
        var response = await client.GetAsync("https://example.com/api/test");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SendAsync_RateLimited_RetriesWithRetryAfterHeader()
    {
        // Arrange
        var responses = new Queue<(HttpStatusCode, TimeSpan?)>(new[]
        {
            (HttpStatusCode.TooManyRequests, TimeSpan.FromMilliseconds(50)),
            (HttpStatusCode.OK, (TimeSpan?)null)
        });
        var handler = new RetryHandler(_loggerMock.Object, maxRetries: 3, initialDelayMs: 10)
        {
            InnerHandler = new TestHttpMessageHandler(responses)
        };
        var client = new HttpClient(handler);

        // Act
        var response = await client.GetAsync("https://example.com/api/test");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SendAsync_ExceedsMaxRetries_ThrowsHttpRequestException()
    {
        // Arrange
        var handler = new RetryHandler(_loggerMock.Object, maxRetries: 2, initialDelayMs: 10)
        {
            InnerHandler = new TestHttpMessageHandler(HttpStatusCode.ServiceUnavailable)
        };
        var client = new HttpClient(handler);

        // Act
        var act = async () => await client.GetAsync("https://example.com/api/test");

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("*retry*");
    }

    [Fact]
    public async Task SendAsync_NonRetryableError_DoesNotRetry()
    {
        // Arrange
        var callCount = 0;
        var handler = new RetryHandler(_loggerMock.Object, maxRetries: 3, initialDelayMs: 10)
        {
            InnerHandler = new TestHttpMessageHandler(() =>
            {
                callCount++;
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            })
        };
        var client = new HttpClient(handler);

        // Act
        var response = await client.GetAsync("https://example.com/api/test");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        callCount.Should().Be(1);
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.GatewayTimeout)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    public async Task SendAsync_RetryableStatusCodes_Retries(HttpStatusCode statusCode)
    {
        // Arrange
        var responses = new Queue<HttpStatusCode>(new[]
        {
            statusCode,
            HttpStatusCode.OK
        });
        var handler = new RetryHandler(_loggerMock.Object, maxRetries: 3, initialDelayMs: 10)
        {
            InnerHandler = new TestHttpMessageHandler(responses)
        };
        var client = new HttpClient(handler);

        // Act
        var response = await client.GetAsync("https://example.com/api/test");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SendAsync_Cancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        var handler = new RetryHandler(_loggerMock.Object, maxRetries: 3, initialDelayMs: 1000)
        {
            InnerHandler = new TestHttpMessageHandler(HttpStatusCode.ServiceUnavailable)
        };
        var client = new HttpClient(handler);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var act = async () => await client.GetAsync("https://example.com/api/test", cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new RetryHandler(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_WithInvalidMaxRetries_ThrowsArgumentException(int maxRetries)
    {
        // Act
        var act = () => new RetryHandler(_loggerMock.Object, maxRetries: maxRetries);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("maxRetries");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_WithInvalidInitialDelay_ThrowsArgumentException(int initialDelayMs)
    {
        // Act
        var act = () => new RetryHandler(_loggerMock.Object, initialDelayMs: initialDelayMs);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("initialDelayMs");
    }

    /// <summary>
    /// Test HTTP message handler for unit testing.
    /// </summary>
    private class TestHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpResponseMessage> _responseFactory;

        public TestHttpMessageHandler(HttpStatusCode statusCode)
        {
            _responseFactory = () => new HttpResponseMessage(statusCode);
        }

        public TestHttpMessageHandler(Queue<HttpStatusCode> statusCodes)
        {
            _responseFactory = () => new HttpResponseMessage(
                statusCodes.Count > 0 ? statusCodes.Dequeue() : HttpStatusCode.OK);
        }

        public TestHttpMessageHandler(Queue<(HttpStatusCode, TimeSpan?)> responses)
        {
            _responseFactory = () =>
            {
                var (statusCode, retryAfter) = responses.Count > 0
                    ? responses.Dequeue()
                    : (HttpStatusCode.OK, null);

                var response = new HttpResponseMessage(statusCode);
                if (retryAfter.HasValue)
                {
                    response.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(retryAfter.Value);
                }
                return response;
            };
        }

        public TestHttpMessageHandler(Func<HttpResponseMessage> responseFactory)
        {
            _responseFactory = responseFactory;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(_responseFactory());
        }
    }
}
