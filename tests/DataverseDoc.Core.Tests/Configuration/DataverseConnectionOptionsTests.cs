using DataverseDoc.Core.Configuration;
using FluentAssertions;
using Xunit;

namespace DataverseDoc.Core.Tests.Configuration;

public class DataverseConnectionOptionsTests
{
    [Fact]
    public void Validate_WithMissingUrl_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DataverseConnectionOptions
        {
            Url = null
        };

        // Act
        var act = () => options.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*URL*required*");
    }

    [Fact]
    public void Validate_WithEmptyUrl_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DataverseConnectionOptions
        {
            Url = "  "
        };

        // Act
        var act = () => options.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*URL*required*");
    }

    [Fact]
    public void Validate_WithInteractiveMode_Succeeds()
    {
        // Arrange
        var options = new DataverseConnectionOptions
        {
            Url = "https://org.crm.dynamics.com",
            AuthMode = AuthenticationMode.Interactive
        };

        // Act
        var act = () => options.Validate();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_WithServicePrincipal_MissingTenantId_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DataverseConnectionOptions
        {
            Url = "https://org.crm.dynamics.com",
            AuthMode = AuthenticationMode.ServicePrincipal,
            ClientId = "client-id",
            ClientSecret = "secret"
        };

        // Act
        var act = () => options.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Tenant ID*required*");
    }

    [Fact]
    public void Validate_WithServicePrincipal_MissingClientId_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DataverseConnectionOptions
        {
            Url = "https://org.crm.dynamics.com",
            AuthMode = AuthenticationMode.ServicePrincipal,
            TenantId = "tenant-id",
            ClientSecret = "secret"
        };

        // Act
        var act = () => options.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Client ID*required*");
    }

    [Fact]
    public void Validate_WithServicePrincipal_MissingCredentials_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DataverseConnectionOptions
        {
            Url = "https://org.crm.dynamics.com",
            AuthMode = AuthenticationMode.ServicePrincipal,
            TenantId = "tenant-id",
            ClientId = "client-id"
        };

        // Act
        var act = () => options.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*secret*certificate*");
    }

    [Fact]
    public void Validate_WithServicePrincipal_WithSecret_Succeeds()
    {
        // Arrange
        var options = new DataverseConnectionOptions
        {
            Url = "https://org.crm.dynamics.com",
            AuthMode = AuthenticationMode.ServicePrincipal,
            TenantId = "tenant-id",
            ClientId = "client-id",
            ClientSecret = "secret"
        };

        // Act
        var act = () => options.Validate();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_WithServicePrincipal_WithCertificate_Succeeds()
    {
        // Arrange
        var options = new DataverseConnectionOptions
        {
            Url = "https://org.crm.dynamics.com",
            AuthMode = AuthenticationMode.ServicePrincipal,
            TenantId = "tenant-id",
            ClientId = "client-id",
            CertificatePath = "/path/to/cert.pfx"
        };

        // Act
        var act = () => options.Validate();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void DefaultAuthMode_IsInteractive()
    {
        // Arrange & Act
        var options = new DataverseConnectionOptions();

        // Assert
        options.AuthMode.Should().Be(AuthenticationMode.Interactive);
    }
}
