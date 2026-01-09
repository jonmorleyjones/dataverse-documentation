using DataverseDoc.Core.Configuration;
using DataverseDoc.Dataverse;
using FluentAssertions;
using Moq;
using Xunit;

namespace DataverseDoc.Dataverse.Tests;

public class AuthenticationServiceTests
{
    [Fact]
    public async Task IAuthenticationService_CanBeMocked()
    {
        // Arrange
        var mockService = new Mock<IAuthenticationService>();
        var expectedToken = "test-access-token";

        mockService.Setup(s => s.GetAccessTokenAsync(
                It.IsAny<DataverseConnectionOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedToken);

        var options = new DataverseConnectionOptions
        {
            Url = "https://org.crm.dynamics.com"
        };

        // Act
        var result = await mockService.Object.GetAccessTokenAsync(options);

        // Assert
        result.Should().Be(expectedToken);
    }

    [Fact]
    public async Task IAuthenticationService_CanReturnDifferentTokensBasedOnOptions()
    {
        // Arrange
        var mockService = new Mock<IAuthenticationService>();

        mockService.Setup(s => s.GetAccessTokenAsync(
                It.Is<DataverseConnectionOptions>(o => o.AuthMode == AuthenticationMode.Interactive),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("interactive-token");

        mockService.Setup(s => s.GetAccessTokenAsync(
                It.Is<DataverseConnectionOptions>(o => o.AuthMode == AuthenticationMode.ServicePrincipal),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("service-principal-token");

        // Act
        var interactiveResult = await mockService.Object.GetAccessTokenAsync(
            new DataverseConnectionOptions { AuthMode = AuthenticationMode.Interactive });

        var spResult = await mockService.Object.GetAccessTokenAsync(
            new DataverseConnectionOptions { AuthMode = AuthenticationMode.ServicePrincipal });

        // Assert
        interactiveResult.Should().Be("interactive-token");
        spResult.Should().Be("service-principal-token");
    }

    [Fact]
    public async Task IAuthenticationService_SupportsCancellation()
    {
        // Arrange
        var mockService = new Mock<IAuthenticationService>();
        var cts = new CancellationTokenSource();

        mockService.Setup(s => s.GetAccessTokenAsync(
                It.IsAny<DataverseConnectionOptions>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act
        var act = async () => await mockService.Object.GetAccessTokenAsync(
            new DataverseConnectionOptions(),
            cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
