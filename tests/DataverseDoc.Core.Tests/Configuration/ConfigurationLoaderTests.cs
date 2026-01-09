using DataverseDoc.Core.Configuration;
using FluentAssertions;
using System.Text.Json;
using Xunit;

namespace DataverseDoc.Core.Tests.Configuration;

public class ConfigurationLoaderTests : IDisposable
{
    private readonly string _tempConfigDir;
    private readonly string _tempConfigFile;
    private readonly Dictionary<string, string?> _originalEnvVars;

    public ConfigurationLoaderTests()
    {
        _tempConfigDir = Path.Combine(Path.GetTempPath(), $"dataverse-doc-test-{Guid.NewGuid()}");
        _tempConfigFile = Path.Combine(_tempConfigDir, "config.json");
        Directory.CreateDirectory(_tempConfigDir);

        // Save original environment variables
        _originalEnvVars = new Dictionary<string, string?>
        {
            ["DATAVERSE_URL"] = Environment.GetEnvironmentVariable("DATAVERSE_URL"),
            ["DATAVERSE_AUTH_MODE"] = Environment.GetEnvironmentVariable("DATAVERSE_AUTH_MODE"),
            ["DATAVERSE_TENANT_ID"] = Environment.GetEnvironmentVariable("DATAVERSE_TENANT_ID"),
            ["DATAVERSE_CLIENT_ID"] = Environment.GetEnvironmentVariable("DATAVERSE_CLIENT_ID"),
            ["DATAVERSE_CLIENT_SECRET"] = Environment.GetEnvironmentVariable("DATAVERSE_CLIENT_SECRET"),
            ["DATAVERSE_CERTIFICATE_PATH"] = Environment.GetEnvironmentVariable("DATAVERSE_CERTIFICATE_PATH"),
            ["DATAVERSE_CERTIFICATE_PASSWORD"] = Environment.GetEnvironmentVariable("DATAVERSE_CERTIFICATE_PASSWORD")
        };

        // Clear environment variables for tests
        ClearEnvironmentVariables();
    }

    public void Dispose()
    {
        // Restore original environment variables
        foreach (var kvp in _originalEnvVars)
        {
            Environment.SetEnvironmentVariable(kvp.Key, kvp.Value);
        }

        // Clean up temp directory
        if (Directory.Exists(_tempConfigDir))
        {
            Directory.Delete(_tempConfigDir, true);
        }
    }

    private void ClearEnvironmentVariables()
    {
        Environment.SetEnvironmentVariable("DATAVERSE_URL", null);
        Environment.SetEnvironmentVariable("DATAVERSE_AUTH_MODE", null);
        Environment.SetEnvironmentVariable("DATAVERSE_TENANT_ID", null);
        Environment.SetEnvironmentVariable("DATAVERSE_CLIENT_ID", null);
        Environment.SetEnvironmentVariable("DATAVERSE_CLIENT_SECRET", null);
        Environment.SetEnvironmentVariable("DATAVERSE_CERTIFICATE_PATH", null);
        Environment.SetEnvironmentVariable("DATAVERSE_CERTIFICATE_PASSWORD", null);
    }

    [Fact]
    public void Load_WithNoConfiguration_ReturnsDefaultOptions()
    {
        // Arrange
        var loader = new ConfigurationLoader(configFilePath: _tempConfigFile);

        // Act
        var options = loader.Load();

        // Assert
        options.Should().NotBeNull();
        options.Url.Should().BeNull();
        options.AuthMode.Should().Be(AuthenticationMode.Interactive);
        options.TenantId.Should().BeNull();
        options.ClientId.Should().BeNull();
        options.ClientSecret.Should().BeNull();
    }

    [Fact]
    public void Load_WithConfigFile_LoadsValues()
    {
        // Arrange
        var config = new
        {
            url = "https://org-from-file.crm.dynamics.com",
            authMode = "serviceprincipal",
            tenantId = "tenant-from-file",
            clientId = "client-from-file"
        };
        File.WriteAllText(_tempConfigFile, JsonSerializer.Serialize(config));

        var loader = new ConfigurationLoader(configFilePath: _tempConfigFile);

        // Act
        var options = loader.Load();

        // Assert
        options.Url.Should().Be("https://org-from-file.crm.dynamics.com");
        options.AuthMode.Should().Be(AuthenticationMode.ServicePrincipal);
        options.TenantId.Should().Be("tenant-from-file");
        options.ClientId.Should().Be("client-from-file");
    }

    [Fact]
    public void Load_WithEnvironmentVariables_LoadsValues()
    {
        // Arrange
        Environment.SetEnvironmentVariable("DATAVERSE_URL", "https://org-from-env.crm.dynamics.com");
        Environment.SetEnvironmentVariable("DATAVERSE_AUTH_MODE", "serviceprincipal");
        Environment.SetEnvironmentVariable("DATAVERSE_TENANT_ID", "tenant-from-env");
        Environment.SetEnvironmentVariable("DATAVERSE_CLIENT_ID", "client-from-env");
        Environment.SetEnvironmentVariable("DATAVERSE_CLIENT_SECRET", "secret-from-env");

        var loader = new ConfigurationLoader(configFilePath: _tempConfigFile);

        // Act
        var options = loader.Load();

        // Assert
        options.Url.Should().Be("https://org-from-env.crm.dynamics.com");
        options.AuthMode.Should().Be(AuthenticationMode.ServicePrincipal);
        options.TenantId.Should().Be("tenant-from-env");
        options.ClientId.Should().Be("client-from-env");
        options.ClientSecret.Should().Be("secret-from-env");
    }

    [Fact]
    public void Load_EnvironmentVariables_OverrideConfigFile()
    {
        // Arrange
        var config = new
        {
            url = "https://org-from-file.crm.dynamics.com",
            tenantId = "tenant-from-file",
            clientId = "client-from-file"
        };
        File.WriteAllText(_tempConfigFile, JsonSerializer.Serialize(config));

        Environment.SetEnvironmentVariable("DATAVERSE_URL", "https://org-from-env.crm.dynamics.com");
        Environment.SetEnvironmentVariable("DATAVERSE_CLIENT_ID", "client-from-env");

        var loader = new ConfigurationLoader(configFilePath: _tempConfigFile);

        // Act
        var options = loader.Load();

        // Assert
        // Environment variables should override config file
        options.Url.Should().Be("https://org-from-env.crm.dynamics.com");
        options.ClientId.Should().Be("client-from-env");
        // Config file values should remain where not overridden
        options.TenantId.Should().Be("tenant-from-file");
    }

    [Fact]
    public void Load_CliOptions_OverrideAll()
    {
        // Arrange
        var config = new
        {
            url = "https://org-from-file.crm.dynamics.com",
            tenantId = "tenant-from-file",
            clientId = "client-from-file"
        };
        File.WriteAllText(_tempConfigFile, JsonSerializer.Serialize(config));

        Environment.SetEnvironmentVariable("DATAVERSE_URL", "https://org-from-env.crm.dynamics.com");
        Environment.SetEnvironmentVariable("DATAVERSE_TENANT_ID", "tenant-from-env");

        var cliOptions = new DataverseConnectionOptions
        {
            Url = "https://org-from-cli.crm.dynamics.com"
        };

        var loader = new ConfigurationLoader(configFilePath: _tempConfigFile);

        // Act
        var options = loader.Load(cliOptions);

        // Assert
        // CLI options should take highest priority
        options.Url.Should().Be("https://org-from-cli.crm.dynamics.com");
        // Environment variables should be next
        options.TenantId.Should().Be("tenant-from-env");
        // Config file values should be lowest priority
        options.ClientId.Should().Be("client-from-file");
    }

    [Theory]
    [InlineData("interactive", AuthenticationMode.Interactive)]
    [InlineData("Interactive", AuthenticationMode.Interactive)]
    [InlineData("INTERACTIVE", AuthenticationMode.Interactive)]
    [InlineData("serviceprincipal", AuthenticationMode.ServicePrincipal)]
    [InlineData("ServicePrincipal", AuthenticationMode.ServicePrincipal)]
    [InlineData("SERVICEPRINCIPAL", AuthenticationMode.ServicePrincipal)]
    public void Load_AuthMode_ParsedCaseInsensitive(string envValue, AuthenticationMode expected)
    {
        // Arrange
        Environment.SetEnvironmentVariable("DATAVERSE_AUTH_MODE", envValue);
        var loader = new ConfigurationLoader(configFilePath: _tempConfigFile);

        // Act
        var options = loader.Load();

        // Assert
        options.AuthMode.Should().Be(expected);
    }

    [Fact]
    public void Load_InvalidAuthMode_DefaultsToInteractive()
    {
        // Arrange
        Environment.SetEnvironmentVariable("DATAVERSE_AUTH_MODE", "invalid");
        var loader = new ConfigurationLoader(configFilePath: _tempConfigFile);

        // Act
        var options = loader.Load();

        // Assert
        options.AuthMode.Should().Be(AuthenticationMode.Interactive);
    }

    [Fact]
    public void Load_CertificateOptions_LoadedFromEnvironment()
    {
        // Arrange
        Environment.SetEnvironmentVariable("DATAVERSE_CERTIFICATE_PATH", "/path/to/cert.pfx");
        Environment.SetEnvironmentVariable("DATAVERSE_CERTIFICATE_PASSWORD", "cert-password");

        var loader = new ConfigurationLoader(configFilePath: _tempConfigFile);

        // Act
        var options = loader.Load();

        // Assert
        options.CertificatePath.Should().Be("/path/to/cert.pfx");
        options.CertificatePassword.Should().Be("cert-password");
    }

    [Fact]
    public void Load_InvalidConfigFile_IgnoredWithDefaults()
    {
        // Arrange
        File.WriteAllText(_tempConfigFile, "{ invalid json }");
        var loader = new ConfigurationLoader(configFilePath: _tempConfigFile);

        // Act
        var options = loader.Load();

        // Assert
        options.Should().NotBeNull();
        options.AuthMode.Should().Be(AuthenticationMode.Interactive);
    }

    [Fact]
    public void GetDefaultConfigPath_ReturnsExpectedPath()
    {
        // Act
        var path = ConfigurationLoader.GetDefaultConfigPath();

        // Assert
        path.Should().Contain(".dataverse-doc");
        path.Should().EndWith("config.json");
    }
}
