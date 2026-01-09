using System.Text.Json;
using System.Text.Json.Serialization;

namespace DataverseDoc.Core.Configuration;

/// <summary>
/// Loads configuration from multiple sources with priority:
/// 1. CLI flags (highest)
/// 2. Environment variables
/// 3. Configuration file (lowest)
/// </summary>
public class ConfigurationLoader
{
    private readonly string _configFilePath;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    /// <summary>
    /// Creates a new ConfigurationLoader with the specified config file path.
    /// </summary>
    /// <param name="configFilePath">Path to the config file. Defaults to ~/.dataverse-doc/config.json</param>
    public ConfigurationLoader(string? configFilePath = null)
    {
        _configFilePath = configFilePath ?? GetDefaultConfigPath();
    }

    /// <summary>
    /// Gets the default configuration file path.
    /// </summary>
    public static string GetDefaultConfigPath()
    {
        var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(homeDir, ".dataverse-doc", "config.json");
    }

    /// <summary>
    /// Loads configuration from all sources, merging with priority.
    /// </summary>
    /// <param name="cliOptions">Options from CLI flags (highest priority).</param>
    /// <returns>Merged configuration options.</returns>
    public DataverseConnectionOptions Load(DataverseConnectionOptions? cliOptions = null)
    {
        // Start with defaults
        var result = new DataverseConnectionOptions();

        // Layer 1: Load from config file (lowest priority)
        var fileConfig = LoadFromFile();
        MergeOptions(result, fileConfig);

        // Layer 2: Load from environment variables
        var envConfig = LoadFromEnvironment();
        MergeOptions(result, envConfig);

        // Layer 3: Apply CLI options (highest priority)
        if (cliOptions != null)
        {
            MergeOptions(result, cliOptions);
        }

        return result;
    }

    private DataverseConnectionOptions LoadFromFile()
    {
        var options = new DataverseConnectionOptions();

        if (!File.Exists(_configFilePath))
        {
            return options;
        }

        try
        {
            var json = File.ReadAllText(_configFilePath);
            var fileConfig = JsonSerializer.Deserialize<ConfigFileModel>(json, JsonOptions);

            if (fileConfig != null)
            {
                options.Url = fileConfig.Url;
                options.TenantId = fileConfig.TenantId;
                options.ClientId = fileConfig.ClientId;
                options.ClientSecret = fileConfig.ClientSecret;
                options.CertificatePath = fileConfig.CertificatePath;
                options.CertificatePassword = fileConfig.CertificatePassword;

                if (!string.IsNullOrWhiteSpace(fileConfig.AuthMode))
                {
                    options.AuthMode = ParseAuthMode(fileConfig.AuthMode);
                }
            }
        }
        catch (JsonException)
        {
            // Invalid JSON file - ignore and use defaults
        }

        return options;
    }

    private DataverseConnectionOptions LoadFromEnvironment()
    {
        var options = new DataverseConnectionOptions();

        var url = Environment.GetEnvironmentVariable("DATAVERSE_URL");
        if (!string.IsNullOrWhiteSpace(url))
        {
            options.Url = url;
        }

        var authMode = Environment.GetEnvironmentVariable("DATAVERSE_AUTH_MODE");
        if (!string.IsNullOrWhiteSpace(authMode))
        {
            options.AuthMode = ParseAuthMode(authMode);
        }

        var tenantId = Environment.GetEnvironmentVariable("DATAVERSE_TENANT_ID");
        if (!string.IsNullOrWhiteSpace(tenantId))
        {
            options.TenantId = tenantId;
        }

        var clientId = Environment.GetEnvironmentVariable("DATAVERSE_CLIENT_ID");
        if (!string.IsNullOrWhiteSpace(clientId))
        {
            options.ClientId = clientId;
        }

        var clientSecret = Environment.GetEnvironmentVariable("DATAVERSE_CLIENT_SECRET");
        if (!string.IsNullOrWhiteSpace(clientSecret))
        {
            options.ClientSecret = clientSecret;
        }

        var certPath = Environment.GetEnvironmentVariable("DATAVERSE_CERTIFICATE_PATH");
        if (!string.IsNullOrWhiteSpace(certPath))
        {
            options.CertificatePath = certPath;
        }

        var certPassword = Environment.GetEnvironmentVariable("DATAVERSE_CERTIFICATE_PASSWORD");
        if (!string.IsNullOrWhiteSpace(certPassword))
        {
            options.CertificatePassword = certPassword;
        }

        return options;
    }

    private static void MergeOptions(DataverseConnectionOptions target, DataverseConnectionOptions source)
    {
        if (!string.IsNullOrWhiteSpace(source.Url))
        {
            target.Url = source.Url;
        }

        // Only override AuthMode if it was explicitly set (non-default for env/file, or any value for CLI)
        // Since we can't distinguish, we check if there's also a URL set (indicating intentional config)
        if (source.AuthMode != AuthenticationMode.Interactive || !string.IsNullOrWhiteSpace(source.Url))
        {
            if (source.AuthMode != AuthenticationMode.Interactive)
            {
                target.AuthMode = source.AuthMode;
            }
        }

        if (!string.IsNullOrWhiteSpace(source.TenantId))
        {
            target.TenantId = source.TenantId;
        }

        if (!string.IsNullOrWhiteSpace(source.ClientId))
        {
            target.ClientId = source.ClientId;
        }

        if (!string.IsNullOrWhiteSpace(source.ClientSecret))
        {
            target.ClientSecret = source.ClientSecret;
        }

        if (!string.IsNullOrWhiteSpace(source.CertificatePath))
        {
            target.CertificatePath = source.CertificatePath;
        }

        if (!string.IsNullOrWhiteSpace(source.CertificatePassword))
        {
            target.CertificatePassword = source.CertificatePassword;
        }
    }

    private static AuthenticationMode ParseAuthMode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return AuthenticationMode.Interactive;
        }

        return value.ToLowerInvariant() switch
        {
            "serviceprincipal" => AuthenticationMode.ServicePrincipal,
            "service_principal" => AuthenticationMode.ServicePrincipal,
            "sp" => AuthenticationMode.ServicePrincipal,
            "interactive" => AuthenticationMode.Interactive,
            _ => AuthenticationMode.Interactive
        };
    }

    /// <summary>
    /// Model for the JSON configuration file.
    /// </summary>
    private class ConfigFileModel
    {
        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("authMode")]
        public string? AuthMode { get; set; }

        [JsonPropertyName("tenantId")]
        public string? TenantId { get; set; }

        [JsonPropertyName("clientId")]
        public string? ClientId { get; set; }

        [JsonPropertyName("clientSecret")]
        public string? ClientSecret { get; set; }

        [JsonPropertyName("certificatePath")]
        public string? CertificatePath { get; set; }

        [JsonPropertyName("certificatePassword")]
        public string? CertificatePassword { get; set; }
    }
}
