using DataverseDoc.Core.Configuration;
using DataverseDoc.Dataverse;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Moq;
using Xunit;

namespace DataverseDoc.Dataverse.Tests;

public class MsalAuthenticationServiceTests
{
    private readonly Mock<ILogger<MsalAuthenticationService>> _loggerMock;

    public MsalAuthenticationServiceTests()
    {
        _loggerMock = new Mock<ILogger<MsalAuthenticationService>>();
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new MsalAuthenticationService(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public async Task GetAccessTokenAsync_WithNullOptions_ThrowsArgumentNullException()
    {
        // Arrange
        var service = new MsalAuthenticationService(_loggerMock.Object);

        // Act
        var act = async () => await service.GetAccessTokenAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("options");
    }

    [Fact]
    public async Task GetAccessTokenAsync_WithMissingUrl_ThrowsInvalidOperationException()
    {
        // Arrange
        var service = new MsalAuthenticationService(_loggerMock.Object);
        var options = new DataverseConnectionOptions();

        // Act
        var act = async () => await service.GetAccessTokenAsync(options);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*URL*");
    }

    [Fact]
    public async Task GetAccessTokenAsync_ServicePrincipal_WithMissingTenantId_ThrowsInvalidOperationException()
    {
        // Arrange
        var service = new MsalAuthenticationService(_loggerMock.Object);
        var options = new DataverseConnectionOptions
        {
            Url = "https://org.crm.dynamics.com",
            AuthMode = AuthenticationMode.ServicePrincipal,
            ClientId = "test-client-id",
            ClientSecret = "test-secret"
        };

        // Act
        var act = async () => await service.GetAccessTokenAsync(options);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Tenant ID*");
    }

    [Fact]
    public async Task GetAccessTokenAsync_ServicePrincipal_WithMissingClientId_ThrowsInvalidOperationException()
    {
        // Arrange
        var service = new MsalAuthenticationService(_loggerMock.Object);
        var options = new DataverseConnectionOptions
        {
            Url = "https://org.crm.dynamics.com",
            AuthMode = AuthenticationMode.ServicePrincipal,
            TenantId = "test-tenant-id",
            ClientSecret = "test-secret"
        };

        // Act
        var act = async () => await service.GetAccessTokenAsync(options);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Client ID*");
    }

    [Fact]
    public async Task GetAccessTokenAsync_ServicePrincipal_WithMissingCredentials_ThrowsInvalidOperationException()
    {
        // Arrange
        var service = new MsalAuthenticationService(_loggerMock.Object);
        var options = new DataverseConnectionOptions
        {
            Url = "https://org.crm.dynamics.com",
            AuthMode = AuthenticationMode.ServicePrincipal,
            TenantId = "test-tenant-id",
            ClientId = "test-client-id"
        };

        // Act
        var act = async () => await service.GetAccessTokenAsync(options);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*secret*certificate*");
    }

    [Fact]
    public async Task GetAccessTokenAsync_Interactive_WithMissingClientId_ThrowsInvalidOperationException()
    {
        // Arrange
        var service = new MsalAuthenticationService(_loggerMock.Object);
        var options = new DataverseConnectionOptions
        {
            Url = "https://org.crm.dynamics.com",
            AuthMode = AuthenticationMode.Interactive
        };

        // Act
        var act = async () => await service.GetAccessTokenAsync(options);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Client ID*");
    }

    [Fact]
    public async Task GetAccessTokenAsync_Cancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        var service = new MsalAuthenticationService(_loggerMock.Object);
        var options = new DataverseConnectionOptions
        {
            Url = "https://org.crm.dynamics.com",
            AuthMode = AuthenticationMode.Interactive,
            ClientId = "test-client-id"
        };
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var act = async () => await service.GetAccessTokenAsync(options, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public void GetScope_ReturnsCorrectDataverseScope()
    {
        // Arrange
        var service = new MsalAuthenticationService(_loggerMock.Object);
        var url = "https://myorg.crm.dynamics.com";

        // Act
        var scope = service.GetScope(url);

        // Assert
        scope.Should().Be("https://myorg.crm.dynamics.com/.default");
    }

    [Fact]
    public void GetScope_WithTrailingSlash_ReturnsCorrectScope()
    {
        // Arrange
        var service = new MsalAuthenticationService(_loggerMock.Object);
        var url = "https://myorg.crm.dynamics.com/";

        // Act
        var scope = service.GetScope(url);

        // Assert
        scope.Should().Be("https://myorg.crm.dynamics.com/.default");
    }

    [Theory]
    [InlineData("https://org.crm.dynamics.com", "https://org.crm.dynamics.com/.default")]
    [InlineData("https://org.crm4.dynamics.com", "https://org.crm4.dynamics.com/.default")]
    [InlineData("https://org.crm.dynamics.cn", "https://org.crm.dynamics.cn/.default")]
    public void GetScope_VariousRegions_ReturnsCorrectScope(string url, string expectedScope)
    {
        // Arrange
        var service = new MsalAuthenticationService(_loggerMock.Object);

        // Act
        var scope = service.GetScope(url);

        // Assert
        scope.Should().Be(expectedScope);
    }
}

public class MsalAuthenticationServiceIntegrationTests
{
    // These tests require actual Azure AD configuration and should be skipped in CI
    // They serve as documentation for how the authentication should work

    [Fact(Skip = "Integration test - requires actual Azure AD configuration")]
    public async Task GetAccessTokenAsync_ServicePrincipal_WithClientSecret_ReturnsToken()
    {
        // This test would require actual Azure AD app registration
        // Arrange
        var logger = new Mock<ILogger<MsalAuthenticationService>>().Object;
        var service = new MsalAuthenticationService(logger);
        var options = new DataverseConnectionOptions
        {
            Url = "https://org.crm.dynamics.com",
            AuthMode = AuthenticationMode.ServicePrincipal,
            TenantId = "your-tenant-id",
            ClientId = "your-client-id",
            ClientSecret = "your-client-secret"
        };

        // Act
        var token = await service.GetAccessTokenAsync(options);

        // Assert
        token.Should().NotBeNullOrEmpty();
    }

    [Fact(Skip = "Integration test - requires actual Azure AD configuration")]
    public async Task GetAccessTokenAsync_Interactive_ReturnsToken()
    {
        // This test would require user interaction
        // Arrange
        var logger = new Mock<ILogger<MsalAuthenticationService>>().Object;
        var service = new MsalAuthenticationService(logger);
        var options = new DataverseConnectionOptions
        {
            Url = "https://org.crm.dynamics.com",
            AuthMode = AuthenticationMode.Interactive,
            ClientId = "your-client-id"
        };

        // Act
        var token = await service.GetAccessTokenAsync(options);

        // Assert
        token.Should().NotBeNullOrEmpty();
    }
}
