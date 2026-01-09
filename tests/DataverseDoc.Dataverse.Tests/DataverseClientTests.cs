using System.Net;
using System.Text.Json;
using DataverseDoc.Core.Configuration;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DataverseDoc.Dataverse.Tests;

public class DataverseClientTests
{
    private readonly Mock<ILogger<DataverseClient>> _loggerMock;
    private readonly Mock<IAuthenticationService> _authServiceMock;

    public DataverseClientTests()
    {
        _loggerMock = new Mock<ILogger<DataverseClient>>();
        _authServiceMock = new Mock<IAuthenticationService>();
        _authServiceMock.Setup(a => a.GetAccessTokenAsync(
                It.IsAny<DataverseConnectionOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("test-access-token");
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        var options = CreateValidOptions();

        // Act
        var act = () => new DataverseClient(null!, _authServiceMock.Object, options);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public void Constructor_WithNullAuthService_ThrowsArgumentNullException()
    {
        // Arrange
        var options = CreateValidOptions();

        // Act
        var act = () => new DataverseClient(_loggerMock.Object, null!, options);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("authenticationService");
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new DataverseClient(_loggerMock.Object, _authServiceMock.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("options");
    }

    [Fact]
    public void Constructor_WithMissingUrl_ThrowsArgumentException()
    {
        // Arrange
        var options = new DataverseConnectionOptions();

        // Act
        var act = () => new DataverseClient(_loggerMock.Object, _authServiceMock.Object, options);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*URL*");
    }

    [Fact]
    public async Task ExecuteWebApiAsync_ValidEndpoint_ReturnsJsonDocument()
    {
        // Arrange
        var expectedJson = """{"value": [{"name": "test"}]}""";
        var handler = new TestHttpMessageHandler(HttpStatusCode.OK, expectedJson);
        var options = CreateValidOptions();

        var client = new DataverseClient(
            _loggerMock.Object,
            _authServiceMock.Object,
            options,
            handler);

        // Act
        var result = await client.ExecuteWebApiAsync("accounts");

        // Assert
        result.Should().NotBeNull();
        result.RootElement.GetProperty("value").GetArrayLength().Should().Be(1);
    }

    [Fact]
    public async Task ExecuteWebApiAsync_SetsAuthorizationHeader()
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;
        var handler = new TestHttpMessageHandler(req =>
        {
            capturedRequest = req;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}")
            };
        });
        var options = CreateValidOptions();

        var client = new DataverseClient(
            _loggerMock.Object,
            _authServiceMock.Object,
            options,
            handler);

        // Act
        await client.ExecuteWebApiAsync("accounts");

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Headers.Authorization.Should().NotBeNull();
        capturedRequest.Headers.Authorization!.Scheme.Should().Be("Bearer");
        capturedRequest.Headers.Authorization.Parameter.Should().Be("test-access-token");
    }

    [Fact]
    public async Task ExecuteWebApiAsync_SetsCorrectBaseUrl()
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;
        var handler = new TestHttpMessageHandler(req =>
        {
            capturedRequest = req;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}")
            };
        });
        var options = CreateValidOptions();
        options.Url = "https://myorg.crm.dynamics.com";

        var client = new DataverseClient(
            _loggerMock.Object,
            _authServiceMock.Object,
            options,
            handler);

        // Act
        await client.ExecuteWebApiAsync("accounts?$top=10");

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.RequestUri!.ToString().Should()
            .Be("https://myorg.crm.dynamics.com/api/data/v9.2/accounts?$top=10");
    }

    [Fact]
    public async Task ExecuteWebApiAsync_SetsODataHeaders()
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;
        var handler = new TestHttpMessageHandler(req =>
        {
            capturedRequest = req;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}")
            };
        });
        var options = CreateValidOptions();

        var client = new DataverseClient(
            _loggerMock.Object,
            _authServiceMock.Object,
            options,
            handler);

        // Act
        await client.ExecuteWebApiAsync("accounts");

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Headers.Accept.Should().Contain(h => h.MediaType == "application/json");
        capturedRequest.Headers.GetValues("OData-MaxVersion").Should().Contain("4.0");
        capturedRequest.Headers.GetValues("OData-Version").Should().Contain("4.0");
    }

    [Fact]
    public async Task ExecuteAsync_DeserializesToType()
    {
        // Arrange
        var expectedJson = """{"value": [{"name": "Account1"}, {"name": "Account2"}]}""";
        var handler = new TestHttpMessageHandler(HttpStatusCode.OK, expectedJson);
        var options = CreateValidOptions();

        var client = new DataverseClient(
            _loggerMock.Object,
            _authServiceMock.Object,
            options,
            handler);

        // Act
        var result = await client.ExecuteAsync<ODataResponse<TestAccount>>("accounts");

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        result.Value[0].Name.Should().Be("Account1");
    }

    [Fact]
    public async Task ExecuteWebApiAsync_NotFound_ThrowsDataverseException()
    {
        // Arrange
        var errorJson = """{"error": {"code": "0x80040217", "message": "Entity not found"}}""";
        var handler = new TestHttpMessageHandler(HttpStatusCode.NotFound, errorJson);
        var options = CreateValidOptions();

        var client = new DataverseClient(
            _loggerMock.Object,
            _authServiceMock.Object,
            options,
            handler);

        // Act
        var act = async () => await client.ExecuteWebApiAsync("accounts(00000000-0000-0000-0000-000000000000)");

        // Assert
        await act.Should().ThrowAsync<DataverseException>()
            .Where(e => e.StatusCode == HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ExecuteWebApiAsync_Unauthorized_ThrowsDataverseException()
    {
        // Arrange
        var handler = new TestHttpMessageHandler(HttpStatusCode.Unauthorized, "");
        var options = CreateValidOptions();

        var client = new DataverseClient(
            _loggerMock.Object,
            _authServiceMock.Object,
            options,
            handler);

        // Act
        var act = async () => await client.ExecuteWebApiAsync("accounts");

        // Assert
        await act.Should().ThrowAsync<DataverseException>()
            .Where(e => e.StatusCode == HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ExecuteWebApiAsync_Cancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        var handler = new TestHttpMessageHandler(HttpStatusCode.OK, "{}");
        var options = CreateValidOptions();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var client = new DataverseClient(
            _loggerMock.Object,
            _authServiceMock.Object,
            options,
            handler);

        // Act
        var act = async () => await client.ExecuteWebApiAsync("accounts", cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task ExecuteWebApiAsync_HandlesUrlWithTrailingSlash()
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;
        var handler = new TestHttpMessageHandler(req =>
        {
            capturedRequest = req;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}")
            };
        });
        var options = CreateValidOptions();
        options.Url = "https://myorg.crm.dynamics.com/";

        var client = new DataverseClient(
            _loggerMock.Object,
            _authServiceMock.Object,
            options,
            handler);

        // Act
        await client.ExecuteWebApiAsync("accounts");

        // Assert
        capturedRequest!.RequestUri!.ToString().Should()
            .Be("https://myorg.crm.dynamics.com/api/data/v9.2/accounts");
    }

    [Fact]
    public async Task ExecuteWebApiAsync_HandlesEndpointWithLeadingSlash()
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;
        var handler = new TestHttpMessageHandler(req =>
        {
            capturedRequest = req;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}")
            };
        });
        var options = CreateValidOptions();

        var client = new DataverseClient(
            _loggerMock.Object,
            _authServiceMock.Object,
            options,
            handler);

        // Act
        await client.ExecuteWebApiAsync("/accounts");

        // Assert
        capturedRequest!.RequestUri!.ToString().Should()
            .Be("https://org.crm.dynamics.com/api/data/v9.2/accounts");
    }

    private static DataverseConnectionOptions CreateValidOptions()
    {
        return new DataverseConnectionOptions
        {
            Url = "https://org.crm.dynamics.com",
            AuthMode = AuthenticationMode.Interactive,
            ClientId = "test-client-id"
        };
    }

    /// <summary>
    /// Test HTTP message handler for unit testing.
    /// </summary>
    private class TestHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responseFactory;

        public TestHttpMessageHandler(HttpStatusCode statusCode, string content = "{}")
        {
            _responseFactory = _ => new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json")
            };
        }

        public TestHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        {
            _responseFactory = responseFactory;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(_responseFactory(request));
        }
    }

    private class ODataResponse<T>
    {
        public List<T> Value { get; set; } = new();
    }

    private class TestAccount
    {
        public string Name { get; set; } = "";
    }
}
